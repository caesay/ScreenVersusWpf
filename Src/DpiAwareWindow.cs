using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ScreenVersusWpf
{
    /// <summary>
    /// Inheirit this class to create a window that scales appropriately to system dpi changes, and monitors with different DPI's.
    /// Make sure your manifest entry is correctly set to `<dpiAware>True/PM</dpiAware>` to disable windows dpi virtualization
    /// </summary>
    public class DpiAwareWindow : Window
    {
        /// <summary>
        /// True if this window is actively monitoring the current monitor DPI and adjusting scale accordingly.
        /// Should be true; If false the current assembly manifest is probably incorrect.
        /// </summary>
        public bool IsPerMonitorDpiAware => _perMonitorEnabled;

        /// <summary>
        /// The current global system dpi. This wont change during the life of the application
        /// </summary>
        public Dpi SystemDpi => DpiUtil.SystemDpi;

        /// <summary>
        /// The current window/WPF dpi. This wont change during the life of the application and should be equal to SystemDpi
        /// </summary>
        public Dpi WindowDpi => DpiUtil.GetDpiFromVisual(this);

        /// <summary>
        /// The Dpi of the current monitor. This is used with SystemDpi to calculate the current scaling factor.
        /// </summary>
        public Dpi MonitorDpi => DpiUtil.GetDpiFromWindowMonitor(this);

        private HwndSource _handle;
        private Dpi _currentDpi;
        private double _currentScale;
        private readonly bool _perMonitorEnabled;
        private readonly bool _sysEnabled;

        /// <summary>
        /// Creates a new instance of DpiAwareWindow
        /// </summary>
        public DpiAwareWindow()
        {
            Loaded += OnLoaded;
            switch (DpiUtil.GetCurrentAwareness())
            {
                case WinAPI.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE:
                    break;
                case WinAPI.PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE:
                    // here we want to set the permon to false so it doesnt change, 
                    // but still scale the window according to system DPI.
                    _sysEnabled = true;
                    break;
                case WinAPI.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE:
                    _perMonitorEnabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_perMonitorEnabled)
            {
                _handle = (HwndSource)System.Windows.PresentationSource.FromVisual(this);
                HwndSourceHook hook = new HwndSourceHook(WndProc);
                _handle.AddHook(hook);
            }

            if (_sysEnabled || _perMonitorEnabled)
            {
                _currentDpi = DpiUtil.GetDpiFromVisual(this);
                _currentScale = _currentDpi.DpiX / (double)DpiUtil.SystemDpi.DpiX;

                Width = Width * _currentScale;
                Height = Height * _currentScale;

                UpdateLayoutTransform(_currentScale);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case (int)WinAPI.WindowMessage.WM_DPICHANGED:

                    //var lpNewRect = Marshal.PtrToStructure<WinAPI.RECT>(lParam);
                    Dpi oldDpi = _currentDpi;
                    _currentDpi = new Dpi(WinAPI.GetLoWord((uint)wParam), WinAPI.GetHiWord((uint)wParam));

                    if (oldDpi != _currentDpi)
                    {
                        this.Left = this.Left / oldDpi.ScaleX * _currentDpi.ScaleX;
                        this.Top = this.Top / oldDpi.ScaleY * _currentDpi.ScaleY;
                        this.Width = this.Width / oldDpi.ScaleX * _currentDpi.ScaleX;
                        this.Height = this.Height / oldDpi.ScaleY * _currentDpi.ScaleY;

                        OnDpiChanged();
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnDpiChanged()
        {
            _currentScale = _currentDpi.DpiX / (double)DpiUtil.SystemDpi.DpiX;
            UpdateLayoutTransform(_currentScale);
        }

        private void UpdateLayoutTransform(double scaleFactor)
        {
            // Adjust the rendering graphics and text size by applying the scale transform to the top level visual node of the Window		
            if (_perMonitorEnabled)
            {
                var child = GetVisualChild(0);
                if (_currentScale != 1.0)
                {
                    ScaleTransform dpiScale = new ScaleTransform(scaleFactor, scaleFactor);
                    child.SetValue(Window.LayoutTransformProperty, dpiScale);
                }
                else
                {
                    child.SetValue(Window.LayoutTransformProperty, null);
                }
            }
        }
    }
}
