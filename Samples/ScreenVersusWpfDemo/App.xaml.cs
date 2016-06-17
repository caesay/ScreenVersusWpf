using System.Windows;
using WpfBindingErrors;

namespace Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if DEBUG
            BindingExceptionThrower.Attach();
#endif
        }
    }
}
