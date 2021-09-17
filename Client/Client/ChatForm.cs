﻿using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

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

            contextMenuStrip1 = new ContextMenuStrip(); // Инициализиуем контекстное меню

            ToolStripMenuItem SendFileItem = new ToolStripMenuItem("Отправить файл"); // Создание элемента меню

            SendFileItem.Click += delegate // Функционал элемента
            {
                if (listBox1.SelectedItems.Count == 0)
                {
                    return;
                }

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.ShowDialog();

                if (!File.Exists(dialog.FileName))
                {
                    MessageBox.Show($"Файл {dialog.FileName} не найден!");
                    return;
                }

                FileInfo fi = new FileInfo(dialog.FileName);
                byte[] buffer = File.ReadAllBytes(dialog.FileName);

                Send($"#sendfileto|{listBox1.SelectedItem}|{buffer.Length}|{fi.Name}"); // Передача информации о файле
                Send(buffer); // Передача самого файла
                MessageBox.Show("Файл загружен");
            };

            contextMenuStrip1.Items.Add(SendFileItem); // Добавить ранее созданного элемента в меню

            ToolStripMenuItem DeleteUser = new ToolStripMenuItem("Удалить");

            DeleteUser.Click += delegate
            {

            };

            contextMenuStrip1.Items.Add(DeleteUser);

            ToolStripMenuItem BlockUser = new ToolStripMenuItem("Заблокировать");

            BlockUser.Click += delegate
            {

            };

            contextMenuStrip1.Items.Add(BlockUser);

            listBox1.ContextMenuStrip = contextMenuStrip1; // Закрепление созданного контекстного меню в листбоксе

            _addMessage = new ChatEvent(AddMessage); // Связывание делегата с функцией
            _loadMessage = new LoadChatEvent(LoadMessage);
        }

        private void ChatForm_Load(object sender, EventArgs e) // При загрузке чата
        {
            IPAddress temp = IPAddress.Parse(_host);
            _serverSocket = new Socket(temp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Connect(new IPEndPoint(temp, _port));

            if (_serverSocket.Connected)
            {
                listenThread = new Thread(listner);
                listenThread.IsBackground = true;
                listenThread.Start();
            }
            else
                MessageBox.Show("Связь с сервером не установлена.");
        }

        public void listner()
        {
            try
            {
                while (_serverSocket.Connected)
                {
                    byte[] buffer = new byte[2048];
                    int bytesReceive = _serverSocket.Receive(buffer);
                    handleCommand(Encoding.Unicode.GetString(buffer, 0, bytesReceive));
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

                    if (string.IsNullOrEmpty(currentCommand))
                        continue;

                    #region Блок подключения и валидации данных

                    if (currentCommand.Contains("connect")) // При удачной идентификации
                    {
                        Invoke((MethodInvoker)delegate // Обеспечение доступа к элементам формы в потоке, в котором они были созданы
                        {
                            richTextBox1.Text = "Подключение выполнено!";
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

                    #region Блок добавления контактов

                    if (currentCommand.Contains("userlist")) // Обновление списка друзей
                    {
                        string[] Users = currentCommand.Split('|')[1].Split(',');
                        int countUsers = Users.Length;

                        listBox1.Invoke((MethodInvoker)delegate { listBox1.Items.Clear(); }); // Очищение списка

                        for (int j = 0; j < countUsers; j++) // Добавляем пользователей
                        {
                            listBox1.Invoke((MethodInvoker)delegate { listBox1.Items.Add(Users[j]); });
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

                        DialogResult Result = MessageBox.Show($"Вы хотите начать диалог с {guest_name} и добавить его в контакты?", "Начало диалога", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (Result == DialogResult.Yes)
                        {
                            Send($"#acceptfriend|{guest_name}");
                        }
                        else
                            Send($"#renouncement|{guest_name}");

                        continue;
                    }

                    if (currentCommand.Contains("addusersuccess")) // Положительный ответ для того, кто отправил запрос дружбы
                    {
                        string targetUser = currentCommand.Split('|')[1];
                        MessageBox.Show($"Пользователь {targetUser} был добавлен");
                        continue;
                    }

                    if (currentCommand.Contains("failusersuccess")) // Отрицательный ответ для того, кто отправил запрос дружбы
                    {
                        string targetUser = currentCommand.Split('|')[1];
                        MessageBox.Show($"Пользователь {targetUser} не может быть добавлен");
                        continue;
                    }

                    if (currentCommand.Contains("addtolist")) // Добавление в пользователя в листбокс
                    {
                        string new_friend = currentCommand.Split('|')[1];
                        listBox1.Invoke((MethodInvoker)delegate { listBox1.Items.Add(new_friend); }); // добавляем пользователя
                        continue;
                    }

                    #endregion

                    #region Блок принятия сообщений и файлов

                    if (currentCommand.Contains("unknownuser")) // Если не найден адресат
                    {
                        string user = currentCommand.Split('|')[1];
                        MessageBox.Show("Сообщение не дошло до " + user);
                        continue;
                    }

                    if (currentCommand.Contains("msg")) // Принять сообщение
                    {
                        string[] Arguments = currentCommand.Split('|');
                        AddMessage(Arguments[2], Arguments[1]);
                        continue;
                    }

                    if (currentCommand.Contains("chat")) // Принять чат
                    {
                        string chat = currentCommand.Split('|')[1];
                        LoadMessage(chat);
                        continue;
                    }

                    if (currentCommand.Contains("unknownfile")) // Если не найден файл
                    {
                        MessageBox.Show("Файл не найден");
                        continue;
                    }

                    if (currentCommand.Contains("getfile")) // Принять файл
                    {
                        string[] Arguments = currentCommand.Split('|');
                        string FileName = Arguments[1];
                        string FromName = Arguments[2];
                        string FileSize = Arguments[3];
                        string IdFile = Arguments[4];

                        DialogResult Result = MessageBox.Show($"Вы хотите принять файл {FileName} размером {FileSize} от {FromName}", "Файл", MessageBoxButtons.YesNo);

                        if (Result == DialogResult.Yes)
                        {
                            Thread.Sleep(1000);

                            Send("#accfile|" + IdFile);

                            byte[] fileBuffer = new byte[int.Parse(FileSize)];
                            _serverSocket.Receive(fileBuffer);

                            if (!Directory.Exists(Environment.CurrentDirectory + "\\Download"))
                                Directory.CreateDirectory("Download");

                            File.WriteAllBytes("Download\\" + FileName, fileBuffer);
                            MessageBox.Show($"Файл {FileName} принят.");
                        }
                        else
                            Send("#notaccfile|" + IdFile);

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
        public void Registration() // Регистрация
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return;

            Send($"#register|{userName}|{email}|{password}");
            password = "";
        }

        public void Login() // Логин
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return;

            Send($"#login|{userName}|{password}");
            password = "";
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

        public void Send(string Buffer) // Отправить сообщение на сервер в формате строки
        {
            try
            {
                _serverSocket.Send(Encoding.Unicode.GetBytes(Buffer));
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

                    richTextBox1.SelectionStart = richTextBox1.TextLength;
                    richTextBox1.SelectionLength = message.Length;
                    richTextBox1.AppendText($"{from}:{message}" + Environment.NewLine);
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

            richTextBox1.Clear();
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionLength = Content.Length;
            richTextBox1.AppendText(Content + Environment.NewLine);
        }

        #endregion

        #region Блок событий формы 

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) // Выбор адресата
        {
            textBox2.Enabled = true;
            button1.Enabled = true;
            Send($"#getchat|{listBox1.SelectedItem}");
        }

        private void button1_Click(object sender, EventArgs e) // Отправка сообщения через кнопку
        {
            string msgData = textBox2.Text;

            if (string.IsNullOrEmpty(msgData) || string.IsNullOrEmpty(listBox1.SelectedItem.ToString()))
                return;

            string to = listBox1.SelectedItem.ToString();
            Send($"#message|{to}|{msgData}");

            textBox2.Text = string.Empty;
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e) // Отправка сообщения через Enter
        {
            if (e.KeyData == Keys.Enter)
            {
                string msgData = textBox2.Text;

                if (string.IsNullOrEmpty(msgData) || string.IsNullOrEmpty(listBox1.SelectedItem.ToString()))
                    return;

                string to = listBox1.SelectedItem.ToString();
                Send($"#message|{to}|{msgData}");

                textBox2.Text = string.Empty;
            }
        }

        private void button2_Click(object sender, EventArgs e) // Найти и добавить по нику
        {
            string nick = textBox1.Text;
            Send("#findbynick|" + nick);
        }

        private void ChatForm_FormClosing_1(object sender, FormClosingEventArgs e) // При закрытии формы
        {
            if (_serverSocket.Connected)
                Send("#endsession");

            Application.Exit();
        }

        #endregion
    }
}