using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Represents a entity which can request and modify data, and execute operations.
    /// </summary>
    public abstract class Actor : Content
    {
        
    }

    /// <summary>
    /// An actor representing a collection of other actors that can act as it.
    /// </summary>
    public abstract class Group : Actor
    {
        /// <summary>
        /// Gets if the specified actor is a member of this group.
        /// </summary>
        public abstract Query<bool> IsMember(Actor Actor);
    }

    /// <summary>
    /// A group with every actor as a member. Any actor can act under the universal group.
    /// </summary>
    public abstract class UniversalGroup : Group
    {
        public override Query<bool> IsMember(Actor Actor)
        {
            return true;
        }
    }

    /// <summary>
    /// A group with no actors as members. This actor will not be able to issue any commands.
    /// </summary>
    public abstract class NullGroup : Group
    {
        public override Query<bool> IsMember(Actor Actor)
        {
            return false;    
        }
    }
}