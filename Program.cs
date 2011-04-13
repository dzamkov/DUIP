using System;
using System.Collections.Generic;
using System.Linq;

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
            Console.Title = "DUIP";
            new RootInterface().Display();
        }
    }

    /// <summary>
    /// The primary interface started when the program starts.
    /// </summary>
    public class RootInterface : Interface
    {
        protected override void Receive(string Message)
        {

        }

        protected override void Enter()
        {
            this.Send("Welcome to DUIP (Version " + Program.Version.ToString() + ")");
        }
    }
}