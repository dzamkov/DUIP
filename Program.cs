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
            DirectoryAllocator alloc = new DirectoryAllocator(data);

            HashMap<ID, ID>.Plan hmplan = new HashMap<ID, ID>.Plan()
            {
                Buckets = 100,
                CellarBuckets = 10,
                BucketSerialization = HashMap<ID, ID>.Bucket.CreateSerialization(ID.Serialization, ID.Serialization),
                KeyHashing = ID.Hashing
            };
            int hmptr;
            Data hmdata = alloc.Allocate(hmplan.TotalSize, out hmptr);

            HashMap<ID, ID> hm = HashMap<ID, ID>.Create(hmdata, hmplan);

            for (int t = 0; t < 100; t++)
            {
                hm.Modify(new ID(0, 0, 0, t), new ID(97, 98, 99, t));
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