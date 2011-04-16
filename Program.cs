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
            Path work = Path.WorkingDirectory;
            Path data = Path.WorkingDirectory["Data"];
            FileStore<ID> fs = new FileStore<ID>(data, ID.Serialization, ID.Ordering);

            Path testfile = work["test.dat"];
            FileData fd = testfile.Create(1024);

            OutStream os = fd.Modify(100);
            os.Write(101);
            os.Write(102);
            os.Write(103);
            os.Finish();

            InStream s = fd.Read(100);
            byte x = s.Read();
            byte y = s.Read();
            byte z = s.Read();
            s.Finish();

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