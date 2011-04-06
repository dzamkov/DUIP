using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An actor whose actions are explicitly given. To act as the user, one most have the complimentary proof for the user.
    /// </summary>
    public abstract class User : Actor
    {

    }

    /// <summary>
    /// A private method of proving that a command was issued by a user.
    /// </summary>
    public abstract class Proof
    {

    }

    /// <summary>
    /// A user-proof pair that can be used to act as a user in a network.
    /// </summary>
    public struct Credential
    {
        public Credential(User User, Proof Proof)
        {
            this.User = User;
            this.Proof = Proof;
        }

        /// <summary>
        /// The user for the credential.
        /// </summary>
        public User User;

        /// <summary>
        /// The proof for the user.
        /// </summary>
        public Proof Proof;
    }
}