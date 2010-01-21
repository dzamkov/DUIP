//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using OpenTK;

using DUIP.Net;

namespace DUIP
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Core.TypeDirectory.LoadAssembly(typeof(Program).Assembly);
            DialogResult dr = MessageBox.Show("Wanna run Server(Okay) or Client(Cancel)?", "Honest question", MessageBoxButtons.OKCancel);
            bool part = dr == DialogResult.Cancel;

            UDPConnection con = new UDPConnection(27200 + (part ? 1 : 0));
            NetManager man;
            if (part)
            {
                System.Net.IPEndPoint endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 27200);
                man = new NetManager(con, con, null);
                man.Connect(endpoint);
            }
            else
            {
                Core.World world = new Core.World();
                man = new NetManager(con, con, world);
            }

            Visual.Window win = new Visual.Window(DisplayDevice.Default);
            if (part)
            {
                win.Title = "DUIP Client";
            }
            else
            {
                win.Title = "DUIP Server";
            }
            win.Run(60.0);
        }
    }
}
