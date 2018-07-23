using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace Sextant.NativeAPI
{
    public static class Windows
    {

        #region Geometry
        [StructLayout(LayoutKind.Sequential)]
        public struct Rectangle
        {
             public int Left;        // x position of upper-left corner
             public int Top;         // y position of upper-left corner
             public int Right;       // x position of lower-right corner
             public int Bottom;      // y position of lower-right corner
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
             public int X;
             public int Y;
        }
        #endregion

        #region Window iteration
        public delegate bool EnumWindowsCallback(IntPtr window, IntPtr param);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(
            IntPtr parentWindow, EnumWindowsCallback callback, IntPtr param);

        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            var windows = new List<IntPtr>();
            EnumWindowsCallback @delegate =
                (window, p) => {
                    windows.Add(window);
                    return true;
                };
            EnumChildWindows(parent, @delegate, IntPtr.Zero);
            return windows;
        }

        public static List<IntPtr> GetRootWindows()
        {
            return GetChildWindows(IntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        #endregion

        [DllImport("user32.dll", SetLastError=true)]
        public static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

        #region Window event hooks
        public delegate void WinEventCallback(
            IntPtr hookHandle, uint eventType, 
            IntPtr windowHandle, int objectId, int childId, 
            uint threadId, uint timestamp);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(
           uint eventMin, uint eventMax, IntPtr dllHandle, 
           WinEventCallback callback, uint processId,
           uint threadId, uint flags);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hookHandle);

        public const uint EVENT_MIN                                    = 0x00000001;
        public const uint EVENT_MAX                                    = 0x7FFFFFFF;
        public const uint EVENT_OBJECT_ACCELERATORCHANGE               = 0x8012;
        public const uint EVENT_OBJECT_CLOAKED                         = 0x8017;
        public const uint EVENT_OBJECT_CONTENTSCROLLED                 = 0x8015;
        public const uint EVENT_OBJECT_CREATE                          = 0x8000;
        public const uint EVENT_OBJECT_DEFACTIONCHANGE                 = 0x8011;
        public const uint EVENT_OBJECT_DESCRIPTIONCHANGE               = 0x800D;
        public const uint EVENT_OBJECT_DESTROY                         = 0x8001;
        public const uint EVENT_OBJECT_DRAGSTART                       = 0x8021;
        public const uint EVENT_OBJECT_DRAGCANCEL                      = 0x8022;
        public const uint EVENT_OBJECT_DRAGCOMPLETE                    = 0x8023;
        public const uint EVENT_OBJECT_DRAGENTER                       = 0x8024;
        public const uint EVENT_OBJECT_DRAGLEAVE                       = 0x8025;
        public const uint EVENT_OBJECT_DRAGDROPPED                     = 0x8026;
        public const uint EVENT_OBJECT_END                             = 0x80FF;
        public const uint EVENT_OBJECT_FOCUS                           = 0x8005;
        public const uint EVENT_OBJECT_HELPCHANGE                      = 0x8010;
        public const uint EVENT_OBJECT_HIDE                            = 0x8003;
        public const uint EVENT_OBJECT_HOSTEDOBJECTSINVALIDATED        = 0x8020;
        public const uint EVENT_OBJECT_IME_HIDE                        = 0x8028;
        public const uint EVENT_OBJECT_IME_SHOW                        = 0x8027;
        public const uint EVENT_OBJECT_IME_CHANGE                      = 0x8029;
        public const uint EVENT_OBJECT_INVOKED                         = 0x8013;
        public const uint EVENT_OBJECT_LIVEREGIONCHANGED               = 0x8019;
        public const uint EVENT_OBJECT_LOCATIONCHANGE                  = 0x800B;
        public const uint EVENT_OBJECT_NAMECHANGE                      = 0x800C;
        public const uint EVENT_OBJECT_PARENTCHANGE                    = 0x800F;
        public const uint EVENT_OBJECT_REORDER                         = 0x8004;
        public const uint EVENT_OBJECT_SELECTION                       = 0x8006;
        public const uint EVENT_OBJECT_SELECTIONADD                    = 0x8007;
        public const uint EVENT_OBJECT_SELECTIONREMOVE                 = 0x8008;
        public const uint EVENT_OBJECT_SELECTIONWITHIN                 = 0x8009;
        public const uint EVENT_OBJECT_SHOW                            = 0x8002;
        public const uint EVENT_OBJECT_STATECHANGE                     = 0x800A;
        public const uint EVENT_OBJECT_TEXTEDIT_CONVERSIONTARGETCHANED = 0x8030;
        public const uint EVENT_OBJECT_TEXTSELECTIONCHANGED            = 0x8014;
        public const uint EVENT_OBJECT_UNCLOAKED                       = 0x8018;
        public const uint EVENT_OBJECT_VALUECHANGE                     = 0x800E;
        public const uint EVENT_SYSTEM_ALERT                           = 0x0002;
        public const uint EVENT_SYSTEM_ARRANGMENTPREVIEW               = 0x8016;
        public const uint EVENT_SYSTEM_CAPTUREEND                      = 0x0009;
        public const uint EVENT_SYSTEM_CAPTURESTART                    = 0x0008;
        public const uint EVENT_SYSTEM_CONTEXTHELPEND                  = 0x000D;
        public const uint EVENT_SYSTEM_CONTEXTHELPSTART                = 0x000C;
        public const uint EVENT_SYSTEM_DESKTOPSWITCH                   = 0x0020;
        public const uint EVENT_SYSTEM_DIALOGEND                       = 0x0011;
        public const uint EVENT_SYSTEM_DIALOGSTART                     = 0x0010;
        public const uint EVENT_SYSTEM_DRAGDROPEND                     = 0x000F;
        public const uint EVENT_SYSTEM_DRAGDROPSTART                   = 0x000E;
        public const uint EVENT_SYSTEM_END                             = 0x00FF;
        public const uint EVENT_SYSTEM_FOREGROUND                      = 0x0003;
        public const uint EVENT_SYSTEM_MENUPOPUPEND                    = 0x0007;
        public const uint EVENT_SYSTEM_MENUPOPUPSTART                  = 0x0006;
        public const uint EVENT_SYSTEM_MENUEND                         = 0x0005;
        public const uint EVENT_SYSTEM_MENUSTART                       = 0x0004;
        public const uint EVENT_SYSTEM_MINIMIZEEND                     = 0x0017;
        public const uint EVENT_SYSTEM_MINIMIZESTART                   = 0x0016;
        public const uint EVENT_SYSTEM_MOVESIZEEND                     = 0x000B;
        public const uint EVENT_SYSTEM_MOVESIZESTART                   = 0x000A;
        public const uint EVENT_SYSTEM_SCROLLINGEND                    = 0x0013;
        public const uint EVENT_SYSTEM_SCROLLINGSTART                  = 0x0012;
        public const uint EVENT_SYSTEM_SOUND                           = 0x0001;
        public const uint EVENT_SYSTEM_SWITCHEND                       = 0x0015;
        public const uint EVENT_SYSTEM_SWITCHSTART                     = 0x0014;

        public const uint WINEVENT_OUTOFCONTEXT = 0u;
        #endregion

        #region Window dimensions
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr window, out Rectangle rectangle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr window, out Rectangle rectangle);
        #endregion

        #region Window visibility
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr window);
        #endregion

        #region Window title
        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int GetWindowText(IntPtr window, StringBuilder text, int maxCharacters);
        #endregion

        #region Class name
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        #endregion

        #region Window placement (getter)
        [DllImport("user32.dll")] 
        public static extern bool GetWindowPlacement(IntPtr window, ref Placement placement);

        public struct Placement
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }
        #endregion

        #region Window positioning
        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            None                        =      0,
            AsynchronousWindowPosition  = 0x4000,
            DeferErase                  = 0x2000,
            DrawFrame                   = 0x0020,
            FrameChanged                = 0x0020,
            HideWindow                  = 0x0080,
            DoNotActivate               = 0x0010,
            DoNotCopyBits               = 0x0100,
            IgnoreMove                  = 0x0002,
            DoNotChangeOwnerZOrder      = 0x0200,
            DoNotRedraw                 = 0x0008,
            DoNotReposition             = 0x0200,
            DoNotSendChangingEvent      = 0x0400,
            IgnoreResize                = 0x0001,
            IgnoreZOrder                = 0x0004,
            ShowWindow                  = 0x0040,
        }
        #endregion

        #region Icon getter
        public enum GCLPIndex : int
        {
            GCL_HICONSM =  -34,
            GCL_HICON   =  -14,
        }

        public enum SMMessage : int
        {
            WM_GETICON  = 0x7F,
        }

        public enum SMIndex : int
        {
            ICON_SMALL  = 0,
            ICON_BIG    = 1,
            ICON_SMALL2 = 2,
        }

        public static IntPtr GetClassLongPtr(IntPtr hWnd, GCLPIndex nIndex)
        {
          if (IntPtr.Size > 4)
            return GetClassLongPtr64(hWnd, nIndex);
          else
            return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        }
         
        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, GCLPIndex nIndex);
         
        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, GCLPIndex nIndex);
         
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, SMMessage Msg, SMIndex wParam, int lParam);
        #endregion

        #region Window activation
        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        #region Show/restore/maximize etc. windows
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr window, ShowWindowCommands command);

        public enum ShowWindowCommands : int
        {
            Hide            =  0,
            Normal          =  1,
            ShowMinimized   =  2,
            Maximize        =  3,
            ShowMaximized   =  3,
            ShowNoActivate  =  4,
            Show            =  5,
            Minimize        =  6,
            ShowMinNoActive =  7,
            ShowNA          =  8,
            Restore         =  9,
            ShowDefault     = 10,
            ForceMinimize   = 11,
        }
        #endregion

        #region Attach to other windows' threads
        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        #endregion

        #region Access window properties
        [DllImport("user32.dll", EntryPoint="GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, WindowLongIndex nIndex);

        [DllImport("user32.dll", EntryPoint="GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, WindowLongIndex nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, WindowLongIndex nIndex)
        {
             if (IntPtr.Size == 8)
                 return GetWindowLongPtr64(hWnd, nIndex);
             else
                 return GetWindowLongPtr32(hWnd, nIndex);
        }

        public enum WindowLongIndex : int
        {
            GWL_EXSTYLE     = -20,
            GWLP_HINSTANCE  = - 6,
            GWLP_HWNDPARENT = - 8,
            GWLP_ID         = -12,
            GWL_STYLE       = -16,
            GWLP_USERDATA   = -21,
            GWLP_WNDPROC    = - 4,
        }

        [Flags]
        public enum WindowStyle : long
        {
            WS_BORDER       = 0x00800000L,
            WS_CAPTION      = 0x00C00000L,
            WS_CHILD        = 0x40000000L,
            WS_CHILDWINDOW  = 0x40000000L,
            WS_CLIPCHILDREN = 0x02000000L,
            WS_CLIPSIBLINGS = 0x04000000L,
            WS_DISABLED     = 0x08000000L,
            WS_DLGFRAME     = 0x00400000L,
            WS_GROUP        = 0x00020000L,
            WS_HSCROLL      = 0x00100000L,
            WS_ICONIC       = 0x20000000L,
            WS_MAXIMIZE     = 0x01000000L,
            WS_MAXIMIZEBOX  = 0x00010000L,
            WS_MINIMIZE     = 0x20000000L,
            WS_MINIMIZEBOX  = 0x00020000L,
            WS_OVERLAPPED   = 0x00000000L,
            WS_POPUP        = 0x80000000L,
            WS_SIZEBOX      = 0x00040000L,
            WS_SYSMENU      = 0x00080000L,
            WS_TABSTOP      = 0x00010000L,
            WS_THICKFRAME   = 0x00040000L,
            WS_TILED        = 0x00000000L,
            WS_VISIBLE      = 0x10000000L,
            WS_VSCROLL      = 0x00200000L,

            WS_OVERLAPPEDWINDOW = 
                  WS_OVERLAPPED | WS_CAPTION     | WS_SYSMENU 
                | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

            WS_TILEDWINDOW = 
                  WS_OVERLAPPED | WS_CAPTION     | WS_SYSMENU 
                | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        }

        [Flags]
        public enum ExtendedWindowStyle : long
        {
            WS_EX_ACCEPTFILES         = 0x00000010L,
            WS_EX_APPWINDOW           = 0x00040000L,
            WS_EX_CLIENTEDGE          = 0x00000200L,
            WS_EX_COMPOSITED          = 0x02000000L,
            WS_EX_CONTEXTHELP         = 0x00000400L,
            WS_EX_CONTROLPARENT       = 0x00010000L,
            WS_EX_DLGMODALFRAME       = 0x00000001L,
            WS_EX_LAYERED             = 0x00080000,
            WS_EX_LAYOUTRTL           = 0x00400000L,
            WS_EX_LEFT                = 0x00000000L,
            WS_EX_LEFTSCROLLBAR       = 0x00004000L,
            WS_EX_LTRREADING          = 0x00000000L,
            WS_EX_MDICHILD            = 0x00000040L,
            WS_EX_NOACTIVATE          = 0x08000000L,
            WS_EX_NOINHERITLAYOUT     = 0x00100000L,
            WS_EX_NOPARENTNOTIFY      = 0x00000004L,
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000L,
            WS_EX_RIGHT               = 0x00001000L,
            WS_EX_RIGHTSCROLLBAR      = 0x00000000L,
            WS_EX_RTLREADING          = 0x00002000L,
            WS_EX_STATICEDGE          = 0x00020000L,
            WS_EX_TOOLWINDOW          = 0x00000080L,
            WS_EX_TOPMOST             = 0x00000008L,
            WS_EX_TRANSPARENT         = 0x00000020L,
            WS_EX_WINDOWEDGE          = 0x00000100L,

            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
        }
        #endregion

        #region Get monitor for window
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, DefaultMonitor @default);
        public enum DefaultMonitor : uint
        {
            DefaultToNull    = 0,
            DefaultToPrimary = 1,
            DefaultToNearest = 2,
        }
        #endregion
    }
}
