using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;
using System.Net.Sockets;

namespace DUIP.Net
{
    /// <summary>
    /// An interface to a UDP client that can send and receive data.
    /// </summary>
    public class UDP
    {
        public UDP()
        {
            this._Client = new UdpClient();
        }

        public UDP(int Port)
        {
            this._Client = new UdpClient(Port);
            this._Receive();
        }

        /// <summary>
        /// Sends data using this UDP interface.
        /// </summary>
        public void Send(IPEndPoint To, byte[] Data)
        {
            while (true)
            {
                try
                {
                    this._Client.Send(Data, Data.Length, To);
                    this._Receive();
                    return;
                }
                catch (SocketException se)
                {
                    if (!_CanIgnore(se))
                    {
                        throw se;
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Event fired whenever this UDP interface receives a packet.
        /// </summary>
        public event ReceiveRawPacketHandler Receive;

        /// <summary>
        /// Begins receiving for the UDP interface.
        /// </summary>
        private void _Receive()
        {
            // This really shouldn't be hard
            while (!this._Receiving)
            {
                try
                {
                    this._Client.BeginReceive(delegate(IAsyncResult ar)
                    {
                        lock (this)
                        {
                            this._Receiving = false;
                            IPEndPoint end = new IPEndPoint(IPAddress.Any, 0);
                            byte[] data;
                            try
                            {
                                data = this._Client.EndReceive(ar, ref end);
                                if (this.Receive != null)
                                {
                                    this.Receive(end, data);
                                }
                            }
                            catch (SocketException se)
                            {
                                if (se.SocketErrorCode == SocketError.Shutdown)
                                {
                                    return;
                                }
                                if (_CanIgnore(se))
                                {
                                    this._Receive();
                                }
                                else
                                {
                                    throw se;
                                }
                            }
                            catch (ObjectDisposedException)
                            {
                                return;
                            }
                            this._Receive();
                        }
                    }, null);
                    this._Receiving = true;
                    return;
                }
                catch (SocketException se)
                {
                    if (!_CanIgnore(se))
                    {
                        throw se;
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Gets if the specified exception can safely be ignored.
        /// </summary>
        private static bool _CanIgnore(SocketException Exception)
        {
            return Exception.SocketErrorCode == SocketError.ConnectionReset;
        }

        /// <summary>
        /// Called when a raw (unprocessed) packet is received.
        /// </summary>
        public delegate void ReceiveRawPacketHandler(IPEndPoint From, byte[] Data);

        private UdpClient _Client;
        private bool _Receiving;
    }
}