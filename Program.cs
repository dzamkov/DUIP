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
            Path file = Path.WorkingDirectory["test.dat"];

            FileOutStream fos = file.OpenWrite();
            var ffos = fos.Format(StreamFormat.Default);
            ffos.WriteInt(4);
            ffos.WriteInt(8);
            ffos.WriteBool(true);
            ffos.Flush();
            fos.Finish();

            FileInStream fis = file.OpenRead();
            var ffis = fis.Format(StreamFormat.Default);
            int x = ffis.ReadInt();
            int y = ffis.ReadInt();
            bool z = ffis.ReadBool();
            ffis.Flush();
            fis.Finish();


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