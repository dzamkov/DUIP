using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// A peer on a UDP network.
    /// </summary>
    public class UDPPeer : Peer
    {
        internal UDPPeer(UDPHub Hub, IPEndPoint EndPoint, int SequenceNumber, int AcknowledgementNumber)
        {
            this._EndPoint = EndPoint;
            this._LastSequenceNumber = AcknowledgementNumber;
            this._InTerminal = new InTerminal(AcknowledgementNumber);
            this._OutTerminal = new OutTerminal(SequenceNumber);
            this._Hub = Hub;
        }

        /// <summary>
        /// Gets the endpoint for this peer.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get
            {
                return this._EndPoint;
            }
        }

        /// <summary>
        /// Gets the terminal that handles incoming messages from this peer.
        /// </summary>
        public InTerminal InTerminal
        {
            get
            {
                return this._InTerminal;
            }
        }

        /// <summary>
        /// Gets the terminal that handles outgoing messages to this peer.
        /// </summary>
        public OutTerminal OutTerminal
        {
            get
            {
                return this._OutTerminal;
            }
        }

        /// <summary>
        /// Gets the hub that manages the connection for this peer.
        /// </summary>
        public UDPHub Hub
        {
            get
            {
                return this._Hub;
            }
        }

        public override void Send(Message Message)
        {
            lock (this) // Prevents two send streams from being open at the same time
            {
                using (Disposable<OutStream> str = this._OutTerminal.Send(this._Hub.Settings.ChunkSize))
                {
                    Message.Write(Message, str);
                }
            }
        }

        public override double RoundTripTime
        {
            get
            {
                return this._RoundTripTime;
            }
        }

        /// <summary>
        /// Processes a chunk of a message sent from this peer.
        /// </summary>
        private void _Process(int SequenceNumber, byte[] Chunk, bool Initial, bool Final)
        {
            if (_After(SequenceNumber, this._LastSequenceNumber))
            {
                this._LastSequenceNumber = SequenceNumber;
            }

            Disposable<InStream> str = null;
            if (this._InTerminal.Process(SequenceNumber, Chunk, Initial, Final, ref str))
            {
                if (this.Receive != null)
                {
                    Message message = Message.Read(str);
                    str.Dispose();

                    this.Receive(this, message);
                }
                else
                {
                    str.Dispose();
                }
            }
        }

        /// <summary>
        /// Determines wether a packet with the given sequence number is likely to be valid (authentic).
        /// </summary>
        private bool _Valid(int SequenceNumber)
        {
            return
                _After(SequenceNumber, this._InTerminal.AcknowledgementNumber) &&
                _After(this._LastSequenceNumber + _ValidThreshold, SequenceNumber);
        }

        /// <summary>
        /// The allowable difference between the last sequence number and the sequence number of the next received
        /// packet in order for the packet to be considered valid.
        /// </summary>
        private const int _ValidThreshold = 64;

        /// <summary>
        /// Determines wether the sequence number A is likely to occur after (or is) the sequence number B.
        /// </summary>
        private static bool _After(int A, int B)
        {
            return A >= B || A < (B ^ int.MinValue);
        }

        /// <summary>
        /// Called when this peer is disconnected, either explicitly (disconnect packet) or implicitly.
        /// </summary>
        internal void _Disconnect()
        {
            if (this.Disconnect != null)
            {
                this.Disconnect(this);
            }
        }

        /// <summary>
        /// Creates a chunk packet to be sent to this peer.
        /// </summary>
        internal void _ConstructChunk(
            PacketFlags AdditionalFlags, int SequenceNumber, byte[] Data, 
            bool Initial, bool Final, OutStream Stream)
        {
            PacketFlags flags = PacketFlags.Chunk;
            if (Initial) flags |= PacketFlags.ChunkInitial;
            if (Final) flags |= PacketFlags.ChunkFinal;
            flags |= AdditionalFlags;

            // Header
            Stream.WriteByte((byte)flags);
            Stream.WriteInt(SequenceNumber);

            // Additional information
            this._WriteAdditional(flags, Stream);

            // Data
            Stream.Write(Data, 0, Data.Length);
        }

        /// <summary>
        /// Creates a keep alive packet to be sent to this peer.
        /// </summary>
        internal void _ConstructKeepAlive(PacketFlags AdditionalFlags, OutStream Stream)
        {
            PacketFlags flags = PacketFlags.Empty;
            flags |= AdditionalFlags;

            // Header
            Stream.WriteByte((byte)flags);
            Stream.WriteInt(this._OutTerminal.SequenceNumber);

            // Additional information
            this._WriteAdditional(flags, Stream);
        }

        /// <summary>
        /// Gets the flags for the additional information to be added to the next sent packet.
        /// </summary>
        private PacketFlags _AdditionalFlags
        {
            get
            {
                PacketFlags flags = PacketFlags.Empty;
                flags |= PacketFlags.Acknowledgement;

                if (this._ShouldSendRoundTripTime)
                {
                    flags |= PacketFlags.RoundTripTime;
                    this._ShouldSendRoundTripTime = false;
                }

                return flags;
            }
        }

        /// <summary>
        /// Writes additional (supplimentary) packet information to the given stream.
        /// </summary>
        private void _WriteAdditional(PacketFlags Flags, OutStream Stream)
        {
            // Acknowledgement number
            if ((Flags & PacketFlags.Acknowledgement) == PacketFlags.Acknowledgement)
            {
                Stream.WriteInt(this._InTerminal.AcknowledgementNumber);
            }

            // Round trip time
            if ((Flags & PacketFlags.RoundTripTime) == PacketFlags.RoundTripTime)
            {
                Stream.WriteDouble(this._RoundTripTime);
            }
        }

        /// <summary>
        /// Reads additional packet information from the given stream.
        /// </summary>
        /// <param name="Size">The size of the remaining bytes in the stream.</param>
        /// <returns>True if the information is valid, false otherwise.</returns>
        private bool _ReadAdditional(PacketFlags Flags, InStream Stream, ref int Size)
        {
            // Acknowledgement number
            if ((Flags & PacketFlags.Acknowledgement) == PacketFlags.Acknowledgement)
            {
                if ((Size -= StreamSize.Int) < 0)
                    return false;
                this._OutTerminal.AcknowledgementNumber = Stream.ReadInt();
            }

            // Round trip time
            if ((Flags & PacketFlags.RoundTripTime) == PacketFlags.RoundTripTime)
            {
                // Assume that the peer is not lying, as it wouldn't be very helpful 
                // to either of us
                if ((Size -= StreamSize.Double) < 0)
                    return false;
                this._RoundTripTime = Stream.ReadDouble();
            }

            return true;
        }

        /// <summary>
        /// Updates the state of the peer and sends packets if needed.
        /// </summary>
        internal void _Update(UDPHubSettings Settings, UDP UDP, double Time, out bool Remove)
        {
            // Send chunk packet if possible
            this._SendDelay -= Time;
            while (this._SendDelay <= 0.0)
            {
                byte[] chunkdata = null;
                bool chunkinitial = false;
                bool chunkfinal = false;
                int chunksequencenumber = 0;
                if (this.OutTerminal.Process(ref chunksequencenumber, ref chunkdata, ref chunkinitial, ref chunkfinal))
                {
                    BufferOutStream bos = new BufferOutStream(Settings.SendBuffer, 0);
                    this._ConstructChunk(this._AdditionalFlags, chunksequencenumber, chunkdata, chunkinitial, chunkfinal, bos);
                    UDP.Send(this.EndPoint, DUIP.Data.FromBuffer(bos.Buffer, 0, bos.Position));
                    this._SendDelay += this._SendRate;
                    this._KeepAliveDelay = Settings.KeepAliveRate;
                }
                else
                {
                    this._SendDelay = 0.0;
                    break;
                }
            }

            // Send keep alive if needed
            this._KeepAliveDelay -= Time;
            if (this._KeepAliveDelay <= 0.0)
            {
                BufferOutStream bos = new BufferOutStream(Settings.SendBuffer, 0);
                this._ConstructKeepAlive(this._AdditionalFlags, bos);
                UDP.Send(this.EndPoint, DUIP.Data.FromBuffer(bos.Buffer, 0, bos.Position));
                this._KeepAliveDelay = Settings.KeepAliveRate;
            }

            // Check for implicit disconnect
            Remove = false;
            this._ExpireDelay -= Time;
            if (this._ExpireDelay <= 0.0)
            {
                this._Disconnect();
                Remove = true;
            }
        }

        /// <summary>
        /// Called when an (unvalidated) packet is received for this peer.
        /// </summary>
        internal void _Receive(UDPHubSettings Settings, UDP UDP, InStream Stream, int Size, out bool Remove)
        {
            int rsize = Size;
            Remove = false;

            // Check for minimal header containing flags and sequence number (for validation)
            if ((rsize -= StreamSize.Byte + StreamSize.Int) < 0) return;
            PacketFlags flags = (PacketFlags)Stream.ReadByte();
            int seqnum = Stream.ReadInt();

            // Validate
            if (this._Valid(seqnum))
            {
                this._ExpireDelay = Settings.ExpireDelay;

                // Disconnect?
                if ((flags & PacketFlags.Disconnect) == PacketFlags.Disconnect)
                {
                    Remove = true;
                    this._Disconnect();
                    return;
                }

                // Process additional information
                if (!this._ReadAdditional(flags, Stream, ref rsize))
                    return;

                // Process chunk if any received
                if ((flags & PacketFlags.Chunk) == PacketFlags.Chunk)
                {
                    byte[] chunk = new byte[rsize];
                    Stream.Read(chunk, 0, chunk.Length);
                    this._Process(seqnum, chunk, (flags & PacketFlags.ChunkInitial) == PacketFlags.ChunkInitial, (flags & PacketFlags.ChunkFinal) == PacketFlags.ChunkFinal);
                }

                // Ping request?
                if ((flags & PacketFlags.PingRequest) == PacketFlags.PingRequest)
                {
                    this._SendPingResponse(Settings, UDP);
                }
            }
        }

        /// <summary>
        /// Immediately sends a ping response packet of some sort.
        /// </summary>
        private void _SendPingResponse(UDPHubSettings Settings, UDP UDP)
        {
            BufferOutStream bos = new BufferOutStream(Settings.SendBuffer, 0);
            byte[] data = null;
            bool initial = false;
            bool final = false;
            int sequencenumber = 0;
            if (this.OutTerminal.Process(ref sequencenumber, ref data, ref initial, ref final))
            {
                this._ConstructChunk(this._AdditionalFlags | PacketFlags.PingRespose, sequencenumber, data, initial, final, bos);
                this._SendDelay = this._SendRate;
            }
            else
            {
                this._ConstructKeepAlive(this._AdditionalFlags | PacketFlags.PingRespose, bos);
            }
            UDP.Send(this.EndPoint, DUIP.Data.FromBuffer(bos.Buffer, 0, bos.Position));
            this._KeepAliveDelay = 0.0;
        }

        public override event Action<Peer, Message> Receive;
        public override event Action<Peer> Disconnect;

        /// <summary>
        /// The amount of time until a keep alive packet should be sent if no other packets are sent.
        /// </summary>
        internal double _KeepAliveDelay;

        /// <summary>
        /// The amount of time until the peer is considered disconnected unless a packet is received.
        /// </summary>
        internal double _ExpireDelay;

        /// <summary>
        /// The amount of time until a chunk packet should be sent.
        /// </summary>
        internal double _SendDelay;

        /// <summary>
        /// The current amount of time between chunk packets.
        /// </summary>
        internal double _SendRate;

        /// <summary>
        /// An estimate of the current round trip time for packets sent to this peer.
        /// </summary>
        internal double _RoundTripTime;

        /// <summary>
        /// Should the round trip time be sent with the next packet?
        /// </summary>
        internal bool _ShouldSendRoundTripTime;

        private IPEndPoint _EndPoint;
        private InTerminal _InTerminal;
        private OutTerminal _OutTerminal;
        private int _LastSequenceNumber;
        private UDPHub _Hub;
    }

    /// <summary>
    /// Manages a set of UDP peers that use a common UDP interface. Also allows new connections to be made.
    /// </summary>
    public class UDPHub
    {
        public UDPHub(UDP UDP, UDPHubSettings Settings)
        {
            this._Settings = Settings;
            this._Peers = new Dictionary<IPEndPoint, UDPPeer>();
            this._ConnectionRequests = new Dictionary<IPEndPoint, _ConnectionRequest>();
            this._UDP = UDP;

            UDP.Receive += new UDP.ReceiveRawPacketHandler(this._Receive);
        }

        public UDPHub(UDP UDP)
            : this(UDP, new UDPHubSettings())
        {

        }

        /// <summary>
        /// Gets or sets the settings for this UDP hub.
        /// </summary>
        public UDPHubSettings Settings
        {
            get
            {
                return this._Settings;
            }
            set
            {
                this._Settings = value;
            }
        }

        /// <summary>
        /// Tries connecting to a peer specified by an IP end point. If the peer is already connected, it will be immediately returned. If
        /// a connection can not be made, the query will be cancelled.
        /// </summary>
        public Query<UDPPeer> Connect(IPEndPoint EndPoint)
        {
            UDPPeer peer;
            if (this._Peers.TryGetValue(EndPoint, out peer))
            {
                return peer;
            }

            _ConnectionRequest cr;
            if (this._ConnectionRequests.TryGetValue(EndPoint, out cr))
            {
                // If there is already a connection request, reset the timeout
                cr.Remain = this._Settings.ConnectionRequestTimeout;
                return cr.Query;
            }
            else
            {
                UDPHubSettings settings = this._Settings;
                DelayedQuery<UDPPeer> query = new DelayedQuery<UDPPeer>();
                this._ConnectionRequests[EndPoint] = cr = new _ConnectionRequest
                {
                    AcknowledgementNumber = settings.Random.Integer(),
                    Delay = 0.0,
                    Query = query,
                    Remain = settings.ConnectionRequestTimeout,
                    Time = 0.0
                };
                return query;
            }
        }

        /// <summary>
        /// Updates the state of the hub by the given amount of time in seconds, sending packets if needed.
        /// </summary>
        public void Update(double Time)
        {
            this._UpdateConnectionRequests(Time);
            this._UpdatePeers(Time);
        }

        /// <summary>
        /// Updates the state of outgoing connection requests for this hub.
        /// </summary>
        private void _UpdateConnectionRequests(double Time)
        {
            UDPHubSettings settings = this.Settings;
            if (this._ConnectionRequests.Count > 0)
            {
                List<IPEndPoint> toremove = new List<IPEndPoint>();
                foreach (KeyValuePair<IPEndPoint, _ConnectionRequest> req in this._ConnectionRequests)
                {
                    IPEndPoint e = req.Key;
                    _ConnectionRequest cr = req.Value;

                    cr.Time += Time;
                    cr.Remain -= Time;
                    if (cr.Remain > 0.0)
                    {
                        double reqrate = settings.ConnectionRequestRate;
                        cr.Delay -= Time;
                        while (cr.Delay <= 0.0)
                        {
                            // Send request packet
                            BufferOutStream bos = new BufferOutStream(settings.SendBuffer, 0);
                            bos.WriteByte((byte)PacketFlags.ConnectionRequest);
                            bos.WriteInt(cr.AcknowledgementNumber);
                            this._UDP.Send(e, DUIP.Data.FromBuffer(bos.Buffer, 0, bos.Position));

                            cr.Delay += reqrate;
                        }
                    }
                    else
                    {
                        cr.Query.Cancel();
                        toremove.Add(e);
                    }
                }
                foreach (IPEndPoint r in toremove)
                {
                    this._ConnectionRequests.Remove(r);
                }
            }
        }

        /// <summary>
        /// Updates the peers for this hub.
        /// </summary>
        private void _UpdatePeers(double Time)
        {
            UDPHubSettings settings = this.Settings;
            List<IPEndPoint> toremove = new List<IPEndPoint>();
            foreach (UDPPeer peer in this._Peers.Values)
            {
                bool remove;
                peer._Update(settings, this._UDP, Time, out remove);
                if (remove)
                {
                    toremove.Add(peer.EndPoint);
                }
            }
            foreach (IPEndPoint r in toremove)
            {
                this._Peers.Remove(r);
            }
        }

        private void _Receive(IPEndPoint From, Temporary<Data> Data)
        {
            Data data = Data;
            int size = (int)data.Size;
            UDPHubSettings settings = this._Settings;

            // Find the peer that sent this message
            UDPPeer peer;
            if (this._Peers.TryGetValue(From, out peer))
            {
                bool remove;
                using (Disposable<InStream> str = data.Read())
                {
                    peer._Receive(settings, this._UDP, str, size, out remove);
                }
                if (remove)
                {
                    this._Peers.Remove(From);
                }
            }

            // See if this packet is a connection request, or a response to one
            if (size >= 5)
            {
                using (Disposable<InStream> dstr = data.Read())
                {
                    InStream str = dstr;
                    PacketFlags flags = (PacketFlags)str.ReadByte();
                    if (flags == PacketFlags.ConnectionRequest && size == 5)
                    {
                        // Connection requested
                        int seq = str.ReadInt();

                        if (_ShouldConnect(From))
                        {
                            int ack = settings.Random.Integer();

                            this._Peers[From] = peer = new UDPPeer(this, From, seq, ack)
                            {
                                _ExpireDelay = settings.InitialExpireDelay,
                                _KeepAliveDelay = settings.KeepAliveRate,
                                _RoundTripTime = settings.InitialRoundTripTime,
                                _ShouldSendRoundTripTime = false,
                                _SendRate = settings.InitialSendRate,
                                _SendDelay = 0.0
                            };

                            if (this.Accept != null)
                            {
                                this.Accept(peer);
                            }

                            BufferOutStream bos = new BufferOutStream(settings.SendBuffer, 0);
                            bos.WriteByte((byte)PacketFlags.ConnectionAccept);
                            bos.WriteInt(seq);
                            bos.WriteInt(ack); 
                            this._UDP.Send(From, DUIP.Data.FromBuffer(bos.Buffer, 0, bos.Position));
                        }
                        else
                        {
                            BufferOutStream bos = new BufferOutStream(settings.SendBuffer, 0);
                            bos.WriteByte((byte)PacketFlags.ConnectionRefuse);
                            bos.WriteInt(seq);
                            this._UDP.Send(From, DUIP.Data.FromBuffer(bos.Buffer, 0, bos.Position));
                        }
                    }

                    _ConnectionRequest cr;
                    if (flags == PacketFlags.ConnectionAccept && size == 9 && this._ConnectionRequests.TryGetValue(From, out cr))
                    {
                        // Connection accepted by peer
                        int ack = str.ReadInt();
                        int seq = str.ReadInt();
                        if (ack == cr.AcknowledgementNumber)
                        {
                            this._Peers[From] = peer = new UDPPeer(this, From, seq, ack)
                            {
                                _ExpireDelay = settings.ExpireDelay,
                                _KeepAliveDelay = 0.0,
                                _RoundTripTime = cr.Time,
                                _ShouldSendRoundTripTime = true,
                                _SendRate = settings.InitialSendRate,
                                _SendDelay = 0.0
                            };

                            cr.Query.Complete(peer);
                            this._ConnectionRequests.Remove(From);
                        }
                    }
                    if (flags == PacketFlags.ConnectionRefuse && size == 5 && this._ConnectionRequests.TryGetValue(From, out cr))
                    {
                        // Connection explicitly refused by peer
                        int ack = str.ReadInt();
                        if (ack == cr.AcknowledgementNumber)
                        {
                            cr.Query.Cancel();
                            this._ConnectionRequests.Remove(From);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines wether to accept a connection from the given endpoint.
        /// </summary>
        private bool _ShouldConnect(IPEndPoint EndPoint)
        {
            return true;
        }

        /// <summary>
        /// Gives information about a connection request in progress.
        /// </summary>
        private class _ConnectionRequest
        {
            /// <summary>
            /// The amount of time this connection request has been in progress.
            /// </summary>
            public double Time;

            /// <summary>
            /// The amount of time left for this connection request.
            /// </summary>
            public double Remain;

            /// <summary>
            /// The amount of time before sending another connection request packet.
            /// </summary>
            public double Delay;

            /// <summary>
            /// The acknowledgement number selected for this connection. A response to the connection request
            /// must use this number.
            /// </summary>
            public int AcknowledgementNumber;

            /// <summary>
            /// The query for this connection request.
            /// </summary>
            public DelayedQuery<UDPPeer> Query;
        }

        /// <summary>
        /// Event fired when a new peer is accepted by the hub.
        /// </summary>
        public event Action<UDPPeer> Accept;

        private UDP _UDP;
        private UDPHubSettings _Settings;
        private Dictionary<IPEndPoint, UDPPeer> _Peers;
        private Dictionary<IPEndPoint, _ConnectionRequest> _ConnectionRequests;
    }

    /// <summary>
    /// Contains information and settings about the processes of a UDPHub.
    /// </summary>
    public class UDPHubSettings
    {
        /// <summary>
        /// The amount of time, in seconds, to wait for a response to a connection request before considering
        /// the connection request failed.
        /// </summary>
        public double ConnectionRequestTimeout = 10.0;

        /// <summary>
        /// The amount of time, in seconds, between the sending of connection request packets.
        /// </summary>
        public double ConnectionRequestRate = 1.0;

        /// <summary>
        /// The amount of time, in seconds, for a newly connected peer to be considered disconnected if no packets
        /// are received.
        /// </summary>
        public double InitialExpireDelay = 3.0;

        /// <summary>
        /// The maximum amount of time, in seconds, a connected peer may remain connected if no packets
        /// are received.
        /// </summary>
        public double ExpireDelay = 25.0;

        /// <summary>
        /// The maximum amount of time, in seconds, between packets sent by the hub to any given peer.
        /// </summary>
        public double KeepAliveRate = 10.0;

        /// <summary>
        /// The amount of time, in seconds, between the sending of data packets of to a newly connected peer.
        /// </summary>
        public double InitialSendRate = 0.1;

        /// <summary>
        /// The initial estimate of the round trip time between peers before it is explicitly tested or given.
        /// </summary>
        public double InitialRoundTripTime = 0.1;

        /// <summary>
        /// The buffer used to construct packets for sending.
        /// </summary>
        public byte[] SendBuffer = new byte[65536];

        /// <summary>
        /// The size of the chunks to break messages into before sending.
        /// </summary>
        public int ChunkSize = 1024;

        /// <summary>
        /// Gets the RNG used to generate random sequence and acknowledgement numbers for sending messages.
        /// </summary>
        public Random Random = Random.Default;
    }
}