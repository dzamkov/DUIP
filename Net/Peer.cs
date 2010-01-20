//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// Representation of a peer of a computer on the network.
    /// </summary>
    public class Peer
    {
        internal Peer(IPEndPoint Location)
        {
            this._Location = Location;
        }

        /// <summary>
        /// Location of this peer on the network. This location only needs to
        /// specify and ip and port by which the current machine can reach the peer.
        /// </summary>
        public IPEndPoint Location
        {
            get
            {
                return this._Location;
            }
        }

        private IPEndPoint _Location;
    }
}
