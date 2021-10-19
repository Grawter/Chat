using System.Windows;
using Client.Helpers;
using Client.Helpers.ViewModel;

namespace Client.ViewModel
{
    class RegViewModel : ViewModelBase
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

        private string email;
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
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

        private string confpassword;
        public string ConfPassword
        {
            get { return confpassword; }
            set
            {
                confpassword = value;
                OnPropertyChanged();
            }
        }

        DisplayRootRegistry displayRootRegistry = (Application.Current as App).displayRootRegistry;
        ChatViewModel ChatViewModel;
        bool attempt = false;

        #endregion

        #region Блок команд

        // команда перехода к аутентификации
        private RelayCommand toAuthCommand;
        public RelayCommand ToAuthCommand
        {
            get
            {
                return toAuthCommand ??
                  (toAuthCommand = new RelayCommand(obj =>
                  {                 
                      AuthViewModel AuthViewModel = new AuthViewModel();

                      displayRootRegistry.ShowPresentation(AuthViewModel);
                      
                      password = "";
                      confpassword = "";
                      displayRootRegistry.ClosePresentation(this);
                  }));
            }
        }

        // команда регистрации
        private RelayCommand registerCommand;
        public RelayCommand RegisterCommand
        {
            get
            {
                return registerCommand ??
                  (registerCommand = new RelayCommand(obj =>
                  {
                      string Error = DataValidation.IsValid(Nick, Email, Password, ConfPassword);
                      if (!string.IsNullOrEmpty(Error))
                      {
                          Status = Error;
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
                          ChatViewModel.Handshake(false, Nick, Password, Email);
                      }                          
                      else
                          ChatViewModel.Registration(Nick, Email, Password);

                      attempt = true;

                      password = "";
                      confpassword = "";
                      displayRootRegistry.ClosePresentation(this);
                  }));
            }
        }

        #endregion
    }
}