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
            this._SetupIP4Socket();
            if (Socket.OSSupportsIPv6)
            {
                this._SetupIP6Socket();
            }
        }

        public UDP(int Port)
        {
            this._Receiving = true;
            Socket ip4 = this._SetupIP4Socket();
            ip4.Bind(new IPEndPoint(IPAddress.Any, Port));
            this._Receive(ip4, new byte[MaxDataSize]);
            if (Socket.OSSupportsIPv6)
            {
                Socket ip6 = this._SetupIP6Socket();
                ip6.Bind(new IPEndPoint(IPAddress.IPv6Any, Port));
                this._Receive(ip6, new byte[MaxDataSize]);
            }
        }

        /// <summary>
        /// Performs setup operations on a socket.
        /// </summary>
        private static void _SetupSocket(Socket Socket)
        {

        }

        /// <summary>
        /// Prepares a socket for IP4.
        /// </summary>
        private Socket _SetupIP4Socket()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _SetupSocket(s);
            this._IP4Socket = s;
            return s;
        }

        /// <summary>
        /// Prepares a socket for IP6.
        /// </summary>
        private Socket _SetupIP6Socket()
        {
            Socket s = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            _SetupSocket(s);
            this._IP6Socket = s;
            return s;
        }

        /// <summary>
        /// Sends data using this UDP interface.
        /// </summary>
        public void Send(IPEndPoint To, Temporary<Data> Data)
        {
            AddressFamily af = To.AddressFamily;
            if (af == AddressFamily.InterNetwork)
            {
                this._Send(this._IP4Socket, To, Data);
            }
            if (af == AddressFamily.InterNetworkV6 && this._IP6Socket != null)
            {
                this._Send(this._IP6Socket, To, Data);
            }
        }

        /// <summary>
        /// Sends data using the given socket.
        /// </summary>
        private void _Send(Socket Socket, IPEndPoint To, Temporary<Data> Data)
        {
            byte[] buffer;
            int offset, size;
            _Bufferize(Data, out buffer, out offset, out size);
            Socket.BeginSendTo(buffer, offset, size, SocketFlags.None, To, delegate(IAsyncResult ar)
            {
                Socket socket = (ar.AsyncState as Socket);
                socket.EndSend(ar);
                if (!this._Receiving)
                {
                    this._Receiving = true;
                    this._Bind(((IPEndPoint)socket.LocalEndPoint).Port);
                }
            }, Socket);
        }

        /// <summary>
        /// Insures that the sockets are bound to the given port and are receiving.
        /// </summary>
        private void _Bind(int Port)
        {
            if (this._IP4Socket.LocalEndPoint == null)
            {
                this._IP4Socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            }
            _Receive(this._IP4Socket, new byte[MaxDataSize]);
            if (this._IP6Socket != null)
            {
                if (this._IP6Socket.LocalEndPoint == null)
                {
                    this._IP6Socket.Bind(new IPEndPoint(IPAddress.IPv6Any, Port));
                }
                _Receive(this._IP6Socket, new byte[MaxDataSize]);
            }
        }

        /// <summary>
        /// Gets a buffer representation of the given data.
        /// </summary>
        private static void _Bufferize(Data Data, out byte[] Buffer, out int Offset, out int Size)
        {
            BufferData bd = Data as BufferData;
            if (bd != null)
            {
                Buffer = bd.Buffer;
                Offset = 0;
                Size = Buffer.Length;
                return;
            }

            PartionData pd = Data as PartionData;
            if (pd != null)
            {
                bd = pd.Source as BufferData;
                if (bd != null)
                {
                    Buffer = bd.Buffer;
                    Size = (int)pd.Size;
                    try
                    {
                        checked
                        {
                            Offset = (int)pd.Start;
                            return;
                        }
                    }
                    catch (OverflowException)
                    {

                    }
                }
            }

            // This way isn't any fun :(
            Buffer = new byte[(int)Data.Size];
            using (Disposable<InStream> str = Data.Read())
            {
                str.Object.Read(Buffer, 0, Buffer.Length);
            }
            Offset = 0;
            Size = Buffer.Length;
        }

        private void _Receive(Socket Socket, byte[] Buffer)
        {
            while (true)
            {
                try
                {
                    const int offset = 0;
                    EndPoint endpoint = new IPEndPoint(Socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                    Socket.BeginReceiveFrom(Buffer, offset, Buffer.Length, SocketFlags.None, ref endpoint, delegate(IAsyncResult ar)
                    {
                        try
                        {
                            int size = Socket.EndReceiveFrom(ar, ref endpoint);
                            if (this.Received != null)
                            {
                                this.Received((IPEndPoint)endpoint, new BufferData(Buffer).GetPartion(0, size));
                            }
                        }
                        catch (SocketException se)
                        {
                            if (!_CanIgnore(se))
                            {
                                throw se;
                            }
                        }

                        // Restart receive cycle
                        this._Receive(Socket, Buffer);
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
            }
        }

        /// <summary>
        /// The maximum size for data sent or received by a UDP interface.
        /// </summary>
        public const int MaxDataSize = 65507;

        /// <summary>
        /// Event fired whenever this UDP interface receives data.
        /// </summary>
        public event ReceiveRawPacketHandler Received;

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
        public delegate void ReceiveRawPacketHandler(IPEndPoint From, Temporary<Data> Data);

        private bool _Receiving;
        private Socket _IP4Socket;
        private Socket _IP6Socket;
    }
}