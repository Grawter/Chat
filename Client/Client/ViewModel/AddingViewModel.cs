using System.Windows;
using Client.Interfaces;
using Client.Helpers;
using Client.Helpers.ViewModel;

namespace Client.ViewModel
{
    class AddingViewModel : ViewModelBase
    {
        #region Блок переменных и свойств

        IShowInfo showInfo = new ShowInfo();
        DisplayRootRegistry displayRootRegistry = (Application.Current as App).displayRootRegistry;
        public string Friend { get; set; }
        public string MyName { get; set; }

        #endregion

        #region Блок команд

        // команда добавления контакта
        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand(obj =>
                  {
                      if (!string.IsNullOrWhiteSpace(Friend) && !Friend.Equals(MyName))
                      
                          ((ChatViewModel)displayRootRegistry.GetParent(this)).AddFriend(Friend);                                          
                      else
                          showInfo.ShowMessage("Имя не указано", 2);
                  }));
            }
        }

        #endregion

        #region Команда закрытия

        // команда закрытия
        private RelayCommand closeCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                return closeCommand ??
                  (closeCommand = new RelayCommand(obj =>
                  {
                      displayRootRegistry.DeleteParent(this);
                  }));
            }
        }

        #endregion
    }
}