using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

using Sextant.NativeAPI.ComImports;

namespace Sextant.NativeAPI
{
    public static class VirtualDesktop
    {
        private static IVirtualDesktopPinnedApps VirtualDesktopPinnedApps;
        private static IApplicationViewCollection ApplicationViewCollection;
        private static IVirtualDesktopManager VirtualDesktopManager;
        private static IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
        private static IVirtualDesktopManagerInternal2 VirtualDesktopManagerInternal2;

        static VirtualDesktop()
		{
			var shell = (IServiceProvider10) 
                Activator.CreateInstance(
                    Type.GetTypeFromCLSID(ClassIdentifiers.ImmersiveShell));

			VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)
                shell.QueryService(
                    ClassIdentifiers.VirtualDesktopManagerInternal, 
                    typeof(IVirtualDesktopManagerInternal).GUID);

			try
            { 
                VirtualDesktopManagerInternal2 = (IVirtualDesktopManagerInternal2)
                    shell.QueryService(
                        ClassIdentifiers.VirtualDesktopManagerInternal,
                        typeof(IVirtualDesktopManagerInternal2).GUID); 
            }
			catch { }

			VirtualDesktopManager = (IVirtualDesktopManager)
                Activator.CreateInstance(
                    Type.GetTypeFromCLSID(ClassIdentifiers.VirtualDesktopManager));

			ApplicationViewCollection = (IApplicationViewCollection)
                shell.QueryService(
                    typeof(IApplicationViewCollection).GUID, 
                    typeof(IApplicationViewCollection).GUID);

			VirtualDesktopPinnedApps = (IVirtualDesktopPinnedApps)
                shell.QueryService(
                    ClassIdentifiers.VirtualDesktopPinnedApps, 
                    typeof(IVirtualDesktopPinnedApps).GUID);
        }
        
        #region Access desktops by index
        private static IVirtualDesktop GetDestkopAt(int index)
        {
            if (index < 0) return null;
            VirtualDesktopManagerInternal.GetDesktops(out var desktops);
            desktops.GetCount(out var count);
            if (index >= count) return null;
            desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out var desktop);
            return (IVirtualDesktop)desktop;
        }
        private static IVirtualDesktop GetFirstDesktop() => GetDestkopAt(0);
        private static IVirtualDesktop GetLastDesktop() 
            => GetDestkopAt(VirtualDesktopManagerInternal.GetCount()-1);
        
        private static IVirtualDesktop GetAdjacentDesktop(int direction)
        {
            var current = VirtualDesktopManagerInternal.GetCurrentDesktop();
            VirtualDesktopManagerInternal.GetAdjacentDesktop(current, direction, out var next);
            Marshal.ReleaseComObject(current);
            return next;
        }
        private static IVirtualDesktop GetNextDesktop() => GetAdjacentDesktop(4);
        private static IVirtualDesktop GetPreviousDesktop() => GetAdjacentDesktop(3);
        #endregion
        
        #region Switch between desktops
        private static void SwitchToDesktop(IVirtualDesktop desktop)
            => VirtualDesktopManagerInternal.SwitchDesktop(desktop);
        
        public static void SwitchToDesktop(int index)
        {
            var desktop = GetDestkopAt(index);
            if (desktop == null) return;
            SwitchToDesktop(desktop);
            Marshal.ReleaseComObject(desktop);
        }

        public static void SwitchToPreviousDesktop()
        {
            var desktop = GetPreviousDesktop() ?? GetLastDesktop();
            if (desktop == null) return;
            SwitchToDesktop(desktop);
            Marshal.ReleaseComObject(desktop);
        }

        public static void SwitchToNextDesktop()
        {
            var desktop = GetNextDesktop() ?? GetFirstDesktop();
            if (desktop == null) return;
            SwitchToDesktop(desktop);
            Marshal.ReleaseComObject(desktop);
        }
        #endregion
        
        #region Window queries
        public static bool IsWindowOnCurrentDesktop(IntPtr windowHandle)
            => VirtualDesktopManager.IsWindowOnCurrentVirtualDesktop(windowHandle);
        #endregion
    }
}
