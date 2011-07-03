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
        internal UDPPeer(UDPHubSettings Settings, IPEndPoint EndPoint, int SequenceNumber, int AcknowledgementNumber)
        {
            this._Settings = Settings;
            this._EndPoint = EndPoint;
            this._InTerminal = new InTerminal(AcknowledgementNumber);
            this._OutTerminal = new OutTerminal(SequenceNumber);
        }

        /// <summary>
        /// Initializes a peer with the local machine being the server of the connection.
        /// </summary>
        internal static UDPPeer _InitializeServer(
            UDPHubSettings Settings, IPEndPoint EndPoint, 
            int SequenceNumber, int AcknowledgementNumber)
        {
            return new UDPPeer(Settings, EndPoint, SequenceNumber, AcknowledgementNumber)
            {
                _RoundTripTime = Settings.InitialRoundTripTime,
                _KeepAliveDelay = Settings.KeepAliveDelay,
                _ExpireDelay = Settings.InitialExpireDelay,
                _WaveDelay = 0.0,
                _WaveSize = Settings.InitialWaveSize
            };
        }

        /// <summary>
        /// Initializes a peer with the local machine being the client (initiator) of the connection.
        /// </summary>
        /// <param name="Time">The time it took to respond to the connection request.</param>
        internal static UDPPeer _InitializeClient(
            UDPHubSettings Settings, IPEndPoint EndPoint, 
            int SequenceNumber, int AcknowledgementNumber,
            double Time)
        {
            return new UDPPeer(Settings, EndPoint, SequenceNumber, AcknowledgementNumber)
            {
                _RoundTripTime = Time,
                _KeepAliveDelay = 0.0,
                _ExpireDelay = Settings.ExpireDelay,
                _WaveDelay = 0.0,
                _WaveSize = Settings.InitialWaveSize,
                _ShouldSendRoundTripTime = true
            };
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

        public override void Send(Message Message)
        {
            lock (this) // Prevents two send streams from being open at the same time
            {
                using (Disposable<OutStream> str = this._OutTerminal.Send(this._Settings.ChunkSize))
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
        /// Determines wether a packet with the given sequence number is likely to be valid (authentic).
        /// </summary>
        private bool _Valid(int SequenceNumber)
        {
            int ack = this._InTerminal.AcknowledgementNumber;
            return
                _After(SequenceNumber + _ValidThreshold, ack) &&
                _After(ack + _ValidThreshold, SequenceNumber);
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
            return A - B >= 0;
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
        /// Processes a chunk of a message sent from this peer.
        /// </summary>
        private void _Process(int SequenceNumber, byte[] Chunk, bool Initial, bool Final)
        {
            Disposable<InStream> str = null;
            bool acknowledged;
            if (this._InTerminal.Process(SequenceNumber, Chunk, Initial, Final, out acknowledged, ref str))
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

            // Make sure an acknowledgement number is sent for a new chunk
            this._ShouldSendAcknowledgement = true;
            if (acknowledged)
            {
                this._KeepAliveDelay = Math.Min(this._KeepAliveDelay, this._Settings.AcknowledgementDelay);
            }
        }

        /// <summary>
        /// Fills the given packet with additional information.
        /// </summary>
        private void _FillAdditional(Packet Packet)
        {
            if (this._ShouldSendAcknowledgement)
            {
                this._ShouldSendAcknowledgement = false;
                Packet.AcknowledgementNumber = this.InTerminal.AcknowledgementNumber;
            }

            if (this._ShouldSendRoundTripTime)
            {
                this._ShouldSendRoundTripTime = false;
                Packet.RoundTripTime = this.RoundTripTime;
            }
        }

        /// <summary>
        /// Updates the state of the peer and sends packets if needed.
        /// </summary>
        internal void _Update(UDPHub Hub, double Time, out bool Remove)
        {
            UDPHubSettings settings = this._Settings;
            OutTerminal oterm = this._OutTerminal;

            // Handle waves
            this._WaveDelay -= Time;
            if (this._WaveDelay <= 0.0)
            {
                // Check acknowledgement and adjust wave size
                if (oterm.AcknowledgementNumber == oterm.SendNumber)
                {
                    if (this._FullWave)
                    {
                        this._WaveSize++;
                        this._FullWave = false;
                    }
                }
                else
                {
                    oterm.SendNumber = oterm.AcknowledgementNumber;
                    if (this._WaveSize > 1)
                    {
                        this._WaveSize--;
                    }
                }

                int chunksequencenumber = 0;
                byte[] chunkdata = null;
                bool chunkinitial = false;
                bool chunkfinal = false;
                int wavesize = 0;
                if (oterm.Process(ref chunksequencenumber, ref chunkdata, ref chunkinitial, ref chunkfinal))
                {
                    while (true)
                    {
                        // Send chunk
                        wavesize++;
                        Packet chunk = new Packet
                        {
                            SequenceNumber = chunksequencenumber,
                            ChunkData = chunkdata,
                            ChunkInitial = chunkinitial,
                            ChunkFinal = chunkfinal
                        };
                        this._FillAdditional(chunk);
                        Hub.Send(chunk, this._EndPoint);

                        // Check if wave is full
                        if (wavesize == this._WaveSize)
                        {
                            this._FullWave = true;
                            break;
                        }

                        // Get next chunk
                        if (!oterm.Process(ref chunksequencenumber, ref chunkdata, ref chunkinitial, ref chunkfinal))
                        {
                            break;
                        }
                    }

                    // Reset delays
                    this._WaveDelay = this._RoundTripTime + settings.WaveRate;
                    this._KeepAliveDelay = settings.KeepAliveDelay;
                }
            }

            // Send keep alive if needed
            this._KeepAliveDelay -= Time;
            if (this._KeepAliveDelay <= 0.0)
            {
                Packet packet = new Packet
                {
                    SequenceNumber = this._OutTerminal.SendNumber
                };
                this._FillAdditional(packet);
                Hub.Send(packet, this.EndPoint);
                this._KeepAliveDelay = settings.KeepAliveDelay;
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
        internal void _Receive(UDPHubSettings Settings, UDPHub Hub, Packet Packet, out bool Remove)
        {
            Remove = false;

            // Validate
            if (!this._Valid(Packet.SequenceNumber))
                return;

            // Disconnect?
            if (Packet.Disconnect)
            {
                Remove = true;
                return;
            }

            // Reset expire delay
            this._ExpireDelay = Settings.ExpireDelay;

            // Read additional information
            int ack;
            if (Packet.AcknowledgementNumber.TryGetValue(out ack))
            {
                this._OutTerminal.AcknowledgementNumber = ack;
            }
            double rtt;
            if (Packet.RoundTripTime.TryGetValue(out rtt))
            {
                this._RoundTripTime = rtt;
            }

            // Chunk?
            if (Packet.ChunkData != null)
            {
                this._Process(Packet.SequenceNumber, Packet.ChunkData, Packet.ChunkInitial, Packet.ChunkFinal);
            }

            // Ping?
            if (Packet.PingRequest)
            {
                this._SendPingResponse(Hub);
            }
        }

        /// <summary>
        /// Immediately sends a ping response packet of some sort.
        /// </summary>
        private void _SendPingResponse(UDPHub Hub)
        {
            byte[] data = null;
            bool initial = false;
            bool final = false;
            int sequencenumber = 0;
            Packet packet;
            if(this._OutTerminal.Process(ref sequencenumber, ref data, ref initial, ref final))
            {
                packet = new Packet
                {
                    SequenceNumber = sequencenumber,
                    ChunkData = data,
                    ChunkInitial = initial,
                    ChunkFinal = final
                };
            }
            else
            {
                packet = new Packet
                {
                    SequenceNumber = this._OutTerminal.SendNumber
                };
            }
            packet.PingResponse = true;
            Hub.Send(packet, this.EndPoint);
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
        /// The amount of time until the next wave of chunk packets should be sent.
        /// </summary>
        internal double _WaveDelay;

        /// <summary>
        /// The amount of chunks sent in a wave.
        /// </summary>
        internal int _WaveSize;

        /// <summary>
        /// An estimate of the current round trip time for packets sent to this peer.
        /// </summary>
        internal double _RoundTripTime;

        /// <summary>
        /// Indicates wether the last sent was has the maximum amount of chunks.
        /// </summary>
        internal bool _FullWave;

        /// <summary>
        /// Should the round trip time be sent with the next packet?
        /// </summary>
        internal bool _ShouldSendRoundTripTime;

        /// <summary>
        /// Should the acknowledgement number be sent with the next packet?
        /// </summary>
        internal bool _ShouldSendAcknowledgement;

        private IPEndPoint _EndPoint;
        private InTerminal _InTerminal;
        private OutTerminal _OutTerminal;
        private UDPHubSettings _Settings;
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
        /// Uses the connection and settings of this hub to send a packet to the given endpoint.
        /// </summary>
        public void Send(Packet Packet, IPEndPoint To)
        {
            BufferOutStream bos = new BufferOutStream(this._Settings.SendBuffer, 0);
            Packet.Write(Packet, bos);
            this._UDP.Send(To, Data.FromBuffer(bos.Buffer, 0, bos.Position));
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
                peer._Update(this, Time, out remove);
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
                    peer._Receive(settings, this, Packet.Read(str, size), out remove);
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

                            this._Peers[From] = peer = UDPPeer._InitializeServer(settings, From, seq, ack);

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
                        // Connection accepted by client
                        int ack = str.ReadInt();
                        int seq = str.ReadInt();
                        if (ack == cr.AcknowledgementNumber)
                        {
                            this._Peers[From] = peer = UDPPeer._InitializeClient(settings, From, seq, ack, cr.Time);
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
        public double KeepAliveDelay = 10.0;

        /// <summary>
        /// The initial estimate of the round trip time between peers before it is explicitly tested or given.
        /// </summary>
        public double InitialRoundTripTime = 0.1;

        /// <summary>
        /// The time in seconds, in addition to round trip time, between the sending of waves
        /// (groups of packets).
        /// </summary>
        public double WaveRate = 0.5;

        /// <summary>
        /// The amount of chunks to send in each wave to a newly connected peer. The wave size may be adjusted
        /// to adapt to the capabilities of the network.
        /// </summary>
        public int InitialWaveSize = 10;

        /// <summary>
        /// The maximum time in seconds, to wait before sending an acknowledgement of a received chunk.
        /// </summary>
        public double AcknowledgementDelay = 0.3;

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