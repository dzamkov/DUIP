//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DUIP.Core;

namespace DUIP.Net
{
    /// <summary>
    /// Simple message scheme to test the network system.
    /// </summary>
    public class TestMessage : Message
    {
        public TestMessage()
        {

        }

        public TestMessage(BinaryReadStream Stream)
        {
            this.Amount = Stream.ReadInt();
        }

        public override void Serialize(BinaryWriteStream Stream)
        {
            Stream.WriteInt(this.Amount);
        }

        protected internal override void OnReceive()
        {
            System.Windows.Forms.MessageBox.Show("Amount received: " + this.Amount.ToString());
            this.Remove();
        }

        protected internal override void OnSend()
        {
            this.Remove();
        }

        public int Amount;
    }
}
