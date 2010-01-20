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
    /// A sandboxed application that works on a spatial area.
    /// </summary>
    public class App
    {

        private object _GlobalData;
        private Sector[,] _Space;
        private Visual.Section _Section;
    }
}
