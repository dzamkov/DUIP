﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using DUIP.Net;
using DUIP.UI;

namespace DUIP
{
    /// <summary>
    /// Contains functions and information related to the program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program release version number.
        /// </summary>
        public const int Version = 1;

        /// <summary>
        /// Gets the icon for the program.
        /// </summary>
        public static Icon Icon
        {
            get
            {
                Assembly cur = Assembly.GetExecutingAssembly();
                try
                {
                    return Icon.ExtractAssociatedIcon(cur.Location);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The location of the cache for the program. This folder may store static procedurally created data between
        /// program runs. This folder may not exist on the first program run.
        /// </summary>
        public static Path Cache
        {
            get
            {
                return _Cache;
            }
        }
        private static Path _Cache;

        /// <summary>
        /// Program main entry point.
        /// </summary>
        [STAThread]
        public static void Main(string[] Args)
        {
            Path work = Path.WorkingDirectory;
            _Cache = work["Cache"];
            Path data = work["Data"];
            DirectoryAllocator alloc = new DirectoryAllocator(data);

            UDPHub mainhub = new UDPHub(new UDP(101));
            UDPHub testhub = new UDPHub(new UDP());
            testhub.Connect(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 101)).Register(delegate(UDPPeer peer)
            {
                peer.Send(new DataRequestMessage
                {
                    Index = new ID(1, 2, 3, 4),
                    Region = DataRegion.Full,
                    Bounty = new Bounty(49.0, 0.9)
                });
                peer.Received += delegate(Peer npeer, Net.Message Message)
                {
                    peer.Send(Message);
                };
            });
            mainhub.Accepted += delegate(UDPPeer peer)
            {
                peer.Received += delegate(Peer npeer, Net.Message Message)
                {
                    peer.Send(Message);
                };
            };

            Application.EnableVisualStyles();
            MainForm mf = new MainForm();
            mf.Icon = Icon;
            mf.Text = "DUIP";
            mf.Show();
            DateTime lastupdate = DateTime.Now;

            while (mf.Visible)
            {
                WorldDisplay view = mf.WorldDisplay;
                view.Render();
                DateTime now = DateTime.Now;
                double updatetime = (now - lastupdate).TotalSeconds;
                lastupdate = now;
                view.Update(updatetime);
                mainhub.Update(updatetime);
                testhub.Update(updatetime);

                Application.DoEvents();
            }
        }
    }
}