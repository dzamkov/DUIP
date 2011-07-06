using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// An interface to a network of peers that can respond to and request queries.
    /// </summary>
    public class Network
    {
        public Network()
        {
            this._Peers = new LinkedList<Peer>();
        }

        /// <summary>
        /// Gets the set of peers in this network.
        /// </summary>
        public IEnumerable<Peer> Peers
        {
            get
            {
                return this._Peers;
            }
        }

        /// <summary>
        /// Adds a peer to this network.
        /// </summary>
        public void AddPeer(Peer Peer)
        {
            this._Peers.AddLast(Peer);
        }

        /// <summary>
        /// Removes a peer from this network.
        /// </summary>
        public void RemovePeer(Peer Peer)
        {
            this._Peers.Remove(Peer);
        }

        private LinkedList<Peer> _Peers;
    }
}