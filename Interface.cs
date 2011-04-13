using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A console context the user can interact with.
    /// </summary>
    public abstract class Interface
    {
        /// <summary>
        /// Displays this interface as the root interface.
        /// </summary>
        public void Display()
        {
            this._Display(null);
        }

        /// <summary>
        /// Displays this interface with the given parent interface.
        /// </summary>
        private void _Display(Interface Parent)
        {
            this._Exited = false;
            this._Parent = Parent;
            this.Enter();
            while (!this._Exited)
            {
                string m = this._WaitMessage(false);
                this.Receive(m);
            }
        }

        /// <summary>
        /// Waits for a message from the user.
        /// </summary>
        private string _WaitMessage(bool Request)
        {
            if (Request)
            {
                Console.Write("? ");
            }
            else
            {
                Console.Write("> ");
            }
            return Console.ReadLine();
        }

        /// <summary>
        /// Called when a message from the user is received.
        /// </summary>
        protected virtual void Receive(string Message)
        {
            
        }

        /// <summary>
        /// Called when the interface is entered.
        /// </summary>
        protected virtual void Enter()
        {

        }

        /// <summary>
        /// Requests a message from the user.
        /// </summary>
        protected string Request()
        {
            return this._WaitMessage(true);
        }

        /// <summary>
        /// Sends a response to the user on this interface.
        /// </summary>
        protected void Send(string Response)
        {
            Console.WriteLine(Response);
        }

        /// <summary>
        /// Exits the interface, returning control to the previous interface if any.
        /// </summary>
        protected void Exit()
        {
            this._Exited = true;
        }

        /// <summary>
        /// Defers interaction to another interface. This interface will be restored once the given interface has exited. 
        /// </summary>
        protected void Defer(Interface Interface)
        {
            Interface._Display(this);
        }

        private bool _Exited;
        private Interface _Parent;
    }
}