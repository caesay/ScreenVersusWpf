using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ScreenVersusWpf;
using WpfCrutches;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer = new DispatcherTimer();
        private ScreenPoint _prevMousePos;

        public MainWindow()
        {
            InitializeComponent();
            ScreenTools.ScreenSettingsChanged += delegate { Refresh(); };
            Refresh();
            _timer.Interval = TimeSpan.FromSeconds(1.0 / 60);
            _timer.Tick += timer_Tick;
            _timer.Start();
        }

        private void Refresh()
        {
            ScreenTools.InitializeDpi(ScreenTools.GetSystemDpi());
            var wpfRectToScreen = new WpfRect(0, 0, 200, 200).ToScreenRect();
            var screenRectToWpf = new ScreenRect(0, 0, 200, 200).ToWpfRect();
            ctVirtualScreenSize.Content = $"{ScreenTools.VirtualScreen.Bounds.Width}x{ScreenTools.VirtualScreen.Bounds.Height}";
            ctVirtualScreenDpi.Content = ScreenTools.GetSystemDpi().ToString();
            ctVirtualScreenZoom.Content = $"{ScreenTools.DpiZoom:0%}";
            ctWpfRectToScreen.Content = $"{wpfRectToScreen.Width}x{wpfRectToScreen.Height}";
            ctScreenRectToWpf.Content = $"{screenRectToWpf.Width}x{screenRectToWpf.Height}";
            ctBoxScreenSize.Width = screenRectToWpf.Width;
            ctBoxScreenSize.Height = screenRectToWpf.Height;
            ctPhysicalScreens.ItemsSource = ScreenTools.Screens.Select(s => new ScreenVM(s)).ToList();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var mousePosScreen = ScreenTools.GetMousePosition();
            if (mousePosScreen == _prevMousePos)
                return;
            _prevMousePos = mousePosScreen;
            var mousePosWpf = mousePosScreen.ToWpfPoint();
            ctMouseScreenPoint.Content = $"ScreenPoint({mousePosScreen.X}, {mousePosScreen.Y})";
            ctMouseWpfPoint.Content = $"WpfPoint({mousePosWpf.X}, {mousePosWpf.Y})";
            var scrContaining = ScreenTools.GetScreenContaining(mousePosScreen);
            foreach (ScreenVM vm in ctPhysicalScreens.ItemsSource)
                vm.ContainsMouseVisibility.Value = vm.Scr == scrContaining ? Visibility.Visible : Visibility.Collapsed;
        }

        private class ScreenVM
        {
            public ScreenInfo Scr { get; private set; }
            public ScreenVM(ScreenInfo scr) { Scr = scr; }
            public override string ToString() => Scr.ToString();
            public ObservableValue<Visibility> ContainsMouseVisibility { get; set; } = new ObservableValue<Visibility>();
        }
    }
}
