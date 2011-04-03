using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;
using System.Net.Sockets;

namespace DUIP
{
    /// <summary>
    /// UDP helper class.
    /// </summary>
    public static class UDP
    {
        /// <summary>
        /// Sends a packet with the given end point.
        /// </summary>
        public static void Send(UdpClient Client, IPEndPoint To, byte[] Data)
        {
            while (true)
            {
                try
                {
                    Client.Send(Data, Data.Length, To);
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
        /// Sends a single packet to the specified end point.
        /// </summary>
        public static void Send(IPEndPoint To, byte[] Data)
        {
            using (UdpClient cli = new UdpClient())
            {
                Send(cli, To, Data);
            }
        }

        /// <summary>
        /// Asynchronously listens on the given client for a single packet.
        /// </summary>
        public static void Receive(UdpClient Client, ReceiveRawPacketHandler OnReceive)
        {
            // This really shouldn't be hard
            while (true)
            {
                try
                {
                    Client.BeginReceive(delegate(IAsyncResult ar)
                    {
                        lock (Client)
                        {
                            IPEndPoint end = new IPEndPoint(IPAddress.Any, 0);
                            byte[] data;
                            try
                            {
                                data = Client.EndReceive(ar, ref end);
                                OnReceive(end, data);
                            }
                            catch (SocketException se)
                            {
                                if (se.SocketErrorCode == SocketError.Shutdown)
                                {
                                    return;
                                }
                                if (_CanIgnore(se))
                                {
                                    Receive(Client, OnReceive);
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
                        }
                    }, null);
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
    }
}