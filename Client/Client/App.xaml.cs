using System.Windows;
using Client.Helpers.ViewModel;
using Client.ViewModel;

namespace Client
{
    public partial class App : Application
    {
        public DisplayRootRegistry displayRootRegistry = new DisplayRootRegistry();
        AuthViewModel AuthViewModel;

        public App()
        {
            displayRootRegistry.RegisterWindowType<AuthViewModel, AuthWindow>();
            displayRootRegistry.RegisterWindowType<RegViewModel, RegWindow>();
            displayRootRegistry.RegisterWindowType<ChatViewModel, ChatWindow>();
            displayRootRegistry.RegisterWindowType<AddingViewModel, AddingWindow>();
            displayRootRegistry.RegisterWindowType<KeySetViewModel, KeySetWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AuthViewModel = new AuthViewModel();

            displayRootRegistry.ShowPresentation(AuthViewModel);

        }
    }
}