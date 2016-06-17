using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

// this file is largely sourced from caesay/Clowd with permission
// http://clowd.ca

namespace ScreenVersusWpf
{
    public static class DpiUtil
    {
        public static readonly Dpi SystemDpi = GetSystemDpi();
        public static readonly Dpi RealDpi = GetRealDpi();

        public static Dpi GetDpiFromVisual(Visual sourceVisual)
        {
            if (sourceVisual == null)
                throw new ArgumentNullException(nameof(sourceVisual));

            var source = PresentationSource.FromVisual(sourceVisual);
            if (source?.CompositionTarget == null)
                return Dpi.Default;

            return new Dpi(
                (int)(Dpi.Default.DpiX * source.CompositionTarget.TransformToDevice.M11),
                (int)(Dpi.Default.DpiY * source.CompositionTarget.TransformToDevice.M22));
        }

        public static Dpi GetDpiFromWindowMonitor(Window sourceVisual)
        {
            if (sourceVisual == null)
                throw new ArgumentNullException(nameof(sourceVisual));

            if (GetCurrentAwareness() != WinAPI.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE)
                return SystemDpi;

            var source = PresentationSource.FromVisual(sourceVisual) as HwndSource;
            if (source == null)
                return SystemDpi;

            var handleMonitor = WinAPI.MonitorFromWindow(source.Handle,
                WinAPI.MonitorOptions.MONITOR_DEFAULTTONEAREST);

            return GetMonitorDpi(handleMonitor);
        }

        public static Dpi GetDpiFromRectMonitor(Rect sourceRect)
        {
            if (sourceRect == Rect.Empty)
                throw new ArgumentNullException(nameof(sourceRect));

            if (GetCurrentAwareness() != WinAPI.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE)
                return SystemDpi;

            var nativeRect = new WinAPI.RECT
            {
                left = (int)sourceRect.Left,
                bottom = (int)sourceRect.Bottom,
                right = (int)sourceRect.Right,
                top = (int)sourceRect.Top
            };
            var handleMonitor = WinAPI.MonitorFromRect(ref nativeRect,
                WinAPI.MonitorOptions.MONITOR_DEFAULTTONEAREST);

            return GetMonitorDpi(handleMonitor);
        }

        public static Dpi GetDpiFromNotifyArea()
        {
            if (!ShcoreAvailable())
                return SystemDpi;

            var handleTaskBar = WinAPI.FindWindowEx(
                IntPtr.Zero,
                IntPtr.Zero,
                "Shell_TrayWnd",
                string.Empty);

            if (handleTaskBar == IntPtr.Zero)
                return SystemDpi;

            var handleNotificationArea = WinAPI.FindWindowEx(
                handleTaskBar,
                IntPtr.Zero,
                "TrayNotifyWnd",
                string.Empty);

            if (handleNotificationArea == IntPtr.Zero)
                return SystemDpi;

            var handleMonitor = WinAPI.MonitorFromWindow(
                handleNotificationArea,
                WinAPI.MonitorOptions.MONITOR_DEFAULTTOPRIMARY);

            return GetMonitorDpi(handleMonitor);
        }

        #region Internal Methods


        private static bool? _shcoreCached = null;
        internal static bool ShcoreAvailable()
        {
            if (_shcoreCached.HasValue)
                return _shcoreCached.Value;

            try
            {
                // best way to detect this is to just try and see if we can load the assembly by calling a method
                WinAPI.PROCESS_DPI_AWARENESS sh_aware = WinAPI.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;
                WinAPI.GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, ref sh_aware);

                // if we get here, shcore is available.
                _shcoreCached = true;
                return true;
            }
            catch (DllNotFoundException)
            { }

            _shcoreCached = false;
            return false;
        }

        internal static Dpi GetMonitorDpi(IntPtr handleMonitor)
        {
            if (handleMonitor == IntPtr.Zero)
                return SystemDpi;

            if (!ShcoreAvailable())
                return SystemDpi;

            uint dpiX = 1;
            uint dpiY = 1;

            var result = WinAPI.GetDpiForMonitor(
                handleMonitor,
                WinAPI.MONITOR_DPI_TYPE.MDT_DEFAULT,
                ref dpiX,
                ref dpiY);

            if (!result)
                return SystemDpi;

            return new Dpi((int)dpiX, (int)dpiY);
        }

        internal static Dpi GetSystemDpi()
        {
            IntPtr dc = WinAPI.GetDC(IntPtr.Zero);
            int dpiX = WinAPI.GetDeviceCaps(dc, WinAPI.DEVICECAP.LOGPIXELSX);
            int dpiY = WinAPI.GetDeviceCaps(dc, WinAPI.DEVICECAP.LOGPIXELSY);
            WinAPI.ReleaseDC(IntPtr.Zero, dc);
            return new Dpi(dpiX, dpiY);
        }

        internal static Dpi GetRealDpi()
        {
            if (GetCurrentAwareness() == WinAPI.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE)
            {
                var dpi = GetComparitiveMonitorDpiScale(Screen.PrimaryScreen) * 96;
                return new Dpi((int)dpi, (int)dpi);
            }
            else
            {
                return SystemDpi;
            }
        }

        internal static WinAPI.PROCESS_DPI_AWARENESS GetCurrentAwareness()
        {
            // try to retrieve awareness with SHCORE first. Will fail on < win8_1
            if (_shcoreCached != false)
            {
                try
                {
                    WinAPI.PROCESS_DPI_AWARENESS sh_aware = WinAPI.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;
                    int s_result = WinAPI.GetProcessDpiAwareness(IntPtr.Zero, ref sh_aware);
                    if (s_result == 0)
                        return sh_aware;
                }
                catch (DllNotFoundException)
                {
                }
            }

            var comparison = GetComparitiveMonitorDpiScale(Screen.PrimaryScreen);
            if (!comparison.Equals(1))
                return WinAPI.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;

            // if the system dpi is not 96/96 then we are aware.
            if (!SystemDpi.Equals(Dpi.Default))
                return WinAPI.PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE;



            return WinAPI.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;
        }

        internal static double GetComparitiveMonitorDpiScale(Screen screen)
        {
            int cxLogical = (screen.Bounds.Right - screen.Bounds.Left);
            int cyLogical = (screen.Bounds.Bottom - screen.Bounds.Top);

            // Get the physical width and height of the monitor.
            WinAPI.DEVMODE dm = new WinAPI.DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(WinAPI.DEVMODE));

            dm.dmDriverExtra = 0;
            WinAPI.EnumDisplaySettings(screen.DeviceName, -1 /*ENUM_CURRENT_SETTINGS*/, ref dm);
            int cxPhysical = dm.dmPelsWidth;
            int cyPhysical = dm.dmPelsHeight;

            // Calculate the scaling factor.
            double horzScale = ((double)cxPhysical / (double)cxLogical);
            double vertScale = ((double)cyPhysical / (double)cyLogical);

            if (!horzScale.Equals(vertScale))
                throw new NotSupportedException("System returned Scale-X value that is not equal to Scale-Y, this is not supported.");

            return horzScale;
        }

        internal static bool Equals(this double one, double two, double tolerance = .00001)
        {
            return Math.Abs(one - two) <= tolerance;
        }

        #endregion
    }
}
