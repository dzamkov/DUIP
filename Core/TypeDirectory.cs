//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DUIP.Core
{
    /// <summary>
    /// Helper class for getting id's for types.
    /// </summary>
    public static class TypeDirectory
    {
        static TypeDirectory()
        {
            _FowardAssemblyLookup = new Dictionary<ID, AssemblyInfo>();
            _AssemblyLookup = new Dictionary<Assembly, AssemblyInfo>();
            _ReverseTypeLookup = new Dictionary<ID, Type>();
        }

        /// <summary>
        /// Gets the type id for the specified type.
        /// </summary>
        /// <param name="Type">The type to get the type id for.</param>
        /// <returns>An id uniquely representing that type across the network.</returns>
        public static ID GetIDForType(Type Type)
        {
            AssemblyInfo ai;
            if (_AssemblyLookup.TryGetValue(Type.Assembly, out ai))
            {
                ID id;
                if (ai.FowardTypeLookup.TryGetValue(Type, out id))
                {
                    return id;
                }
            }
            throw new Exception("Type not loaded");
        }

        /// <summary>
        /// Gets a type by its id.
        /// </summary>
        /// <param name="ID">The id of the type to get.</param>
        /// <returns>The type indexed by the specified id.</returns>
        public static Type GetTypeByID(ID ID)
        {
            Type t;
            if (_ReverseTypeLookup.TryGetValue(ID, out t))
            {
                return t;
            }
            throw new Exception("No type for id.");
        }

        /// <summary>
        /// Loads an assembly and records all types and the associated id's found in there.
        /// </summary>
        /// <param name="Assem">The assembly to load.</param>
        public static void LoadAssembly(Assembly Assem)
        {
            if (!_AssemblyLookup.ContainsKey(Assem))
            {
                ID key = ID.Hash("ASSEMBLY:" + Assem.FullName);
                AssemblyInfo ai = new AssemblyInfo(Assem, key);
                if (!_FowardAssemblyLookup.ContainsKey(key))
                {
                    ai.Load();
                    _AssemblyLookup[Assem] = ai;
                    _FowardAssemblyLookup[ai.AssemblyID] = ai;
                }
                else
                {
                    throw new Exception("Unique hash fail");
                }
            }
            else
            {
                throw new Exception("Assembly already loaded!");
            }
        }

        /// <summary>
        /// Unloads an assembly and removes the id's for all types there.
        /// </summary>
        /// <param name="Assem">The assembly to unload.</param>
        public static void UnloadAssembly(Assembly Assem)
        {
            AssemblyInfo ai;
            if (_AssemblyLookup.TryGetValue(Assem, out ai))
            {
                ai.Unload();
                _AssemblyLookup.Remove(Assem);
                _FowardAssemblyLookup.Remove(ai.AssemblyID);
            }
        }

        /// <summary>
        /// Information about an assembly.
        /// </summary>
        private class AssemblyInfo
        {
            public AssemblyInfo(Assembly Assembly, ID ID)
            {
                this.Assembly = Assembly;
                this.AssemblyID = ID;
            }

            /// <summary>
            /// Loads the assemblies types.
            /// </summary>
            public void Load()
            {
                foreach (Type t in this.Assembly.GetTypes())
                {
                    ID typeid = ID.Hash(new ID[] { this.AssemblyID, ID.Hash("TYPE: " + t.FullName) });
                    this.FowardTypeLookup[t] = typeid;
                    _ReverseTypeLookup[typeid] = t;
                }
            }

            /// <summary>
            /// Unloads the assemblies types.
            /// </summary>
            public void Unload()
            {
                foreach (KeyValuePair<Type, ID> kvp in this.FowardTypeLookup)
                {
                    _ReverseTypeLookup.Remove(kvp.Value);
                }
            }

            /// <summary>
            /// Identifier for this assembly. 
            /// </summary>
            public ID AssemblyID;

            /// <summary>
            /// The actual assembly.
            /// </summary>
            public Assembly Assembly;

            /// <summary>
            /// Foward lookup table to get the id's for the types defined in
            /// this assembly.
            /// </summary>
            public Dictionary<Type, ID> FowardTypeLookup;
        }

        private static Dictionary<ID, AssemblyInfo> _FowardAssemblyLookup;
        private static Dictionary<Assembly, AssemblyInfo> _AssemblyLookup;
        private static Dictionary<ID, Type> _ReverseTypeLookup;
    }
}
