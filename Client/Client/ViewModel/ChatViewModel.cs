using System;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Collections.ObjectModel;
using Client.Crypt;
using Client.Helpers;
using Client.Interfaces;
using Client.Helpers.ViewModel;
using System.Linq;

namespace Client.ViewModel
{
    class ChatViewModel : ViewModelBase
    {         
        #region Блок переменных, свойств, интерфейсов

        private const string _host = "127.0.0.1";
        private const int _port = 2222;

        private Socket _serverSocket;
        private Thread listenThread;

        private IShowInfo showInfo;
        private IAsymmCrypt AsymmCrypt;
        private ISymmCrypt SymmCrypt;

        private RSAParameters SupportingAsimmKey;
        private byte[] session_key;
        private Dictionary<string, byte[]> Simmetric_Keys;
        private Dictionary<string, (RSAParameters, RSAParameters)> Personal_Asimmetric_Keys;
        private Dictionary<string, RSAParameters> Asimmetric_Keys;

        private Dictionary<string, string> DataChat;

        public string Sum1
        {
            get { return sum1; }
            set 
            { 
                sum1 = value;
                OnPropertyChanged();
            }
        }
        private string sum1;

        public string Sum2
        {
            get { return sum2; }
            set
            {
                sum2 = value;
                OnPropertyChanged();
            }
        }
        private string sum2;

        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                OnPropertyChanged();
            }
        }
        private string userName;       

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                OnPropertyChanged();
            }
        }
        private string message;

        public string Chat
        {
            get { return chat; }
            set
            {
                chat = value;
                OnPropertyChanged();
            }
        }
        private string chat;

        public bool Silencemode { get; set; }
        public string SelectedName { get; set; }
        public ObservableCollection<string> Friends { get; set; }        

        DisplayRootRegistry displayRootRegistry;
        
        #endregion

        public ChatViewModel()
        {
            showInfo = new ShowInfo();
            AsymmCrypt = new RSACrypt();
            SymmCrypt = new AESCrypt();

            SupportingAsimmKey = new RSAParameters { Exponent = new byte[] { 1, 0, 1 } }; 

            Friends = new ObservableCollection<string>();
            displayRootRegistry = (Application.Current as App).displayRootRegistry;
            UserName = "Ник";
            SelectedName = "";
        }

        #region Подключение, прослушивание и обработчик команд

        public void Connect() // Подключение к серверу
        {
            try
            {
                IPAddress temp = IPAddress.Parse(_host);
                _serverSocket = new Socket(temp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Connect(new IPEndPoint(temp, _port));

                Thread.Sleep(400);

                if (_serverSocket.Connected)
                {
                    byte[] buffer1 = new byte[256];
                    byte[] buffer2 = new byte[256];
                    _serverSocket.Receive(buffer1);
                    _serverSocket.Receive(buffer2);

                    if (EDS.Check_byte(buffer1, buffer2))
                    {
                        SupportingAsimmKey.Modulus = buffer1;
                        listenThread = new Thread(listner);
                        listenThread.IsBackground = true;
                        listenThread.Start();
                    }
                    else
                    {
                        showInfo.ShowMessage("Не удалось установить доверенное соединение! Завершение сеанса", 3);
                        Application.Current.Shutdown();
                    }
                        
                }
                else
                    showInfo.ShowMessage("Связь с сервером не установлена.", 3);
            }
            catch
            {
                showInfo.ShowMessage("Связь с сервером не установлена.", 3);
            }
        }

        public void listner() // Прослушивание
        {
            try
            {
                while (_serverSocket.Connected)
                {
                    byte[] buffer = new byte[32768];
                    int bytesReceive = _serverSocket.Receive(buffer);

                    byte[] mess = new byte[bytesReceive];
                    Array.Copy(buffer, mess, bytesReceive);
                    
                    string command = SymmCrypt.Decrypt(mess, session_key).Result;
                    string Sign = command.Split("|").Last();
                    command = command.Substring(0, command.LastIndexOf("|"));

                    if (EDS.Check_str(command, Sign))         
                        handleCommand(command); 
                }
            }
            catch
            {
                showInfo.ShowMessage("Связь с сервером прервана");
                Application.Current.Shutdown();
            }
        }

        public void handleCommand(string cmd) // Обработчки команд
        {
            string[] commands = cmd.Split('#');
            int countCommands = commands.Length;

            for (int i = 0; i < countCommands; i++)
            {
                try
                {
                    string currentCommand = commands[i];

                    if (string.IsNullOrWhiteSpace(currentCommand))
                        continue;

                    #region Блок подключения и валидации данных

                    if (currentCommand.Contains("connect")) // При удачной идентификации
                    {
                        Personal_Asimmetric_Keys = new Dictionary<string, (RSAParameters, RSAParameters)>();
                        Asimmetric_Keys = new Dictionary<string, RSAParameters>();
                        Simmetric_Keys = new Dictionary<string, byte[]>();
                        DataChat = new Dictionary<string, string>();

                        Chat = "Подключение выполнено!";
                        UserName = currentCommand.Split('|')[1];
                        Sum2 = currentCommand.Split('|')[2];

                        if (Sum1 != Sum2)
                        {
                            showInfo.ShowMessage("Ключи не согласованны! Завершение сеанса",3);
                            Application.Current.Dispatcher.Invoke(delegate { CloseCommand.Execute(null); });
                        }

                        continue;
                    }

                    if (currentCommand.Contains("regfault")) // Неудачная регистрация
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            displayRootRegistry.HidePresentation(this);
                            ((RegViewModel)displayRootRegistry.GetParent(this)).Status = currentCommand.Split('|')[1];
                            displayRootRegistry.ShowParent(this);
                        }));

                        continue;
                    }

                    if (currentCommand.Contains("logfault")) // Неудачная аутентификация
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            displayRootRegistry.HidePresentation(this);
                            ((AuthViewModel)displayRootRegistry.GetParent(this)).Status = currentCommand.Split('|')[1];
                            displayRootRegistry.ShowParent(this);
                        }));

                        continue;
                    }

                    if (currentCommand.Contains("offline")) // Если адресат не в сети
                    {
                        Chat = "";
                        Chat += "Пользователь не в сети\r\n";

                        continue;
                    }

                    #endregion

                    #region Блок добавления и удаления контактов 

                    if (currentCommand.Contains("userlist")) // Обновление списка друзей
                    {
                        string[] Users = currentCommand.Split('|')[1].Split(',');

                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            Friends.Clear();

                            foreach (var item in Users)
                            {
                                Friends.Add(item);

                                if (!DataChat.ContainsKey(item))
                                    DataChat.Add(item, "");
                            }
                        }));

                        continue;
                    }

                    if (currentCommand.Contains("friendrequest")) // Оповещение об отправке запроса о дружбе
                    {
                        string targetUser = currentCommand.Split('|')[1];
                        showInfo.ShowMessage($"Пользователю {targetUser} отправлен дружеский запрос ");
                        continue;
                    }

                    if (currentCommand.Contains("addfriend")) // Запрос о добавлении в контакты
                    {
                        string guest_name = currentCommand.Split('|')[1];
                        string guest_id = currentCommand.Split('|')[2];

                        MessageBoxResult Result = (MessageBoxResult)showInfo.ShowMessage($"Вы хотите начать диалог с {guest_name} и добавить его в контакты?", 4);

                        if (Result == MessageBoxResult.Yes)
                            Send($"#acceptfriend|{guest_name}|{guest_id}");
                        else
                            Send($"#renouncement|{guest_name}");

                        continue;
                    }

                    if (currentCommand.Contains("failusersuccess")) // Отрицательный ответ для того, кто отправил запрос дружбы
                    {
                        string targetUser = currentCommand.Split('|')[1];
                        showInfo.ShowMessage($"Пользователь {targetUser} не может быть добавлен", 2);
                        continue;
                    }

                    if (currentCommand.Contains("addtolist")) // Добавление пользователя в листбокс
                    {
                        string new_friend = currentCommand.Split('|')[1];
                        Application.Current.Dispatcher.Invoke(new Action(delegate { Friends.Add(new_friend); })); // добавляем пользователя
                        DataChat.Add(new_friend, "");
                        continue;
                    }

                    if (currentCommand.Contains("remtolist")) // Удаление пользователя из листбокс
                    {
                        string guest = currentCommand.Split('|')[1];
                        Application.Current.Dispatcher.Invoke(new Action(delegate { Friends.Remove(guest); })); // удаление пользователя
                        if (DataChat.ContainsKey(guest))
                            DataChat.Remove(guest);

                        continue;
                    }

                    #endregion

                    #region Блок принятия сообщений и ключей

                    if (currentCommand.Contains("unknownuser")) // Если не найден адресат
                    {
                        string user = currentCommand.Split('|')[1];
                        showInfo.ShowMessage("Сообщение не дошло до " + user, 2);
                        continue;
                    }

                    if (currentCommand.Contains("msg")) // Принять сообщение
                    {
                        string from = currentCommand.Split('|')[1];
                        int mode = int.Parse(currentCommand.Split('|')[2]);
                        string message = currentCommand.Split('|')[3];

                        if (Simmetric_Keys.ContainsKey(from) && mode == 1)
                        {
                            byte[] enc_mass = Convert.FromBase64String(message);
                            string dec_mess = SymmCrypt.Decrypt(enc_mass, Simmetric_Keys[from]).Result;

                            DataChat[from] += Environment.NewLine + from + "**: " + dec_mess;
                            AddMessage(dec_mess, from, true);
                        }
                        else if (Personal_Asimmetric_Keys.ContainsKey(from) && mode == 2)
                        {
                            byte[] enc_mass = Convert.FromBase64String(message);
                            string dec_mess = AsymmCrypt.Decrypt_Str(enc_mass, Personal_Asimmetric_Keys[from].Item2);

                            DataChat[from] += Environment.NewLine + from + "**: " + dec_mess;
                            AddMessage(dec_mess, from, true);
                        }
                        else
                        {
                            if (!message.EndsWith("\r\n"))
                                message += "\r\n";

                            DataChat[from] += Environment.NewLine + from + ": " + message;
                            AddMessage(message, from);
                        }

                        continue;
                    }

                    if (currentCommand.Contains("giveAsymm")) // Получение публичного асимметричного ключа
                    {
                        string from = currentCommand.Split('|')[1];
                        string key = currentCommand.Split('|')[2];

                        SetOtherAsymmKey(from, ref key);
                        showInfo.ShowMessage("Получен асимметричный ключ от " + from);
                        continue;
                    }

                    #endregion

                }
                catch (Exception exp)
                {
                    showInfo.ShowMessage("Ошибка обработчика команд: " + exp.Message, 3);
                }

            }

        }

        #endregion

        #region Блок обработки основных сценариев

        public void Handshake(bool login, string Nick, string Password, string Email = "") // "Обмен рукопожатиями"
        {
            Aes aes = Aes.Create();
            session_key = aes.Key;
            Send(AsymmCrypt.Encrypt(session_key, SupportingAsimmKey));
            Sum1 = SumByte(session_key).ToString();

            if (login)
                Login(Nick, Password);
            else
                Registration(Nick, Email, Password);
        }

        public void Registration(string Nick, string Email, string Password) // Регистрация
        {
            if (!string.IsNullOrWhiteSpace(Nick) && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password))
                Send($"#register|{Nick}|{Email}|{Password}");
        }

        public void Login(string Nick, string Password) // Логин
        {
            if (!string.IsNullOrWhiteSpace(Nick) && !string.IsNullOrWhiteSpace(Password))
                Send($"#login|{Nick}|{Password}");
        }

        public void AddFriend(string name) // Добавление контакта
        {
            if (!string.IsNullOrWhiteSpace(name))
                Send($"#findbynick|{name}");
        }

        public void SetSimmKey(string name, byte[] key) // Установить симметричный ключ
        {
            if (Simmetric_Keys.ContainsKey(name))
                Simmetric_Keys[name] = key;
            else
                Simmetric_Keys.Add(name, key);
        }

        public void SetMyAsymmKey(string name, ref (RSAParameters, RSAParameters) Mykey) // Установить свой асимметричный ключ
        {
            Personal_Asimmetric_Keys[name] = Mykey;
        }

        public void SetOtherAsymmKey(string name, ref string OtherKey) // Установить чужой асимметричный ключ
        {
            Asimmetric_Keys[name] = new RSAParameters { Exponent = new byte[] { 1, 0, 1 }, Modulus = Convert.FromBase64String(OtherKey) };
        }

        public void SendAsymm(string name, string key) // Отправка публичного ключа
        {
            Send($"#sendAsymm|{name}|{key}");
        }

        public void DelKey(string name, bool type) // Удаление ключа
        {
            if (type)
                Simmetric_Keys.Remove(name);
            else
            {
                Personal_Asimmetric_Keys.Remove(name);
                Asimmetric_Keys.Remove(name);
            }
        }

        private void Send(string buffer) // Отправить сообщение на сервер в формате строки
        {
            try
            {
                byte[] command = SymmCrypt.Encrypt(buffer, session_key).Result;
                _serverSocket.Send(command);
            }
            catch (Exception exp)
            {
                showInfo.ShowMessage("Ошибка отправки строки: " + exp.Message, 3);
            }
        }

        private void Send(byte[] buffer) // Отправить сообщение на сервер в формате массив байт
        {
            try
            {
                _serverSocket.Send(buffer);
            }
            catch (Exception exp)
            {
                showInfo.ShowMessage("Ошибка отправки байт: " + exp.Message, 3);
            }
        }      

        private void SendMess(string msgData, string to) // Обработка перед отправкой сообщения
        {
            if (!string.IsNullOrWhiteSpace(msgData) && !string.IsNullOrWhiteSpace(to))
            {
                if (!msgData.EndsWith("\r\n"))
                    msgData += "\r\n";

                if (Simmetric_Keys.ContainsKey(to) && Asimmetric_Keys.ContainsKey(to))
                    showInfo.ShowMessage("Выберите только один метод дополнительного шифрования", 2);
                else if (Simmetric_Keys.ContainsKey(to))
                {
                    DataChat[to] += Environment.NewLine + UserName + "**: " + msgData;

                    string crp_mess = Convert.ToBase64String(SymmCrypt.Encrypt(msgData, Simmetric_Keys[to]).Result);
                    Send($"#message|{to}|1|{crp_mess}");
                    AddMessage(msgData, UserName, true);
                }
                else if (Asimmetric_Keys.ContainsKey(to))
                {
                    DataChat[to] += Environment.NewLine + UserName + "**: " + msgData;

                    string crp_mess = Convert.ToBase64String(AsymmCrypt.Encrypt_Str(msgData, Asimmetric_Keys[to]));
                    Send($"#message|{to}|2|{crp_mess}");
                    AddMessage(msgData, UserName, true);
                }
                else
                {
                    DataChat[to] += Environment.NewLine + UserName + ": " + msgData;

                    Send($"#message|{to}|0|{msgData}");
                    AddMessage(msgData, UserName);
                }
            }

            Message = "";
        }

        private void AddMessage(string message, string from, bool deepsecure = false) // Отображение нового сообщения в чате 
        {
            if (from == UserName || from == SelectedName)
            {
                if (deepsecure)
                    Chat += $"{from}**: {message}" + Environment.NewLine;
                else
                    Chat += $"{from}: {message}" + Environment.NewLine;
            }
        }

        private void LoadMessage(string Content) // Отображение чата
        {
            Chat = "";
            Chat += Content + Environment.NewLine;
        }

        #endregion

        #region Блок вспомогательных сценариев

        private int SumByte(byte[] mas)
        {
            int sum = 0;

            foreach (var item in mas)
            {
                sum += item;
            }

            return sum;
        }

        #endregion

        #region Блок команд

        // команда перехода к поиску пользователя
        private RelayCommand findCommand;
        public RelayCommand FindCommand
        {
            get
            {
                return findCommand ??
                  (findCommand = new RelayCommand(async obj =>
                  {
                      AddingViewModel AddingViewModel = new AddingViewModel();
                      displayRootRegistry.SetParent(AddingViewModel, this);
                      AddingViewModel.MyName = UserName;
                      await displayRootRegistry.ShowModalPresentation(AddingViewModel);       
                  }));
            }
        }
        
        // команда выбора адресата
        private RelayCommand destinationCommand;
        public RelayCommand DestinationCommand
        {
            get
            {
                return destinationCommand ??
                  (destinationCommand = new RelayCommand(obj =>
                  {
                      if (!string.IsNullOrWhiteSpace(SelectedName))
                      {
                          string chat;

                          if (DataChat.TryGetValue(SelectedName, out chat))
                              LoadMessage(chat);

                          Send($"#isonline|{SelectedName}");
                      }

                  }));
            }
        }

        // команда перехода к установке сквозного шифрования
        private RelayCommand setkeyCommand;
        public RelayCommand SetKeyCommand
        {
            get
            {
                return setkeyCommand ??
                  (setkeyCommand = new RelayCommand(async bj =>
                  {
                      if (!string.IsNullOrWhiteSpace(SelectedName))
                      {
                          KeySetViewModel KeySetViewModel = new KeySetViewModel();
                          displayRootRegistry.SetParent(KeySetViewModel, this);

                          KeySetViewModel.UserName = SelectedName;

                          byte[] current_key;
                          if (Simmetric_Keys.TryGetValue(SelectedName, out current_key))
                              KeySetViewModel.Current_Simmetric_Key_Str = Helpers.KeySet.ByteConvStr.ByteToStr(current_key);

                          (RSAParameters, RSAParameters) asim_current_key;
                          if (Personal_Asimmetric_Keys.TryGetValue(SelectedName, out asim_current_key))
                          {
                              KeySetViewModel.Current_Asimmetric_Key_Str = Convert.ToBase64String(asim_current_key.Item1.Modulus);
                              KeySetViewModel.Current_Asimmetric_Key = asim_current_key;
                          }

                          RSAParameters friendAsymm;
                          if (Asimmetric_Keys.TryGetValue(SelectedName, out friendAsymm))
                              KeySetViewModel.UserNameAsymm_Str = Convert.ToBase64String(friendAsymm.Modulus);

                          await displayRootRegistry.ShowModalPresentation(KeySetViewModel);
                      }

                  }));
            }
        }

        // команда удаления контакта
        private RelayCommand delCommand;
        public RelayCommand DelCommand
        {
            get
            {
                return delCommand ??
                  (delCommand = new RelayCommand(obj =>
                  {
                      if (!string.IsNullOrWhiteSpace(SelectedName))
                      {
                          MessageBoxResult Result = (MessageBoxResult)showInfo.ShowMessage($"Удалить {SelectedName}?", 4);
                          if (Result == MessageBoxResult.Yes)
                          {
                              Send($"#delete|{SelectedName}");
                              DataChat.Remove(SelectedName);
                              Friends.Remove(SelectedName);
                          }
                      }
                  }));
            }
        }
        
        // команда отправки сообщения
        private RelayCommand silenceCommand;
        public RelayCommand SilenceCommand
        {
            get
            {
                return silenceCommand ??
                  (silenceCommand = new RelayCommand(obj =>
                  {
                      if (Silencemode)
                          Send("#silenceon");
                      else
                          Send("#silenceoff");
                  }));
            }
        }

        // команда отправки сообщения
        private RelayCommand sendCommand;
        public RelayCommand SendCommand
        {
            get
            {
                return sendCommand ??
                  (sendCommand = new RelayCommand(obj =>
                  {
                      if (!string.IsNullOrWhiteSpace(SelectedName))               
                          SendMess(Message, SelectedName);            
                      else
                          showInfo.ShowMessage("Адресат не выбран", 2);
                  }));
            }
        }

        // команда закрытия чата
        private RelayCommand closeCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                return closeCommand ??
                  (closeCommand = new RelayCommand(obj =>
                  {
                      if (_serverSocket.Connected)
                      {
                          Send($"#endsession");
                      }

                      Application.Current.Shutdown();

                  }));
            }
        }

        #endregion
    }
}