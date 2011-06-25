﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Reflection;

using DUIP.GUI;

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
        public static void Main(string[] Args)
        {
            Path work = Path.WorkingDirectory;
            _Cache = work["Cache"];
            Path data = work["Data"];
            DirectoryAllocator alloc = new DirectoryAllocator(data);
            
            
            new Window().Run();
        }
    }
}