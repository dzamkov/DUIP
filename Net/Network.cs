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

        /// <summary>
        /// Creates a network query to lookup the given indexed data.
        /// </summary>
        /// <param name="Bounty">The reward for a successful response to the query.</param>
        public Query<Data> Retreive(ID Index, Bounty Bounty)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stores the given data on the network and gets an index for it.
        /// </summary>
        /// <param name="PredictedBounty">A predication of the reward a peer will get by storing the data and making it available for
        /// other peers. After x seconds (from calling this method), the total favor a peer will receive for storing the data is predicted
        /// to be the intergral of the bounty at x. This can also be interpreted as the predicted reward a peer will get for storing the data for one second.</param>
        public Query<ID> Store(Data Data, Bounty PredictedBounty)
        {
            throw new NotImplementedException();
        }

        private LinkedList<Peer> _Peers;
    }
}