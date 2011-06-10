using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

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

            // Test buddy allocator
            BuddyAllocator.Scheme scheme = new BuddyAllocator.Scheme
            {
                BaseContentSize = 16,
                Depth = 4
            };
            Memory mem = alloc.Allocate((long)scheme.RequiredSize, 0) ?? alloc.Lookup(0);
            BuddyAllocator balloc = BuddyAllocator.Create(mem, scheme);
            long ptr;
            balloc.Allocate(100, out ptr);
            balloc.Allocate(1, out ptr);
            balloc.Allocate(1, out ptr);
            balloc.Allocate(1, out ptr);
            balloc.Allocate(1, out ptr);

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

                Application.DoEvents();
            }
        }
    }
}