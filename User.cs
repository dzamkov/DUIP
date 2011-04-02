using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Represents an actor which can modify data.
    /// </summary>
    public abstract class User : Datum
    {
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

        /// <summary>
        /// Gets a non-unique readable name for the user. The name may be null if it is not defined.
        /// </summary>
        public abstract Query<string> GetShortName();

        public override string ToString()
        {
            return this.GetShortName().Execute();
        }
    }
}