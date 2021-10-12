﻿using System;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Collections.Generic;
using Client.Crypt;
using Client.Interfaces;
using Client.Helpers;

namespace Client
{
    public partial class ChatForm : Form
    {
        IShowInfo showInfo = new ShowInfo();
        #region Блок переменных, свойств и делегатов

        private const string _host = "127.0.0.1";
        private const int _port = 2222;

        private delegate void ChatEvent(string message, string from, bool ds);
        private delegate void LoadChatEvent(string message);
        private ChatEvent _addMessage;
        private LoadChatEvent _loadMessage;

        private Socket _serverSocket;
        private Thread listenThread;

        private RSAParameters RSAKeyInfo = new RSAParameters { Exponent = new byte[] { 1, 0, 1 } };
        private byte[] session_key;
        private Dictionary<string, byte[]> simmetrickeys;
        private Dictionary<string, (RSAParameters, RSAParameters)> PersonalAsimmetricKeys;
        private Dictionary<string, RSAParameters> asimmetrickeys;

        private Dictionary<string, string> DataChat;

        public string Friend
        {
            get { return friend; }
            set { friend = value; }
        }
        private string friend;

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }
        private string userName;

        public string Email
        {
            get { return email; }
            set { email = value; }
        }
        private string email;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        private string password;

        #endregion

        public ChatForm()
        {
            InitializeComponent();

            #region Блок инициализации контекстного меню

            contextMenuStrip1 = new ContextMenuStrip(); // Инициализиуем контекстное меню

            ToolStripMenuItem SetKey = new ToolStripMenuItem("Установить ключ"); // Создание элемента меню

            SetKey.Click += delegate // Функционал элемента
            {
                if (listBox1.SelectedItem != null)
                {
                    if (!string.IsNullOrWhiteSpace(listBox1.SelectedItem.ToString()))
                    {           
                        string nick = listBox1.SelectedItem.ToString();

                        KeySetForm ks = new KeySetForm
                        {
                            Owner = this,
                            Username = nick
                        };

                        byte[] current_key;
                        if (simmetrickeys.TryGetValue(nick, out current_key))
                            ks.CurrentSimmetricKey = current_key;

                        (RSAParameters, RSAParameters) asim_current_key;
                        if (PersonalAsimmetricKeys.TryGetValue(nick, out asim_current_key))
                            ks.CurrentAsimmetricKey = asim_current_key;

                        RSAParameters friendRSA;
                        if (asimmetrickeys.TryGetValue(nick, out friendRSA))
                            ks.UsernameRSA = friendRSA;

                        ks.ShowDialog();               
                    }
                }    
            };

            ToolStripMenuItem DeleteUser = new ToolStripMenuItem("Удалить"); 

            DeleteUser.Click += delegate // Функционал элемента
            {
                if (listBox1.SelectedItem != null)
                {
                    if (!string.IsNullOrWhiteSpace(listBox1.SelectedItem.ToString()))
                    {
                        DialogResult Result = (DialogResult)showInfo.ShowMessage($"Удалить {listBox1.SelectedItem}?", 4);
                        if (Result == DialogResult.Yes)
                        {
                            Send($"#delete|{listBox1.SelectedItem}");
                            DataChat.Remove(listBox1.SelectedItem.ToString());
                            listBox1.Items.Remove(listBox1.SelectedItem);
                        }
                    }
                }                  
            };

            contextMenuStrip1.Items.Add(SetKey); // Добавить ранее созданного элемента в меню
            contextMenuStrip1.Items.Add(DeleteUser); // Добавить ранее созданного элемента в меню
           
            listBox1.ContextMenuStrip = contextMenuStrip1; // Закрепление созданного контекстного меню в листбоксе
            
            #endregion

            _addMessage = new ChatEvent(AddMessage); // Связывание делегата с функцией
            _loadMessage = new LoadChatEvent(LoadMessage);
        }

        private void ChatForm_Load(object sender, EventArgs e) // При загрузке чата
        {
            try
            {
                IPAddress temp = IPAddress.Parse(_host);
                _serverSocket = new Socket(temp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Connect(new IPEndPoint(temp, _port));

                Thread.Sleep(400);

                if (_serverSocket.Connected)
                {
                    byte[] buffer = new byte[256];
                    int bytesReceive = _serverSocket.Receive(buffer);
                    RSAKeyInfo.Modulus = buffer;

                    listenThread = new Thread(listner);
                    listenThread.IsBackground = true;
                    listenThread.Start();
                }
                else
                    showInfo.ShowMessage("Связь с сервером не установлена.", 3);
            }
            catch
            {
                showInfo.ShowMessage("Связь с сервером не установлена.", 3);
            }            
        }

        private void listner()
        {
            try
            {
                while (_serverSocket.Connected)
                {
                    byte[] buffer = new byte[32768];
                    int bytesReceive = _serverSocket.Receive(buffer);

                    byte[] mess = new byte[bytesReceive];
                    Array.Copy(buffer, mess, bytesReceive);

                    string command = AESCrypt.AESDecrypt(mess, session_key).Result;
                    handleCommand(command);
                }
            }
            catch
            {
                showInfo.ShowMessage("Связь с сервером прервана");
                Application.Exit();
            }
        }

        private void handleCommand(string cmd)
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
                        PersonalAsimmetricKeys = new Dictionary<string, (RSAParameters, RSAParameters)>();
                        asimmetrickeys = new Dictionary<string, RSAParameters>();
                        simmetrickeys = new Dictionary<string, byte[]>();
                        DataChat = new Dictionary<string, string>();

                        Invoke((MethodInvoker)delegate // Обеспечение доступа к элементам формы в потоке, в котором они были созданы
                        {
                            textBox3.Text = "Подключение выполнено!";
                            label1.Text = UserName;
                        });

                        continue;
                    }

                    if (currentCommand.Contains("usernamenotaccess")) // Ник уже занят
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            (Owner as RegForm).Status = "Ник уже занят";
                            Owner.Show();
                            this.Hide();
                        });

                        continue;
                    }

                    if (currentCommand.Contains("emailnotaccess")) // Email уже занят
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            (Owner as RegForm).Status = "Электронная почта уже занята";
                            Owner.Show();
                            this.Hide();
                        });

                        continue;
                    }

                    if (currentCommand.Contains("logfault")) // Неверные данные для входа
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            (Owner as AuthForm).Status = "Неверный логин или пароль";
                            Owner.Show();
                            this.Hide();
                        });

                        continue;
                    }

                    if (currentCommand.Contains("sessionbusy")) // Неверные данные для входа
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            (Owner as AuthForm).Status = "Сессия занята";
                            Owner.Show();
                            this.Hide();
                        });

                        continue;
                    }

                    if (currentCommand.Contains("offline")) // Если адресат не в сети
                    {
                        textBox3.Invoke((MethodInvoker)delegate { textBox3.AppendText("Пользователь не в сети\r\n"); });

                        continue;
                    }

                    #endregion

                    #region Блок добавления и удаления контактов 

                    if (currentCommand.Contains("userlist")) // Обновление списка друзей
                    {
                        string[] Users = currentCommand.Split('|')[1].Split(',');

                        listBox1.Invoke((MethodInvoker)delegate { listBox1.Items.Clear(); }); // Очищение списка

                        for (int j = 0; j < Users.Length; j++) // Добавляем пользователей
                        {
                            listBox1.Invoke((MethodInvoker)delegate { listBox1.Items.Add(Users[j]); });
                            
                            if(!DataChat.ContainsKey(Users[j]))
                                DataChat.Add(Users[j], "");
                        }

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

                        DialogResult Result = (DialogResult)showInfo.ShowMessage($"Вы хотите начать диалог с {guest_name} и добавить его в контакты?", 4);

                        if (Result == DialogResult.Yes)
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
                        listBox1.Invoke((MethodInvoker)delegate { listBox1.Items.Add(new_friend); }); // добавляем пользователя
                        DataChat.Add(new_friend, "");
                        continue;
                    }

                    if (currentCommand.Contains("remtolist")) // Удаление пользователя из листбокс
                    {
                        string guest = currentCommand.Split('|')[1];
                        listBox1.Invoke((MethodInvoker)delegate { listBox1.Items.Remove(guest); }); // удаление пользователя
                        
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
                        
                        if (simmetrickeys.ContainsKey(from) && mode == 1)
                        {
                            byte[] enc_mass = Convert.FromBase64String(message);
                            string dec_mess = AESCrypt.AESDecrypt(enc_mass, simmetrickeys[from]).Result;

                            DataChat[from] += Environment.NewLine + from + "**: " + dec_mess;
                            AddMessage(dec_mess, from, true);
                        }
                        else if(PersonalAsimmetricKeys.ContainsKey(from) && mode == 2)
                        {                                    
                            byte[] enc_mass = Convert.FromBase64String(message);
                            string dec_mess = RSACrypt.RSADecrypt_Str(enc_mass, PersonalAsimmetricKeys[from].Item2);

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

                    if (currentCommand.Contains("chat")) // Принять чат
                    {
                        string chat = currentCommand.Split('|')[1];
                        LoadMessage(chat);
                        continue;
                    }

                    if (currentCommand.Contains("giveRSA")) // Получение публичного RSA ключа
                    {
                        string from = currentCommand.Split('|')[1];
                        string key = currentCommand.Split('|')[2];

                        if (!asimmetrickeys.ContainsKey(from))
                        {
                            RSAParameters new_key = new RSAParameters { Exponent = new byte[] { 1, 0, 1 }, Modulus = Convert.FromBase64String(key) };
                            asimmetrickeys.Add(from, new_key);
                        }
                        else
                            asimmetrickeys[from] = new RSAParameters { Exponent = new byte[] { 1, 0, 1 }, Modulus = Convert.FromBase64String(key) };

                        showInfo.ShowMessage("Получен RSA ключ от " + from);

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

        #region Блок обработки основных сценариев

        public void Handshake(bool login) // "Обмен рукопожатиями"
        {
            Aes aes = Aes.Create();
            session_key = aes.Key;
            Send(RSACrypt.RSAEncrypt(session_key, RSAKeyInfo));

            if (login)
                Login();
            else
                Registration();
        }

        public void Registration() // Регистрация
        {
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            {
                Send($"#register|{userName}|{email}|{password}");
                password = "";
            }     
        }

        public void Login() // Логин
        {
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                Send($"#login|{userName}|{password}");
                password = "";
            }
        }

        public void SetAES(string name, byte [] key) // Установить AES ключ
        {
            if (simmetrickeys.ContainsKey(name))
                simmetrickeys[name] = key;
            else
                simmetrickeys.Add(name, key);
        }

        public void SetRSA(string name, (RSAParameters, RSAParameters) key) // Установить RSA ключ
        {
            if (PersonalAsimmetricKeys.ContainsKey(name))
                PersonalAsimmetricKeys[name] = key;
            else
                PersonalAsimmetricKeys.Add(name, key);
        }

        public void SendRSA(string name, string key) // Отправка публичного ключа
        {
            Send($"#sendRSA|{name}|{key}");
        }

        public void DelKey(string name, bool type) // Удаление ключа
        {
            if (type)
                simmetrickeys.Remove(name);
            else
            {
                PersonalAsimmetricKeys.Remove(name);
                asimmetrickeys.Remove(name);
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

        private void Send(string buffer) // Отправить сообщение на сервер в формате строки
        {
            try
            {
                byte[] command = AESCrypt.AESEncrypt(buffer, session_key).Result;
                _serverSocket.Send(command);
            }
            catch (Exception exp)
            {
                showInfo.ShowMessage("Ошибка отправки строки: " + exp.Message, 3);
            }
        }      

        private void SendMess(string msgData, string to)
        {
            if (!string.IsNullOrWhiteSpace(textBox2.Text) && !string.IsNullOrWhiteSpace(listBox1.SelectedItem.ToString()))
            {
                if (!msgData.EndsWith("\r\n"))
                    msgData += "\r\n";

                if (simmetrickeys.ContainsKey(to) && asimmetrickeys.ContainsKey(to))
                    showInfo.ShowMessage("Выберите только один метод дополнительного шифрования", 2);
                else if (simmetrickeys.ContainsKey(to))
                {
                    DataChat[to] += Environment.NewLine + UserName + "**: " + msgData;

                    string crp_mess = Convert.ToBase64String(AESCrypt.AESEncrypt(msgData, simmetrickeys[to]).Result);
                    Send($"#message|{to}|1|{crp_mess}");
                    AddMessage(msgData, UserName, true);
                }
                else if (asimmetrickeys.ContainsKey(to))
                {
                    DataChat[to] += Environment.NewLine + UserName + "**: " + msgData;

                    string crp_mess = Convert.ToBase64String(RSACrypt.RSAEncrypt_Str(msgData, asimmetrickeys[to]));
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

            textBox2.Text = string.Empty;
        }

        private void AddMessage(string message, string from, bool ds = false) // Отображение нового сообщения в чате 
        {
            if(InvokeRequired) // Защита от чужого потока
            {
                Invoke(_addMessage, message, from, ds);
                return;
            }
            else if (listBox1.SelectedItem != null)
            {
                if (from == label1.Text || from == listBox1.SelectedItem.ToString())
                {                    
                    if (ds)
                        textBox3.AppendText($"{from}**: {message}" + Environment.NewLine);
                    else
                        textBox3.AppendText($"{from}: {message}" + Environment.NewLine);
                }
            }
        }

        private void LoadMessage(string Content) // Отображение чата
        {
            if (InvokeRequired) // Защита от чужого потока
            {
                Invoke(_loadMessage, Content);
                return;
            }

            textBox3.Clear();
            textBox3.AppendText(Content + Environment.NewLine);
        }

        #endregion

        #region Блок событий формы 

        private void checkBox1_CheckedChanged(object sender, EventArgs e) // Переключения режима не беспокоить
        {
            if (checkBox1.Checked)
                Send("#silenceon");
            else
                Send("#silenceoff");
        }

        private void добавитьКонтактToolStripMenuItem_Click(object sender, EventArgs e) // Найти и добавить по нику
        {
            AddingForm add = new AddingForm();
            add.Owner = this;
            add.ShowDialog();
            
            if (!string.IsNullOrWhiteSpace(Friend))
            {
                Send($"#findbynick|{Friend}");
                Friend = string.Empty;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) // Выбор адресата
        {
            if (listBox1.SelectedItem != null)
            {
                if (!string.IsNullOrWhiteSpace(listBox1.SelectedItem.ToString()))
                {
                    textBox2.Enabled = true;
                    button1.Enabled = true;              

                    string chat, nick = listBox1.SelectedItem.ToString();

                    if (DataChat.TryGetValue(nick, out chat))
                        LoadMessage(chat);

                    Send($"#isonline|{nick}");
                }
            }                      
        }

        private void button1_Click(object sender, EventArgs e) // Отправка сообщения через кнопку
        {
            if (listBox1.SelectedItem != null)
            {
                string msgData = textBox2.Text, to = listBox1.SelectedItem.ToString();
                SendMess(msgData, to);
            }    
            else
                showInfo.ShowMessage("Адресат не выбран", 2);
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e) // Отправка сообщения через Enter       
        {
            if (e.KeyData == Keys.Enter)
            {
                if (listBox1.SelectedItem != null)
                {
                    string msgData = textBox2.Text, to = listBox1.SelectedItem.ToString();
                    SendMess(msgData, to);
                }
                else
                    showInfo.ShowMessage("Адресат не выбран", 2);
            }
        }

        private void ChatForm_FormClosing_1(object sender, FormClosingEventArgs e) // При закрытии формы
        {
            if (_serverSocket.Connected)           
                Send($"#endsession");
            

            Application.Exit();
        }

        #endregion       
    }
}