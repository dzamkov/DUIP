//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace DUIP.Net
{
    /// <summary>
    /// Connection through UDP.
    /// </summary>
    public class UDPConnection : PullConnection, PushConnection
    {
        /// <summary>
        /// Creates a udp connection and begins listening on the specified port.
        /// </summary>
        /// <param name="Port">The port to start listening on.</param>
        public UDPConnection(int Port)
        {
            this._Conn = new UdpClient(Port);
            this._BeginListen();
        }

        public void Send(byte[] Data, IPEndPoint To)
        {
            this._Conn.Send(Data, Data.Length, To);
        }

        public event ReceiveMessageHandler ReceiveMessage;

        /// <summary>
        /// Receive callback for use with BeginReceive.
        /// </summary>
        /// <param name="AR">Async result.</param>
        private void _ReceiveCallback(IAsyncResult AR)
        {
            IPEndPoint end = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = this._Conn.EndReceive(AR, ref end);
            Message me = new Message();
            me.Connection = this;
            me.From = end;
            me.Data = data;
            this._BeginListen();
            if (this.ReceiveMessage != null)
            {
                this.ReceiveMessage.Invoke(me);
            }
        }

        /// <summary>
        /// Begins listening for more messages.
        /// </summary>
        private void _BeginListen()
        {
            this._Conn.BeginReceive(_ReceiveCallback, null);
        }

        private UdpClient _Conn;
    }
}
