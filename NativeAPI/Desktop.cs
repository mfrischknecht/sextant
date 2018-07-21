using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sextant.NativeAPI
{
    public static class Desktop
    {
        #region System metrics (various variables)
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        public enum SystemMetric
        {
            SM_CXSCREEN = 0, // 0x00
            SM_CYSCREEN = 1, // 0x01
            SM_CXVSCROLL = 2, // 0x02
            SM_CYHSCROLL = 3, // 0x03
            SM_CYCAPTION = 4, // 0x04
            SM_CXBORDER = 5, // 0x05
            SM_CYBORDER = 6, // 0x06
            SM_CXDLGFRAME = 7, // 0x07
            SM_CXFIXEDFRAME = 7, // 0x07
            SM_CYDLGFRAME = 8, // 0x08
            SM_CYFIXEDFRAME = 8, // 0x08
            SM_CYVTHUMB = 9, // 0x09
            SM_CXHTHUMB = 10, // 0x0A
            SM_CXICON = 11, // 0x0B
            SM_CYICON = 12, // 0x0C
            SM_CXCURSOR = 13, // 0x0D
            SM_CYCURSOR = 14, // 0x0E
            SM_CYMENU = 15, // 0x0F
            SM_CXFULLSCREEN = 16, // 0x10
            SM_CYFULLSCREEN = 17, // 0x11
            SM_CYKANJIWINDOW = 18, // 0x12
            SM_MOUSEPRESENT = 19, // 0x13
            SM_CYVSCROLL = 20, // 0x14
            SM_CXHSCROLL = 21, // 0x15
            SM_DEBUG = 22, // 0x16
            SM_SWAPBUTTON = 23, // 0x17
            SM_CXMIN = 28, // 0x1C
            SM_CYMIN = 29, // 0x1D
            SM_CXSIZE = 30, // 0x1E
            SM_CYSIZE = 31, // 0x1F
            SM_CXSIZEFRAME = 32, // 0x20
            SM_CXFRAME = 32, // 0x20
            SM_CYSIZEFRAME = 33, // 0x21
            SM_CYFRAME = 33, // 0x21
            SM_CXMINTRACK = 34, // 0x22
            SM_CYMINTRACK = 35, // 0x23
            SM_CXDOUBLECLK = 36, // 0x24
            SM_CYDOUBLECLK = 37, // 0x25
            SM_CXICONSPACING = 38, // 0x26
            SM_CYICONSPACING = 39, // 0x27
            SM_MENUDROPALIGNMENT = 40, // 0x28
            SM_PENWINDOWS = 41, // 0x29
            SM_DBCSENABLED = 42, // 0x2A
            SM_CMOUSEBUTTONS = 43, // 0x2B
            SM_SECURE = 44, // 0x2C
            SM_CXEDGE = 45, // 0x2D
            SM_CYEDGE = 46, // 0x2E
            SM_CXMINSPACING = 47, // 0x2F
            SM_CYMINSPACING = 48, // 0x30
            SM_CXSMICON = 49, // 0x31
            SM_CYSMICON = 50, // 0x32
            SM_CYSMCAPTION = 51, // 0x33
            SM_CXSMSIZE = 52, // 0x34
            SM_CYSMSIZE = 53, // 0x35
            SM_CXMENUSIZE = 54, // 0x36
            SM_CYMENUSIZE = 55, // 0x37
            SM_ARRANGE = 56, // 0x38
            SM_CXMINIMIZED = 57, // 0x39
            SM_CYMINIMIZED = 58, // 0x3A
            SM_CXMAXTRACK = 59, // 0x3B
            SM_CYMAXTRACK = 60, // 0x3C
            SM_CXMAXIMIZED = 61, // 0x3D
            SM_CYMAXIMIZED = 62, // 0x3E
            SM_NETWORK = 63, // 0x3F
            SM_CLEANBOOT = 67, // 0x43
            SM_CXDRAG = 68, // 0x44
            SM_CYDRAG = 69, // 0x45
            SM_SHOWSOUNDS = 70, // 0x46
            SM_CXMENUCHECK = 71, // 0x47
            SM_CYMENUCHECK = 72, // 0x48
            SM_SLOWMACHINE = 73, // 0x49
            SM_MIDEASTENABLED = 74, // 0x4A
            SM_MOUSEWHEELPRESENT = 75, // 0x4B
            SM_XVIRTUALSCREEN = 76, // 0x4C
            SM_YVIRTUALSCREEN = 77, // 0x4D
            SM_CXVIRTUALSCREEN = 78, // 0x4E
            SM_CYVIRTUALSCREEN = 79, // 0x4F
            SM_CMONITORS = 80, // 0x50
            SM_SAMEDISPLAYFORMAT = 81, // 0x51
            SM_IMMENABLED = 82, // 0x52
            SM_CXFOCUSBORDER = 83, // 0x53
            SM_CYFOCUSBORDER = 84, // 0x54
            SM_TABLETPC = 86, // 0x56
            SM_MEDIACENTER = 87, // 0x57
            SM_STARTER = 88, // 0x58
            SM_SERVERR2 = 89, // 0x59
            SM_MOUSEHORIZONTALWHEELPRESENT = 91, // 0x5B
            SM_CXPADDEDBORDER = 92, // 0x5C
            SM_DIGITIZER = 94, // 0x5E
            SM_MAXIMUMTOUCHES = 95, // 0x5F

            SM_REMOTESESSION = 0x1000, // 0x1000
            SM_SHUTTINGDOWN = 0x2000, // 0x2000
            SM_REMOTECONTROL = 0x2001, // 0x2001

            SM_CONVERTABLESLATEMODE = 0x2003,
            SM_SYSTEMDOCKED = 0x2004,
        }
        #endregion

        #region Monitor info (size, position etc)
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        public const int CCHDEVICENAME = 32;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MonitorInfoEx
        {
            public int Size;
            public RectStruct Monitor;
            public RectStruct WorkArea;
            public uint Flags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;

            public void Init()
            {
                this.Size = 40 + 2 * CCHDEVICENAME;
                this.DeviceName = string.Empty;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RectStruct
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion

        #region Enumerate displays
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(
            string lpDevice, uint iDevNum, ref DisplayDevice lpDisplayDevice, uint dwFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DisplayDevice
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceState StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags()]
        public enum DisplayDeviceState : int
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VGACompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }
        #endregion

        #region Enumerate virtual monitors
        public static IDictionary<IntPtr,Rect> GetMonitors()
        {
            var monitors = new Dictionary<IntPtr,Rect>();
            MonitorEnumCallback @delegate =
                (IntPtr monitor, IntPtr context, ref Rect bounds, IntPtr data) => {
                    monitors[monitor] = bounds;
                    return true;
                };
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, @delegate, IntPtr.Zero);
            return monitors;
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(
            IntPtr hdc, IntPtr lprcClip, MonitorEnumCallback lpfnEnum, IntPtr dwData);

        public delegate bool MonitorEnumCallback(
            IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public class DisplayInfo
        {
            public string Availability { get; set; }
            public string ScreenHeight { get; set; }
            public string ScreenWidth  { get; set; }
            public Rect   MonitorArea  { get; set; }
            public Rect   WorkArea     { get; set; }
        }
        #endregion

        #region Workspaces
        public delegate bool EnumDesktopProc(string lpszDesktop, IntPtr lParam);
        public delegate bool EnumDesktopWindowsProc(IntPtr windowHandle, IntPtr lParam);

        public static List<string> GetWindowStationDesktopNames(IntPtr windowStation)
        {
            if (windowStation == IntPtr.Zero) throw new ArgumentNullException(nameof(windowStation));

            var desktops = new List<string>();
            EnumDesktopProc @delegate =
                (name, p) => {
                    desktops.Add(name);
                    return true;
                };
            EnumDesktops(windowStation, @delegate, IntPtr.Zero);
            return desktops;
        }

        public static List<IntPtr> GetDesktopWindows(IntPtr desktop)
        {
            if (desktop == IntPtr.Zero) throw new ArgumentNullException(nameof(desktop));

            var windows = new List<IntPtr>();
            EnumDesktopWindowsProc @delegate = 
                (window,p) => {
                    windows.Add(window);
                    return true;
                };
            EnumDesktopWindows(desktop,@delegate,IntPtr.Zero);
            return windows;
        }

        [Flags]
        public enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000f0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001f0000,

            SPECIFIC_RIGHTS_ALL = 0x0000ffff,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,
            DESKTOP_ALL = (DESKTOP_READOBJECTS | DESKTOP_CREATEWINDOW | DESKTOP_CREATEMENU |
                        DESKTOP_HOOKCONTROL | DESKTOP_JOURNALRECORD | DESKTOP_JOURNALPLAYBACK |
                        DESKTOP_ENUMERATE | DESKTOP_WRITEOBJECTS | DESKTOP_SWITCHDESKTOP |
                        STANDARD_RIGHTS_REQUIRED),

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037f
        }

        [DllImport("user32.dll", EntryPoint = "CreateDesktop", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateDesktop(
                        [MarshalAs(UnmanagedType.LPWStr)] string desktopName,
                        /*[MarshalAs(UnmanagedType.LPWStr)] */ IntPtr device, // must be null.
                        /*[MarshalAs(UnmanagedType.LPWStr)] */ IntPtr deviceMode, // must be null,
                        [MarshalAs(UnmanagedType.U4)] int flags,  // use 0
                        [MarshalAs(UnmanagedType.U4)] ACCESS_MASK accessMask,
                        /*[MarshalAs(UnmanagedType.LPStruct)] */ IntPtr attributes);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, long dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr OpenInputDesktop(int dwFlags, bool fInherit, long dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumDesktops(IntPtr hwinsta, EnumDesktopProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsProc lpfn, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetThreadDesktop(int dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, IntPtr pvInfo, int nLength, ref int lpnLengthNeeded);
        #endregion
    }
}
