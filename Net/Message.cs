using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// A type of message that can be sent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageType : Attribute
    {
        public MessageType(byte ID)
        {
            this.ID = ID;
        }

        /// <summary>
        /// The identifier for this message type.
        /// </summary>
        public byte ID;

        static MessageType()
        {
            _ForID = new Dictionary<byte, MessageType>();
            _ForType = new Dictionary<System.Type, MessageType>();
            foreach (var kvp in Reflection.SearchAttributes<MessageType>())
            {
                MessageType mt = kvp.Value;
                mt._Write = Reflection.Cast<Func<Message, Data>>(kvp.Key, "Write").Value;
                mt._Read = Reflection.Cast<Func<Data, Message>>(kvp.Key, "Read").Value;
                _ForID[kvp.Value.ID] = mt;
                _ForType[kvp.Key] = mt;
            }
        }

        public static MessageType ForID(byte ID)
        {
            return _ForID[ID];
        }

        public static MessageType ForType(System.Type Type)
        {
            return _ForType[Type];
        }

        public Data Write(Message Message)
        {
            return this._Write(Message);
        }

        public Message Read(Data Data)
        {
            return this._Read(Data);
        }

        private static Dictionary<byte, MessageType> _ForID;
        private static Dictionary<System.Type, MessageType> _ForType;
        private Func<Message, Data> _Write;
        private Func<Data, Message> _Read;
    }

    /// <summary>
    /// A message sent between peers in a network.
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Creates a data representation of this message for transmission or storage.
        /// </summary>
        public Data Write()
        {
            return MessageType.ForType(this.GetType()).Write(this);
        }

        /// <summary>
        /// Reads a message from data or returns null if the data could not be interpreted as a message.
        /// </summary>
        public static Message Read(Data Data)
        {
            return MessageType.ForID(Data[0]).Read(Data);
        }
    }

    /// <summary>
    /// A message that requests indexed data.
    /// </summary>
    [MessageType(0)]
    public class DataRequestMessage : Message
    {
        public static Data Write(Message Message)
        {
            throw new NotImplementedException();
        }

        public static new Message Read(Data Data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The index for the requested data.
        /// </summary>
        public ID Index;

        /// <summary>
        /// The bounty for a successful response to the request.
        /// </summary>
        public Bounty Bounty;
    }

    /// <summary>
    /// Describes the reward given to a peer for successfully responding to a query. The bounty of a query lets
    /// peers know how important a query is, wether they should respond, and how timely the response must be.
    /// </summary>
    /// <remarks>When the query is given to multiple peers, the bounty will be distributed among the responders so that the total
    /// favor granted is equal to the value of the bounty at the earliest time the result of the query is known. It is not required for
    /// a peer to explicitly track the favor of connected peers, however, queries still need to be associated with bounties to inform
    /// possible responders how much (if any) effort they should take to respond to the query.</remarks>
    public struct Bounty
    {
        /// <summary>
        /// The base reward (in favor) of the bounty. If a peer immediately (or preemptively) responds to the query, its favor will
        /// increase by this amount.
        /// </summary>
        /// <remarks>A unit of favor is defined as the average cost of sending or receiving a network packet. This means that responding to a query
        /// with a reward of 1.0 favor will balance out the transmission of 1 packet in terms of favor.</remarks>
        public double Base;

        /// <summary>
        /// The portion of the reward that remains after each second. This determines how time-sensitive the query is.
        /// </summary>
        public double Decay;
    }
}