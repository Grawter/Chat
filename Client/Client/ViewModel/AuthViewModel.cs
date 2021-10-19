using System.Windows;
using Client.Helpers.ViewModel;

namespace Client.ViewModel
{
    class AuthViewModel : ViewModelBase
    {
        #region Блок переменных и свойств

        private string status;
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        private string nick;
        public string Nick
        {
            get { return nick; }
            set
            {
                nick = value;
                OnPropertyChanged();
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }

        DisplayRootRegistry displayRootRegistry = (Application.Current as App).displayRootRegistry;
        ChatViewModel ChatViewModel;
        bool attempt = false;

        #endregion

        #region Блок команд

        // команда перехода к регистрации
        private RelayCommand toRegisterCommand;
        public RelayCommand ToRegisterCommand
        {
            get
            {
                return toRegisterCommand ??
                  (toRegisterCommand = new RelayCommand(obj =>
                  {              
                      RegViewModel RegViewModel = new RegViewModel();

                      displayRootRegistry.ShowPresentation(RegViewModel);
                      
                      Password = "";
                      displayRootRegistry.ClosePresentation(this);
                  }));
            }
        }

        // команда аутентификации
        private RelayCommand loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return loginCommand ??
                  (loginCommand = new RelayCommand(obj =>
                  {
                      if (string.IsNullOrWhiteSpace(Nick) || string.IsNullOrWhiteSpace(Password))
                      {
                          Status = "Заполните пустые поля!";
                          return;
                      }

                      if (ChatViewModel == null)
                      {
                          ChatViewModel = new ChatViewModel();
                      }
                      
                      displayRootRegistry.ShowPresentation(ChatViewModel);

                      if (!attempt)
                      {
                          displayRootRegistry.SetParent(ChatViewModel, this);
                          ChatViewModel.Connect();
                          ChatViewModel.Handshake(true, Nick, Password);
                      }
                      else
                          ChatViewModel.Login(Nick, Password);

                      attempt = true;

                      Password = "";
                      displayRootRegistry.ClosePresentation(this);
                  }));
            }
        }

        #endregion
    }
}