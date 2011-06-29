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
            this._CompleteChunks = new LinkedList<int>();
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
            using (Disposable<OutStream> str = this._OutTerminal.Send(this._Hub.Settings.ChunkSize))
            {
                Message.Write(Message, str);
            }
        }

        /// <summary>
        /// Processes a chunk of a message sent from this peer.
        /// </summary>
        public void Process(int SequenceNumber, byte[] Chunk, bool Initial, bool Final)
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
        public bool Valid(int SequenceNumber)
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

        public override event Action<Peer, Message> Receive;

        private IPEndPoint _EndPoint;
        private InTerminal _InTerminal;
        private OutTerminal _OutTerminal;
        private LinkedList<int> _CompleteChunks;
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
            UDPHubSettings settings = this._Settings;

            // Handle outgoing connection requests, if any
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
                            BufferOutStream bos = new BufferOutStream(settings.SendBuffer, 0);
                            bos.WriteByte((byte)PacketFlags.ConnectionRequest);
                            bos.WriteInt(cr.AcknowledgementNumber);
                            this._UDP.Send(e, bos.Buffer, (int)bos.Position);

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

        private void _Receive(IPEndPoint From, byte[] Packet)
        {
            UDPHubSettings settings = this._Settings;

            // Find the peer that sent this message
            UDPPeer peer;
            if (this._Peers.TryGetValue(From, out peer))
            {
                throw new NotImplementedException();
            }

            // See if this packet is a connection request, or a response to one
            if (Packet.Length >= 5)
            {
                BufferInStream bis = new BufferInStream(Packet, 0);
                PacketFlags flags = (PacketFlags)bis.ReadByte();
                if (flags == PacketFlags.ConnectionRequest && Packet.Length == 5)
                {
                    int seq = bis.ReadInt();

                    if (_ShouldConnect(From))
                    {
                        int ack = settings.Random.Integer();

                        peer = new UDPPeer(this, From, seq, ack);
                        if (this.Accept != null)
                        {
                            this.Accept(peer);
                        }

                        BufferOutStream bos = new BufferOutStream(settings.SendBuffer, 0);
                        bos.WriteByte((byte)PacketFlags.ConnectionAccept);
                        bos.WriteInt(seq);
                        bos.WriteInt(ack);
                        this._UDP.Send(From, bos.Buffer, (int)bos.Position);
                    }
                    else
                    {
                        BufferOutStream bos = new BufferOutStream(settings.SendBuffer, 0);
                        bos.WriteByte((byte)PacketFlags.ConnectionRefuse);
                        bos.WriteInt(seq);
                        this._UDP.Send(From, bos.Buffer, (int)bos.Position);
                    }
                }

                _ConnectionRequest cr;
                if (flags == PacketFlags.ConnectionAccept && Packet.Length == 9 && this._ConnectionRequests.TryGetValue(From, out cr))
                {
                    int ack = bis.ReadInt();
                    int seq = bis.ReadInt();
                    if (ack == cr.AcknowledgementNumber)
                    {
                        peer = new UDPPeer(this, From, seq, ack);
                        cr.Query.Complete(peer);
                        this._ConnectionRequests.Remove(From);
                    }
                }
                if (flags == PacketFlags.ConnectionRefuse && Packet.Length == 5 && this._ConnectionRequests.TryGetValue(From, out cr))
                {
                    int ack = bis.ReadInt();
                    if (ack == cr.AcknowledgementNumber)
                    {
                        cr.Query.Cancel();
                        this._ConnectionRequests.Remove(From);
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