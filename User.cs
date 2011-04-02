using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Represents an actor which can modify data.
    /// </summary>
    public abstract class User : Content
    {
        /// <summary>
        /// Gets a non-unique readable name for the user. The name may be null if it is not defined.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the credential for the user.
        /// </summary>
        public abstract Credential Credential { get; }

        /// <summary>
        /// Gets if this user can act as another user. If so, this user may make any modification the given
        /// user can, including modifications to the user itself such as which users can act as or under the given user.
        /// </summary>
        public abstract Query<bool> CanActAs(User User);

        /// <summary>
        /// Gets if this user can act under another user. If so, this user may make any modification the given
        /// user can, excluding modifications to the user itself.
        /// </summary>
        public abstract Query<bool> CanActUnder(User User);

        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// A public method of proving if a command was issued directly from a user.
    /// </summary>
    public abstract class Credential : Content
    {
        /// <summary>
        /// Gets if the specified proof is sufficient for this credential.
        /// </summary>
        public abstract bool CanProve(Proof Proof);

        /// <summary>
        /// Gets the null credential (a credential that cannot be proven).
        /// </summary>
        public static NullCredential Null
        {
            get
            {
                return NullCredential.Singleton;
            }
        }
    }

    /// <summary>
    /// A private method of proving a command is issued directly from a user.
    /// </summary>
    public abstract class Proof : Content
    {

    }

    /// <summary>
    /// Credential that disallows all direct commands from a user.
    /// </summary>
    public class NullCredential : Credential
    {
        private NullCredential()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly NullCredential Singleton = new NullCredential();

        public override bool CanProve(Proof Proof)
        {
            return false;
        }
    }
}