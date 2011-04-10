using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI;
using DUIP.UI.GDI;

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
        /// Program main entry point.
        /// </summary>
        public static void Main(string[] Args)
        {
            ControlHost.Run(new TestControl());
        }
    }
}