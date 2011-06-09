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
            Function func = Function.Identity;
            FunctionType functype = Type.Function(Type.String, Type.String);
            ISerialization<object> funcser = functype.GetSerialization(null);

            byte[] buffer = new byte[1024];
            funcser.Serialize(func, new BufferOutStream(buffer, 0));
            func = funcser.Deserialize(new BufferInStream(buffer, 0)) as Function;

            object test;
            func.Evaluate("hello world", out test);


            Path work = Path.WorkingDirectory;
            _Cache = work["Cache"];
            Path data = work["Data"];
            DirectoryAllocator alloc = new DirectoryAllocator(data);


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