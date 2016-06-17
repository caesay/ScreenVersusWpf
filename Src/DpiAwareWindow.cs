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
    public class DpiAwareWindow : Window
    {
        private HwndSource _handle;
        private Dpi _currentDpi;
        private double _currentScale;
        private readonly bool _perMonitorEnabled;
        private readonly bool _sysEnabled;

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
