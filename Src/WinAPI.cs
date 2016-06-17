using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using D = System.Drawing;

// this file is largely sourced from caesay/Clowd with permission
// http://clowd.ca

namespace ScreenVersusWpf
{
    internal static class WinAPI
    {
        #region Functions

        // GDI32
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, DEVICECAP nIndex);

        // USER32
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetProcessDPIAware();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromRect(ref RECT lprc, MonitorOptions dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOptions dwFlags);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        // SHCORE
        [DllImport("shcore.dll")]
        public static extern bool GetDpiForMonitor(IntPtr hMonitor, MONITOR_DPI_TYPE dpiType, ref uint dpiX, ref uint dpiY);

        [DllImport("shcore.dll")]
        public static extern int SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);

        [DllImport("shcore.dll")]
        public static extern int GetProcessDpiAwareness(IntPtr hProcess, ref PROCESS_DPI_AWARENESS value);

        // Helpers
        public static ushort GetLoWord(uint dword)
        {
            return (ushort)(dword & 0xffff);
        }

        public static ushort GetHiWord(uint dword)
        {
            return (ushort)(dword >> 16);
        }

        #endregion Functions

        #region Enums / constants

        public enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        public enum PROCESS_DPI_AWARENESS
        {
            PROCESS_DPI_UNAWARE = 0,
            PROCESS_SYSTEM_DPI_AWARE = 1,
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }

        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }

        public enum DEVICECAP
        {
            LOGPIXELSX = 88,
            LOGPIXELSY = 90,
            VERTRES = 10,
            DESKTOPVERTRES = 117,
            DESKTOPHORZRES = 118,
        }

        #endregion Enums / constants

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public static implicit operator D.Point(POINT pt) { return new D.Point(pt.x, pt.y); }
            public static implicit operator POINT(D.Point pt) { return new POINT { x = pt.X, y = pt.Y }; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public bool HasSize()
            {
                return right - left > 0 && bottom - top > 0;
            }

            public static implicit operator D.Rectangle(RECT rect)
            {
                return new D.Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            }

            public static implicit operator RECT(D.Rectangle rect)
            {
                return new RECT
                {
                    left = rect.X,
                    top = rect.Y,
                    right = rect.X + rect.Width,
                    bottom = rect.Y + rect.Height
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public uint cbSize;

            public RECT rcMonitor;

            public RECT rcWork;

            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            [System.Runtime.InteropServices.FieldOffset(0)]
            public string dmDeviceName;
            [System.Runtime.InteropServices.FieldOffset(32)]
            public Int16 dmSpecVersion;
            [System.Runtime.InteropServices.FieldOffset(34)]
            public Int16 dmDriverVersion;
            [System.Runtime.InteropServices.FieldOffset(36)]
            public Int16 dmSize;
            [System.Runtime.InteropServices.FieldOffset(38)]
            public Int16 dmDriverExtra;
            [System.Runtime.InteropServices.FieldOffset(40)]
            public DM dmFields;

            [System.Runtime.InteropServices.FieldOffset(44)]
            Int16 dmOrientation;
            [System.Runtime.InteropServices.FieldOffset(46)]
            Int16 dmPaperSize;
            [System.Runtime.InteropServices.FieldOffset(48)]
            Int16 dmPaperLength;
            [System.Runtime.InteropServices.FieldOffset(50)]
            Int16 dmPaperWidth;
            [System.Runtime.InteropServices.FieldOffset(52)]
            Int16 dmScale;
            [System.Runtime.InteropServices.FieldOffset(54)]
            Int16 dmCopies;
            [System.Runtime.InteropServices.FieldOffset(56)]
            Int16 dmDefaultSource;
            [System.Runtime.InteropServices.FieldOffset(58)]
            Int16 dmPrintQuality;

            [System.Runtime.InteropServices.FieldOffset(44)]
            public POINT dmPosition;
            [System.Runtime.InteropServices.FieldOffset(52)]
            public Int32 dmDisplayOrientation;
            [System.Runtime.InteropServices.FieldOffset(56)]
            public Int32 dmDisplayFixedOutput;

            [System.Runtime.InteropServices.FieldOffset(60)]
            public short dmColor;
            [System.Runtime.InteropServices.FieldOffset(62)]
            public short dmDuplex;
            [System.Runtime.InteropServices.FieldOffset(64)]
            public short dmYResolution;
            [System.Runtime.InteropServices.FieldOffset(66)]
            public short dmTTOption;
            [System.Runtime.InteropServices.FieldOffset(68)]
            public short dmCollate;
            [System.Runtime.InteropServices.FieldOffset(72)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            [System.Runtime.InteropServices.FieldOffset(102)]
            public Int16 dmLogPixels;
            [System.Runtime.InteropServices.FieldOffset(104)]
            public Int32 dmBitsPerPel;
            [System.Runtime.InteropServices.FieldOffset(108)]
            public Int32 dmPelsWidth;
            [System.Runtime.InteropServices.FieldOffset(112)]
            public Int32 dmPelsHeight;
            [System.Runtime.InteropServices.FieldOffset(116)]
            public Int32 dmDisplayFlags;
            [System.Runtime.InteropServices.FieldOffset(116)]
            public Int32 dmNup;
            [System.Runtime.InteropServices.FieldOffset(120)]
            public Int32 dmDisplayFrequency;
        }

        [Flags()]
        public enum DM : int
        {
            Orientation = 0x1,
            PaperSize = 0x2,
            PaperLength = 0x4,
            PaperWidth = 0x8,
            Scale = 0x10,
            Position = 0x20,
            NUP = 0x40,
            DisplayOrientation = 0x80,
            Copies = 0x100,
            DefaultSource = 0x200,
            PrintQuality = 0x400,
            Color = 0x800,
            Duplex = 0x1000,
            YResolution = 0x2000,
            TTOption = 0x4000,
            Collate = 0x8000,
            FormName = 0x10000,
            LogPixels = 0x20000,
            BitsPerPixel = 0x40000,
            PelsWidth = 0x80000,
            PelsHeight = 0x100000,
            DisplayFlags = 0x200000,
            DisplayFrequency = 0x400000,
            ICMMethod = 0x800000,
            ICMIntent = 0x1000000,
            MediaType = 0x2000000,
            DitherType = 0x4000000,
            PanningWidth = 0x8000000,
            PanningHeight = 0x10000000,
            DisplayFixedOutput = 0x20000000
        }

        internal enum WindowMessage
        {
            WM_DPICHANGED = 0x02E0,

            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,

            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,

            WM_ACTIVATE = 0x0006,

            WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
        }

        #endregion Structs
    }
}
