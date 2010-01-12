using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DUIP
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Net.UDPConnection A = new Net.UDPConnection(11000);
            Net.UDPConnection B = new Net.UDPConnection(System.Net.IPAddress.Loopback, 11000);

            B.Send(Encoding.ASCII.GetBytes("Hello"));

            A.ReceiveMessage += new Net.ReceiveMessageHandler(delegate(Net.Message Message)
                {
                    MessageBox.Show("Received \"" + Encoding.ASCII.GetString(Message.Data) +
                        "\" from " + Message.Sender.ToString());
                });
        }
    }
}
