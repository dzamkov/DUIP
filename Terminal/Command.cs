using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Terminal
{
    /// <summary>
    /// An action or operation performed by the user using an interface.
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Gets the user-friendly name for this command.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the arguments that must be specified right after the command name without being named themselves.
        /// </summary>
        public virtual IEnumerable<Argument> ImplicitArguments
        {
            get
            {
                return new Argument[0];
            }
        }

        /// <summary>
        /// Gets the arguments that may be specified after the implicit arguments.
        /// </summary>
        public virtual IEnumerable<Argument> ImplicitOptionalArguments
        {
            get
            {
                return new Argument[0];
            }
        }

        /// <summary>
        /// Gets the arguments to be specified explicitly after the commands implicit arguments. These arguments are optional.
        /// </summary>
        public virtual IEnumerable<Argument> ExplicitArguments
        {
            get
            {
                return new Argument[0];
            }
        }

        /// <summary>
        /// Gets the result type of the command.
        /// </summary>
        public abstract Type ResultType { get; }

        /// <summary>
        /// Invokes the command for some argument set.
        /// </summary>
        public abstract void Invoke(Invocation Invocation);
    }

    /// <summary>
    /// Information about a call to a command.
    /// </summary>
    public abstract class Invocation
    {
        /// <summary>
        /// Gets the interface the command was invoked on. Messages may be printed to the interface.
        /// </summary>
        public abstract Interface Interface { get; }

        /// <summary>
        /// Looks up the value of an argument in this invocation.
        /// </summary>
        public abstract Maybe<T> Lookup<T>(Argument Argument);

        /// <summary>
        /// Sets the result of the invocation. The result should be the result type of the command.
        /// </summary>
        public abstract void SetResult<T>(T Result);
    }

    /// <summary>
    /// An argument to a command.
    /// </summary>
    public class Argument
    {
        public Argument(string ShortName, string Name, Type Type)
        {
            this._ShortName = ShortName;
            this._Name = Name;
            this._Type = Type;
        }

        /// <summary>
        /// Creates a non-optional argument with the given name and type.
        /// </summary>
        public static Argument Create(string Name, Type Type)
        {
            return new Argument(null, Name, Type);
        }

        /// <summary>
        /// Creates a non-optional argument with the given name, short name and type.
        /// </summary>
        public static Argument Create(string Name, string ShortName, Type Type)
        {
            return new Argument(ShortName, Name, Type);
        }

        /// <summary>
        /// Gets the short name for the argument, or null if there is none.
        /// </summary>
        public string ShortName
        {
            get
            {
                return this._ShortName;
            }
        }

        /// <summary>
        /// Gets the name of this argument.
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        /// <summary>
        /// Gets the type of the argument. Note that arguments of the Void type can have their value completely omitted in an invocation.
        /// </summary>
        public Type Type
        {
            get
            {
                return this._Type;
            }
        }

        private string _ShortName;
        private string _Name;
        private Type _Type;
    }
}