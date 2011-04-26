using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.Terminal;
using DUIP.Lang.Parse;

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
            Parser<Expression> p = Parser.Expression;
            Expression e = null;
            Text t = Message;
            if (p.Accept(ref t, ref e))
            {

            }
            else
            {
                this.Send("Syntax error somewhere");
            }
        }

        protected override void Enter()
        {
            this.Send("Welcome to DUIP (Version " + Program.Version.ToString() + ")");
        }
    }
}