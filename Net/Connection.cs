using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// A message sent over the network.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// IP that sent this message.
        /// </summary>
        public IPAddress Sender;

        /// <summary>
        /// Connection that received this message.
        /// </summary>
        public PullConnection Connection;

        /// <summary>
        /// Message data.
        /// </summary>
        public byte[] Data;
    }

    /// <summary>
    /// Callback for when a message is received.
    /// </summary>
    /// <param name="Message">The newly received message.</param>
    public delegate void ReceiveMessageHandler(Message Message);

    /// <summary>
    /// Connection through which messages can be sent.
    /// </summary>
    public interface PushConnection
    {
        /// <summary>
        /// Sends data over this connection.
        /// </summary>
        /// <param name="Data">The data to send.</param>
        void Send(byte[] Data);
    }

    /// <summary>
    /// Connection through which messages can be received async.
    /// </summary>
    public interface PullConnection
    {
        /// <summary>
        /// Receive message event for this connect.
        /// </summary>
        event ReceiveMessageHandler ReceiveMessage;
    }
}
