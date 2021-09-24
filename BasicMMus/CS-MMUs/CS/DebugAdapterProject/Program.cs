
using MMICSharp.Adapter;
using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;


using DebugAdapter;
using ReachMMU;
using ReachMMUConcurrent;


namespace DebugAdapterProject
{
    class Program
    {
        static void Main(string[] args)
        {
            ReachMMUConcurrentImpl rm = new ReachMMUConcurrentImpl();
            DebugAdapter.DebugAdapter a = new DebugAdapter.DebugAdapter(rm.GetType());
            a.Start();
            
        }
    }
}
