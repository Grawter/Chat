using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using Client.Crypt;

namespace Client
{
    public partial class ChatForm : Form
    {
        #region Блок переменных, свойств и делегатов

        private const string _host = "127.0.0.1";
        private const int _port = 2222;

        private delegate void ChatEvent(string from, string message);
        private delegate void LoadChatEvent(string message);
        private ChatEvent _addMessage;
        private LoadChatEvent _loadMessage;

        private Socket _serverSocket;
        private Thread listenThread;

        private RSAParameters RSAKeyInfo = new RSAParameters { Exponent = new byte[] { 1, 0, 1 } };
        private byte[] session_key;
        private Dictionary<string, byte[]> keys;

        private Dictionary<string, string> DataChat;

        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
            }
        }
        private string userName;

        public string Email
        {
            get { return email; }
            set
            {
                email = value;
            }
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

            ToolStripMenuItem DeleteUser = new ToolStripMenuItem("Удалить"); // Создание элемента меню

            DeleteUser.Click += delegate // Функционал элемента
            {
                if (listBox1.SelectedItems.Count > 0 && !string.IsNullOrWhiteSpace(listBox1.SelectedItem.ToString()))
                {
                    DialogResult Result = MessageBox.Show($"Удалить {listBox1.SelectedItem}?", "Удаление из контактов", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (Result == DialogResult.Yes)
                    {
                        Send($"#delete|{listBox1.SelectedItem}");
                        DataChat.Remove(listBox1.SelectedItem.ToString());
                        listBox1.Items.Remove(listBox1.SelectedItem);               
                    }
                }
            };

            contextMenuStrip1.Items.Add(DeleteUser); // Добавить ранее созданного элемента в меню

            ToolStripMenuItem BlockUser = new ToolStripMenuItem("Заблокировать");

            BlockUser.Click += delegate
            {

            };

            contextMenuStrip1.Items.Add(BlockUser);

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
                    MessageBox.Show("Связь с сервером не установлена.");
            }
            catch
            {
                MessageBox.Show("Связь с сервером не установлена.");
            }            
        }

        public void listner()
        {
            try
            {
                while (_serverSocket.Connected)
                {
                    byte[] buffer = new byte[65536];
                    int bytesReceive = _serverSocket.Receive(buffer);

                    byte[] mess = new byte[bytesReceive];
                    Array.Copy(buffer, mess, bytesReceive);

                    string command = AESCrypt.AESDecrypt(mess, session_key).Result;
                    handleCommand(command);
                }
            }
            catch
            {
                MessageBox.Show("Связь с сервером прервана");
                Application.Exit();
            }
        }

        public void handleCommand(string cmd)
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
                        Invoke((MethodInvoker)delegate // Обеспечение доступа к элементам формы в потоке, в котором они были созданы
                        {
                            keys = new Dictionary<string, byte[]>();
                            DataChat = new Dictionary<string, string>();
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
                        MessageBox.Show($"Пользователю {targetUser} отправлен дружеский запрос ");
                        continue;
                    }

                    if (currentCommand.Contains("addfriend")) // Запрос о добавлении в контакты
                    {
                        string guest_name = currentCommand.Split('|')[1];
                        string guest_id = currentCommand.Split('|')[2];

                        DialogResult Result = MessageBox.Show($"Вы хотите начать диалог с {guest_name} и добавить его в контакты?", "Начало диалога", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (Result == DialogResult.Yes)
                        {
                            Send($"#acceptfriend|{guest_name}|{guest_id}");
                        }
                        else
                        {
                            Send($"#renouncement|{guest_name}");
                        }

                        continue;
                    }

                    if (currentCommand.Contains("failusersuccess")) // Отрицательный ответ для того, кто отправил запрос дружбы
                    {
                        string targetUser = currentCommand.Split('|')[1];
                        MessageBox.Show($"Пользователь {targetUser} не может быть добавлен");
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

                    #region Блок принятия сообщений

                    if (currentCommand.Contains("unknownuser")) // Если не найден адресат
                    {
                        string user = currentCommand.Split('|')[1];
                        MessageBox.Show("Сообщение не дошло до " + user);
                        continue;
                    }

                    if (currentCommand.Contains("msg")) // Принять сообщение
                    {
                        string[] Arguments = currentCommand.Split('|');

                        if (!Arguments[1].Equals(UserName))
                            DataChat[Arguments[1]] += Environment.NewLine + Arguments[1] + ": " + Arguments[2];

                        AddMessage(Arguments[2], Arguments[1]);
                        continue;
                    }

                    if (currentCommand.Contains("chat")) // Принять чат
                    {
                        string chat = currentCommand.Split('|')[1];
                        LoadMessage(chat);
                        continue;
                    }
              

                    #endregion
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Ошибка обработчика команд: " + exp.Message);
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

        public void Send(byte[] buffer) // Отправить сообщение на сервер в формате массив байт
        {
            try
            {
                _serverSocket.Send(buffer);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка отправки байт: " + exp.Message);
            }
        }

        public void Send(string buffer) // Отправить сообщение на сервер в формате строки
        {
            try
            {
                byte[] command = AESCrypt.AESEncrypt(buffer, session_key).Result;
                _serverSocket.Send(command);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка отправки строки: " + exp.Message);
            }
        }

        private void AddMessage(string message, string from = "") // Отображение нового сообщения в чате 
        {
            if (listBox1.SelectedItem != null)
            {
                if (from == label1.Text || from == listBox1.SelectedItem.ToString() || from == "")
                {
                    if (InvokeRequired) // Защита от чужого потока
                    {
                        Invoke(_addMessage, message, from);
                        return;
                    }

                    textBox3.SelectionStart = textBox3.TextLength;
                    textBox3.SelectionLength = message.Length;
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
            textBox3.SelectionStart = textBox3.TextLength;
            textBox3.SelectionLength = Content.Length;
            textBox3.AppendText(Content + Environment.NewLine);
        }

        #endregion

        #region Блок событий формы 

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) // Выбор адресата
        {
            if (listBox1.SelectedItem != null)
            {
                if (!string.IsNullOrWhiteSpace(listBox1.SelectedItem.ToString()))
                {
                    textBox2.Enabled = true;
                    button1.Enabled = true;

                    string chat;

                    if (DataChat.TryGetValue(listBox1.SelectedItem.ToString(), out chat))
                        LoadMessage(chat);

                    //Send($"#getchat|{listBox1.SelectedItem}"); // Не используется
                }
            }                      
        }

        private void button1_Click(object sender, EventArgs e) // Отправка сообщения через кнопку
        {
            if (listBox1.SelectedItem != null)
            {
                if (!string.IsNullOrWhiteSpace(textBox2.Text) && !string.IsNullOrWhiteSpace(listBox1.SelectedItem.ToString()))
                {
                    string msgData = textBox2.Text;
                    string to = listBox1.SelectedItem.ToString();
                    DataChat[to] += Environment.NewLine + UserName + ": " + msgData;
                    Send($"#message|{to}|{msgData}");
                }

                textBox2.Text = string.Empty;
            }               
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e) // Отправка сообщения через Enter       
        {
            if (e.KeyData == Keys.Enter)
            {
                if (listBox1.SelectedItem != null)
                {
                    if (!string.IsNullOrWhiteSpace(textBox2.Text) && !string.IsNullOrWhiteSpace(listBox1.SelectedItem.ToString()))
                    {
                        string msgData = textBox2.Text;
                        string to = listBox1.SelectedItem.ToString();
                        DataChat[to] += Environment.NewLine + UserName + ": " + msgData;
                        Send($"#message|{to}|{msgData}");
                    }

                    textBox2.Text = string.Empty;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) // Найти и добавить по нику
        {
            if(!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                string nick = textBox1.Text;
                Send($"#findbynick|{nick}");
                textBox1.Text = string.Empty;
            }
        }

        private void ChatForm_FormClosing_1(object sender, FormClosingEventArgs e) // При закрытии формы
        {
            if (_serverSocket.Connected)
            {
                Send($"#endsession");
            }

            Application.Exit();
        }

        #endregion
    }
}