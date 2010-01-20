//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using DUIP.Core;

namespace DUIP.Net
{
    /// <summary>
    /// A message sent across the network.
    /// </summary>
    public abstract class Message : Serializable
    {
        public Message()
        {
            this._ID = ID.Blank();
            this._Timed = new List<Timer>();
        }

        /// <summary>
        /// Serializes the contents of this message. AND ONLY THE CONTENTS. No metadata.
        /// </summary>
        /// <param name="Stream">The stream to serialize to.</param>
        public abstract void Serialize(BinaryWriteStream Stream);

        /// <summary>
        /// Called when this message is being responded to. This is called after the message's OnAssign
        /// and OnReceive callbacks.
        /// </summary>
        /// <param name="Response">The response to this message.</param>
        internal protected virtual void OnRespond(Message Response)
        {

        }

        /// <summary>
        /// Called when the network manager, peer and id are assigned to this
        /// message.
        /// </summary>
        internal protected virtual void OnAssign(Peer From, Peer To, NetManager Manager)
        {

        }

        /// <summary>
        /// Called when the message is sent. This happens after being assigned and
        /// is called on the sender.
        /// </summary>
        internal protected virtual void OnSend()
        {

        }

        /// <summary>
        /// Called when this message is received. This happens after being assigned
        /// and is called on the receiver. This is called before OnRespond on the
        /// parent message.
        /// </summary>
        internal protected virtual void OnReceive()
        {


        }

        /// <summary>
        /// Called on this messages receiver after the message it responded to had
        /// OnRespond called.
        /// </summary>
        internal protected virtual void OnRead()
        {


        }

        /// <summary>
        /// Called when there is a problem with loading the message.
        /// </summary>
        /// <param name="E">Exception details.</param>
        internal protected virtual void OnExcept(NetworkException E)
        {
            throw E;
        }

        /// <summary>
        /// Removes this message from the message queue, disallowing more responses.
        /// </summary>
        protected void Remove()
        {
            // Stop all timed actions
            lock (this)
            {
                foreach (Timer t in this._Timed)
                {
                    t.Stop();
                }
            }

            // Let all currently running timed actions run out

            // Remove the message
            lock (this)
            {
                this._Manager._RemoveMessage(this);
            }
        }

        /// <summary>
        /// Adds an action to this message that executes after a set amount of time.
        /// </summary>
        /// <param name="Time">The amount of time in seconds to execute.</param>
        /// <param name="Action">The action to perform.</param>
        /// <param name="OneTime">Should the timed action be one time or should it use
        /// the time span as an interval and execute that many times.</param>
        protected void AddTimedAction(double Time, TimedActionHandler Action, bool OneTime)
        {
            lock (this)
            {
                Timer t = new Timer(Time);
                t.Elapsed += delegate
                {
                    lock (this)
                    {
                        bool remove = Action() || OneTime;
                        if (remove)
                        {
                            t.Stop();
                            this._Timed.Remove(t);
                        }
                    }
                };
                this._Timed.Add(t);
                t.Start();
            }
        }

        /// <summary>
        /// Sends this message. This can be called if OnAssign has not yet been
        /// called on the message.
        /// </summary>
        /// <param name="Manager">The net manager to send this message on.</param>
        /// <param name="Parent">The message to respond to or null if this message is not
        /// a response.</param>
        /// <param name="To">The peer to send the message.</param>
        public void Send(NetManager Manager, Message Parent, Peer To)
        {
            this._Manager = Manager;
            this._ID = ID.Random();
            this._Parent = Parent;
            Manager._SendMessage(this, To);
        }

        /// <summary>
        /// Gets the identifier of this message.
        /// </summary>
        public ID ID
        {
            get
            {
                return this._ID;
            }
        }

        /// <summary>
        /// Gets the net manager for this message.
        /// </summary>
        public NetManager NetManager
        {
            get
            {
                return this._Manager;
            }
        }

        /// <summary>
        /// Gets the peer this is from.
        /// </summary>
        public Peer From
        {
            get
            {
                return this._From;
            }
        }

        /// <summary>
        /// Gets the peer this is to.
        /// </summary>
        public Peer To
        {
            get
            {
                return this._To;
            }
        }

        /// <summary>
        /// Gets the message this message responds to.
        /// </summary>
        public Message Parent
        {
            get
            {
                return this._Parent;
            }
        }

        internal Peer _From;
        internal Peer _To;
        internal NetManager _Manager;
        internal Message _Parent;
        internal ID _ID;
        private List<Timer> _Timed;
    }

    /// <summary>
    /// Handler for a timed action.
    /// </summary>
    /// <returns>True to keep the timed action and cause it to
    /// repeat at a later time or false to remove the timed action. This value does not
    /// matter if the OneTime parameter used to create the timed action is true.</returns>
    public delegate bool TimedActionHandler();

    /// <summary>
    /// An exception that involves networking.
    /// </summary>
    public class NetworkException : Exception
    {
        /// <summary>
        /// The message that is involved with the network exception.
        /// </summary>
        public Message For;
    }

    /// <summary>
    /// An exception where the message was sent with a parent but the receiving
    /// network manager does not have the parent message stored.
    /// </summary>
    public class ParentNotFoundException : NetworkException
    {
        /// <summary>
        /// Identifier for the parent message.
        /// </summary>
        public ID ParentID;
    }
}