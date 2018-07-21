using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sextant.NativeAPI
{
    public static class Processes
    {
        [DllImport("psapi.dll")]
        static extern uint GetProcessImageFileName(
            IntPtr process, 
            StringBuilder imageFileName, 
            int size
        );
    }
}
