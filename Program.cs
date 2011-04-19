using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.Terminal;

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
            BigInt a = new BigInt(1345651412);
            BigInt b = new BigInt(1234321431);
            BigInt c = a;
            c *= b;
            c += b;
            c *= a;
            c *= a + b;
            c /= a + b;
            c /= a;
            c -= b;
            c /= b;

            Path work = Path.WorkingDirectory;
            Path data = Path.WorkingDirectory["Data"];
            DirectoryAllocator alloc = new DirectoryAllocator(data);

            for (int t = 0; t < 100; t++)
            {
                Data d;
                alloc.Allocate(100, out d);
            }

            Console.Title = "DUIP";
            new RootInterface().Display();
        }
    }

    /// <summary>
    /// The primary interface started when the program starts.
    /// </summary>
    public class RootInterface : Interface
    {
        public RootInterface()
        {

        }

        public override string Name
        {
            get
            {
                return "";
            }
        }

        protected override void Receive(string Message)
        {
            this.Send("You said, " + Message + "?");
        }

        protected override void Enter()
        {
            this.Send("Welcome to DUIP (Version " + Program.Version.ToString() + ")");
        }
    }
}