using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sextant.NativeAPI
{
    public static class DwmThumbnails
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out Size size);

        [StructLayout(LayoutKind.Sequential)]
        public struct Size
        {
            public int x;
            public int y;
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref ThumbnailProperties props);

        [StructLayout(LayoutKind.Sequential)]
        public struct ThumbnailProperties
        {
            public uint Flags;
            public Rect Destination;
            public Rect Source;
            public byte Opacity;
            public bool Visible;
            public bool SourceClientAreaOnly;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public const uint DWM_TNP_RECTDESTINATION      = 0x00000001;
        public const uint DWM_TNP_RECTSOURCE           = 0x00000002;
        public const uint DWM_TNP_OPACITY              = 0x00000004;
        public const uint DWM_TNP_VISIBLE              = 0x00000008;
        public const uint DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;
    }
}
