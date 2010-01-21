//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUIP.Core
{
    /// <summary>
    /// An object, or a representation of a machine or a collection of other
    /// entities that can issue commands. This is the basis for security over networks
    /// and between applications.
    /// </summary>
    public abstract class Entity : Resource
    {
        public Entity(World World) : base(World)
        {

        }

    }
}
