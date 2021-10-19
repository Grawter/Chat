using System;
using System.Windows;
using System.Security.Cryptography;
using Client.Helpers;
using Client.Helpers.KeySet;
using Client.Helpers.ViewModel;
using Client.Interfaces;

namespace Client.ViewModel
{
    class KeySetViewModel : ViewModelBase
    {
        #region Блок переменных и свойств

        IShowInfo showInfo = new ShowInfo();
        DisplayRootRegistry displayRootRegistry = (Application.Current as App).displayRootRegistry;

        public int AsimmKeySize { get; set; }

        public string UserName
        {
            get { return username; }
            set { username = value; }
        }
        private string username;

        public string Current_Simmetric_Key_Str
        {
            get { return current_Simmetric_Key_Str; }
            set
            {
                current_Simmetric_Key_Str = value;
                OnPropertyChanged();
            }
        }
        private string current_Simmetric_Key_Str;

        public string Current_Asimmetric_Key_Str
        {
            get { return current_Asimmetric_Key_Str; }
            set
            {
                current_Asimmetric_Key_Str = value;
                OnPropertyChanged();
            }
        }
        private string current_Asimmetric_Key_Str;

        public (RSAParameters, RSAParameters) Current_Asimmetric_Key
        {
            get { return current_Asimmetric_Key; }
            set
            {
                current_Asimmetric_Key = value;
            }
        }
        private (RSAParameters, RSAParameters) current_Asimmetric_Key;

        public string UserNameRSA_Str
        {
            get { return usernameRSA_Str; }
            set
            {
                usernameRSA_Str = value;
                OnPropertyChanged();
            }
        }
        private string usernameRSA_Str;
        
        #endregion

        #region Блок команд для симметричного ключа

        // команда генерации симметричного ключа
        private RelayCommand generatesymmetricCommand;
        public RelayCommand GenerateSymmetricCommand
        {
            get
            {
                return generatesymmetricCommand ??
                  (generatesymmetricCommand = new RelayCommand(obj =>
                  {
                      Aes aes = Aes.Create();
                      Current_Simmetric_Key_Str = ByteConvStr.ByteToStr(aes.Key);
                  }));
            }
        }

        // команда установки симметричного ключа
        private RelayCommand setsymmetricCommand;
        public RelayCommand SetSymmetricCommand
        {
            get
            {
                return setsymmetricCommand ??
                  (setsymmetricCommand = new RelayCommand(obj =>
                  {
                      try
                      {
                          byte[] key_mass = ByteConvStr.StrToByte(Current_Simmetric_Key_Str);
                          ((ChatViewModel)displayRootRegistry.GetParent(this)).SetAES(UserName, key_mass);
                          showInfo.ShowMessage("Ключ установлен");
                      }
                      catch (OverflowException)
                      {
                          showInfo.ShowMessage("Указывайте цифры от 0 до 255");
                      }
                      catch (FormatException)
                      {
                          showInfo.ShowMessage("Ключ не указан, либо указан не полностью", 2);
                      }
                  }));
            }
        }

        // команда удаления симметричного ключа
        private RelayCommand delsymmetricCommand;
        public RelayCommand DelSymmetricCommand
        {
            get
            {
                return delsymmetricCommand ??
                  (delsymmetricCommand = new RelayCommand(obj =>
                  {
                      ((ChatViewModel)displayRootRegistry.GetParent(this)).DelKey(UserName, true);
                      showInfo.ShowMessage("Ключ удалён");
                      Current_Simmetric_Key_Str = "";
                  }));
            }
        }

        // команда копирование симметричного ключа
        private RelayCommand copysymmetricCommand;
        public RelayCommand CopySymmetricCommand
        {
            get
            {
                return copysymmetricCommand ??
                  (copysymmetricCommand = new RelayCommand(obj =>
                  {
                      Clipboard.SetText(Current_Simmetric_Key_Str);
                  }));
            }
        }

        #endregion

        #region Блок команд для асимметричного ключа

        //команда копирование асимметричного ключа
        private RelayCommand generateasymmetricCommand;
        public RelayCommand GenerateAsymmetricCommand
        {
            get
            {
                return generateasymmetricCommand ??
                  (generateasymmetricCommand = new RelayCommand(obj =>
                  {
                      RSA RSA = RSA.Create();

                      RSA.KeySize = AsimmKeySize switch
                      {
                          0 => 2048,
                          1 => 3072,
                          2 => 4096,
                          3 => 5120,
                          4 => 6016,
                          _ => 2048,
                      };

                      current_Asimmetric_Key.Item1 = RSA.ExportParameters(false);
                      current_Asimmetric_Key.Item2 = RSA.ExportParameters(true);

                      Current_Asimmetric_Key_Str = Convert.ToBase64String(Current_Asimmetric_Key.Item1.Modulus);
                  }));
            }
        }

        // команда установки асимметричного ключа
        private RelayCommand setasymmetricCommand;
        public RelayCommand SetAsymmetricCommand
        {
            get
            {
                return setasymmetricCommand ??
                  (setasymmetricCommand = new RelayCommand(obj =>
                  {
                      bool send = false;

                      if (Current_Asimmetric_Key.Item1.Modulus != null && Current_Asimmetric_Key.Item2.Modulus != null)
                      {
                          ((ChatViewModel)displayRootRegistry.GetParent(this)).SetMyRSA(UserName, ref current_Asimmetric_Key);
                          send = true;
                      }
                           
                      if(!string.IsNullOrWhiteSpace(UserNameRSA_Str))
                      {
                          ((ChatViewModel)displayRootRegistry.GetParent(this)).SetOtherRSA(UserName, ref usernameRSA_Str);
                          send = true;
                      }
                          
                      if (send)
                        showInfo.ShowMessage("Ключ установлен");

                  }));
            }
        }

        // команда удаления асимметричного ключа
        private RelayCommand delasymmetricCommand;
        public RelayCommand DelAsymmetricCommand
        {
            get
            {
                return delasymmetricCommand ??
                  (delasymmetricCommand = new RelayCommand(obj =>
                  {
                      ((ChatViewModel)displayRootRegistry.GetParent(this)).DelKey(UserName, false);
                      showInfo.ShowMessage("Ключ удалён");
                      Current_Asimmetric_Key_Str = "";
                      UserNameRSA_Str = "";
                  }));
            }
        }

        // команда отправки асимметричного ключа
        private RelayCommand sendasymmetricCommand;
        public RelayCommand SendAsymmetricCommand
        {
            get
            {
                return sendasymmetricCommand ??
                  (sendasymmetricCommand = new RelayCommand(obj =>
                  {
                      if (Current_Asimmetric_Key.Item1.Modulus != null && Current_Asimmetric_Key.Item2.Modulus != null)                     
                          ((ChatViewModel)displayRootRegistry.GetParent(this)).SendRSA(UserName, Current_Asimmetric_Key_Str);
                                         
                  }));
            }
        }

        //команда копирование асимметричного ключа
        private RelayCommand copyasymmetricCommand;
        public RelayCommand CopyAsymmetricCommand
        {
            get
            {
                return copyasymmetricCommand ??
                  (copyasymmetricCommand = new RelayCommand(obj =>
                  {
                      Clipboard.SetText(Current_Asimmetric_Key_Str);
                  }));
            }
        }

        #endregion

        #region  Команда закрытия

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