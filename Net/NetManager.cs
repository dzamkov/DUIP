//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using DUIP.Core;

namespace DUIP.Net
{
    /// <summary>
    /// Manager of network interactions on the current machine.
    /// </summary>
    public class NetManager
    {
        public NetManager(PullConnection PullCon, PushConnection PushCon, World World)
        {
            this._PullCon = PullCon;
            this._PushCon = PushCon;
            this._World = World;
            this._World._NetManager = this;
            this._Self = new Peer(new IPEndPoint(IPAddress.Loopback, 0));
            this._OutstandingMessages = new Dictionary<ID, Message>();
            this._Peers = new Dictionary<IPEndPoint, Peer>();
            
            this._PullCon.ReceiveMessage += new ReceiveNetDataHandler(_ReceiveNetData);
        }

        /// <summary>
        /// Connects to a peer. If no world was specified for the creation of this
        /// netmanager, this will also get information about the world on the peer.
        /// </summary>
        /// <param name="Peer">The peer to connect to.</param>
        public Peer Connect(IPEndPoint Peer)
        {
            Peer p = new Peer(Peer);
            if (this._World == null)
            {
                new WorldDescriptionRequest().Send(this, null, p);
            }
            return p;
        }

        /// <summary>
        /// Gets the world this netmanager is transmiting and receiving data
        /// for. If this is null, the netmanager will try to download the world
        /// of the next peer thats connect to.
        /// </summary>
        public World World
        {
            get
            {
                return this._World;
            }
            internal set
            {
                this._World = value;
                this._World._NetManager = this;
                if (this.WorldLoad != null)
                {
                    this.WorldLoad.Invoke(this._World);
                }
            }
        }

        /// <summary>
        /// Called when the previously null world has been downloaded from a peer.
        /// </summary>
        public event WorldLoadHandler WorldLoad;

        /// <summary>
        /// Callback for receiving network data.
        /// </summary>
        private void _ReceiveNetData(NetData Data)
        {
            Peer from = this.GetPeer(Data.From);
            this._GetMessage(new ByteArrayReader(Data.Data), from);
        }

        /// <summary>
        /// Gets a peer from an ip endpoint.
        /// </summary>
        public Peer GetPeer(IPEndPoint From)
        {
            lock (this)
            {
                Peer p;
                if (this._Peers.TryGetValue(From, out p))
                {
                    return p;
                }
                else
                {
                    p = new Peer(From);
                    this._Peers[From] = p;
                    return p;
                }
            }
        }

        /// <summary>
        /// Gets a message from the complete message data and the peer that sent it. The message
        /// is added to the outstanding message list.
        /// </summary>
        internal void _GetMessage(BinaryReadStream Read, Peer From)
        {
            lock (this)
            {
                ID messageid = new ID(Read);
                ID parentid = new ID(Read);
                Message m = (Message)Serialize.DeserializeLong(Read);
                Message p = null;

                if (parentid != ID.Blank())
                {
                    if (!this._OutstandingMessages.TryGetValue(parentid, out p))
                    {
                        ParentNotFoundException pnfe = new ParentNotFoundException
                        {
                            For = m,
                            ParentID = parentid
                        };
                        m.OnExcept(pnfe);
                    }
                }

                m._From = From;
                m._To = this._Self;
                m._ID = messageid;
                m._Parent = p;
                m._Manager = this;
                this._OutstandingMessages[m._ID] = m;

                lock (m)
                {
                    m.OnAssign(m._From, m._To, m._Manager);
                    m.OnDataRead(Read);
                    m.OnReceive();
                    if (p != null)
                    {
                        lock (p)
                        {
                            p.OnRespond(m);
                        }
                        m.OnRead();
                    }
                }
            }
        }

        /// <summary>
        /// Removes a message from this net managers message queue.
        /// </summary>
        /// <param name="Message">The message to remove.</param>
        internal void _RemoveMessage(Message Message)
        {
            lock (this)
            {
                this._OutstandingMessages.Remove(Message._ID);
            }
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="Message">The destination of the message</param>
        /// <param name="To">The peer to send the message to.</param>
        internal void _SendMessage(Message Message, Peer To)
        {
            lock (Message)
            {
                Message._Manager = this;
                Message._From = this._Self;
                Message._To = To;
                this._OutstandingMessages[Message._ID] = Message;
                Message.OnAssign(Message.From, Message.To, this);
                if (To == this._Self)
                {
                    // Message sent to self
                    Message.OnSend();
                    Message.OnReceive();
                    if (Message.Parent != null)
                    {
                        Message.Parent.OnRespond(Message);
                        Message.OnRead();
                    }
                }
                else
                {
                    // Message sent across net.
                    ByteArrayWriter baw = new ByteArrayWriter();
                    Message.ID.Serialize(baw);
                    if (Message.Parent == null)
                    {
                        ID.Blank().Serialize(baw);
                    }
                    else
                    {
                        Message.Parent.ID.Serialize(baw);
                    }
                    Serialize.SerializeLong(Message, baw);
                    Message.OnDataWrite(baw);
                    Message.OnSend();
                    this._PushCon.Send(baw.Data, To.Location);
                }
            }
        }

        private PullConnection _PullCon;
        private PushConnection _PushCon;
        private World _World;
        private Peer _Self;
        private Dictionary<IPEndPoint, Peer> _Peers;
        private Dictionary<ID, Message> _OutstandingMessages;
    }

    /// <summary>
    /// Handler for when a world is downloaded from another peer.
    /// </summary>
    /// <param name="World">The world that was loaded.</param>
    public delegate void WorldLoadHandler(World World);
}
