using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using Server.Models;
using Server.Crypt;
using Server.Helpers;
using Server.Interfaces;

namespace Server
{ 
    public class UsersFunc
    {
        #region Блок переменных и свойств

        private Socket _userHandle;

        private Thread _userThread;

        private IAsymmCrypt AsymmCrypt = new RSACrypt();
        private ISymmCrypt SymmCrypt = new AESCrypt();

        private RSAParameters publicKey;
        private RSAParameters privateKey;

        private byte[] session_key;

        private IShowInfo showInfo = new ShowInfo();

        public User Me
        {
            get { return me; }
        }
        private User me;

        private bool Handshake = false;
        private bool AuthSuccess = false;
        private bool Silence_mode = false;

        #endregion

        public UsersFunc(Socket handle)
        {
            _userHandle = handle;

            RSA AsimmKey = RSA.Create();
            publicKey = AsimmKey.ExportParameters(false);
            privateKey = AsimmKey.ExportParameters(true);

            Send(publicKey.Modulus);
            Send(EDS.Sign_byte(publicKey.Modulus));

            _userThread = new Thread(Listner)
            {
                IsBackground = true
            };
            _userThread.Start();      
        }

        #region Блок прослушивания и обработчика команд

        private void Listner()
        {
            try
            {
                byte[] buffer, mess;
                int bytesReceive;

                System.Collections.Generic.List<byte> ll = new();
                while (_userHandle.Connected)
                {                  
                    buffer = new byte[32768];
                    bytesReceive = _userHandle.Receive(buffer); // Количество полученных байтов

                    mess = new byte[bytesReceive];
                    Array.Copy(buffer, mess, bytesReceive);
                    buffer = null;

                    if (Handshake)
                    {
                        string command = SymmCrypt.Decrypt(mess, session_key).Result;                    
                        mess = null;
                        HandleCommand(command); // Отправка сообщения на обработку
                    }
                    else
                    {
                        session_key = AsymmCrypt.Decrypt(mess, privateKey);                        
                        mess = null;
                        Handshake = true;
                    }

                }
            }
            catch (Exception exp)
            {
                if (me != null)
                    showInfo.ShowMessage($"{Me.UserName} - Ошибка блока прослушивания: " + exp.Message);

                Server.EndUser(this);
            }
        }

        private void HandleCommand(string cmd)
        {
            try
            {
                string[] commands = cmd.Split('#');
                int countCommands = commands.Length;

                for (int i = 0; i < countCommands; i++)
                {
                    string currentCommand = commands[i];

                    if (string.IsNullOrEmpty(currentCommand))
                        continue;

                    #region Блок регистраци, аутентификации и завершения сессии

                    if (!AuthSuccess)
                    {
                        if (currentCommand.Contains("register")) // Инициализация пользователя
                        {
                            string[] info = currentCommand.Split('|');

                            int status = Server.ContainsUserGlobal(info[1], info[2]);

                            switch (status)
                            {
                                case 1:
                                    Send("#regfault|Ник уже занят");
                                    continue;
                                case 2:
                                    Send("#regfault|Электронная почта уже занята");
                                    continue;
                            }

                            byte[] salt = SHA256Hash.GenerateSalt();

                            me = new User
                            {
                                UserName = info[1],
                                Email = info[2],
                                Password = HashManager.GenerateHash(info[3], salt),
                                Salt = salt,
                                PrivateID = Guid.NewGuid().ToString()
                            };

                            Server.NewUser(this);

                            Server.AddUserGlobal(me);

                            AuthSuccess = true;

                            Send($"#connect|{me.UserName}|{SumByte(session_key)}");
                        }
                        else if (currentCommand.Contains("login"))
                        {
                            string[] info = currentCommand.Split('|');

                            var member = Server.GetUserGlobalByNick(info[1]).Result;
                            if (member == null)
                            {
                                Send("#logfault|Неверный логин или пароль");
                                continue;
                            }

                            if (HashManager.Access(info[2], member.Salt, member.Password))
                            {
                                if (Server.ContainsNick(info[1]))
                                {
                                    Send("#logfault|Сессия занята");
                                    continue;
                                }

                                me = member;
                                Server.NewUser(this);

                                AuthSuccess = true;
                                Send($"#connect|{me.UserName}|{SumByte(session_key)}");

                                string userList = FriendsList.GetList(ref me);

                                if (userList != null)
                                    Send("#userlist|" + userList);
                            }
                            else
                                Send("#logfault|Неверный логин или пароль");
                        }

                        continue;
                    }

                    if (currentCommand.Contains("endsession")) // Завершить сессию
                    {
                        Server.EndUser(this);
                        return;
                    }

                    #endregion

                    #region Блок добавления, удаления и блокирования контактов, проверка статусов

                    if (currentCommand.Contains("findbynick")) // Найти пользователя
                    {
                        string TargetNick = currentCommand.Split('|')[1];

                        UsersFunc targetUser = Server.GetUserByNick(TargetNick);
                        if (targetUser == null || Me.Friends.Any(f => f.Name == TargetNick) || targetUser.Silence_mode)
                        {
                            Send($"#failusersuccess|{TargetNick}");
                        }
                        else if (targetUser.Me == Me)
                            continue;
                        else
                        {
                            targetUser.Send($"#addfriend|{Me.UserName}|{Me.PrivateID}"); // Отправка запроса о дружбе
                            Send($"#friendrequest|{TargetNick}");
                        }

                        continue;
                    }

                    if (currentCommand.Contains("acceptfriend")) // Принятие запроса о дружбе
                    {
                        string friendtNick = currentCommand.Split('|')[1];
                        string friendID = currentCommand.Split('|')[2];

                        UsersFunc friend = Server.GetUserByNick(friendtNick);
                        if (friend != null && friendID.Equals(friend.Me.PrivateID))
                        {
                            me.Friends.Add(new Friend { Name = friendtNick });
                            friend.Me.Friends.Add(new Friend { Name = Me.UserName });
                            Server.SaveChangeGlobal();

                            Send($"#addtolist|{friendtNick}");
                            friend.Send($"#addtolist|{Me.UserName}");
                        }

                        continue;
                    }

                    if (currentCommand.Contains("renouncement")) // Отказ запроса о дружбе
                    {
                        string unfriendlyNick = currentCommand.Split('|')[1];

                        UsersFunc unfriend = Server.GetUserByNick(unfriendlyNick);
                        if (unfriend != null && !unfriend.Silence_mode)
                            unfriend.Send($"#failusersuccess|{Me.UserName}");

                        continue;
                    }

                    if (currentCommand.Contains("delete")) // Удаление из друзей
                    {
                        string unfriendlyNick = currentCommand.Split('|')[1];
                        Server.RemoveFriendShip(Me.UserName, unfriendlyNick);

                        UsersFunc unfr = Server.GetUserByNick(unfriendlyNick);
                        if (unfr != null)
                            unfr.Send($"#remtolist|{Me.UserName}");

                        continue;
                    }

                    if (currentCommand.Contains("silenceon")) // Включить режим не беспокоить
                    {
                        Silence_mode = true;
                        continue;
                    }

                    if (currentCommand.Contains("silenceoff")) // Выключить режим не беспокоить
                    {
                        Silence_mode = false;
                        continue;
                    }

                    if (currentCommand.Contains("isonline")) // Проверка онлайна
                    {
                        string friend = currentCommand.Split('|')[1];

                        if (!Server.ContainsNick(friend))
                            Send($"#offline");

                        continue;
                    }

                    #endregion

                    #region Блок передачи сообщений и ключей

                    if (currentCommand.Contains("message")) // Обработка отправленного сообщения
                    {
                        string TargetName = currentCommand.Split('|')[1];
                        string mode = currentCommand.Split('|')[2];
                        string Content = currentCommand.Split('|')[3];

                        UsersFunc targetUser = Server.GetUserByNick(TargetName);

                        if (targetUser == null || !Me.Friends.Any(fr => fr.Name == TargetName))
                        {
                            Send($"#unknownuser|{TargetName}");
                            continue;
                        }

                        targetUser.Send($"#msg|{Me.UserName}|{mode}|{Content}");

                        continue;
                    }

                    if (currentCommand.Contains("sendAsymm")) // Обмен ключами
                    {
                        string TargetName = currentCommand.Split('|')[1];
                        string publickey = currentCommand.Split('|')[2];

                        UsersFunc targetUser = Server.GetUserByNick(TargetName);

                        if (targetUser == null || !Me.Friends.Any(fr => fr.Name == TargetName))
                        {
                            Send($"#unknownuser|{TargetName}");
                            continue;
                        }

                        targetUser.Send($"#giveAsymm|{Me.UserName}|{publickey}");

                        continue;
                    }

                    #endregion
                }

            }
            catch (Exception exp)
            {
                showInfo.ShowMessage($"{Me.UserName} - Ошибка обработчика команд: " + exp.Message);
            }
        }

        #endregion

        #region Блок обработки основных сценариев

        private void Send(string buffer) // Отослать сообщение в формате строки
        {
            try
            {
                byte[] command = SymmCrypt.Encrypt(buffer + "|" + EDS.Sign_str(buffer), session_key).Result;
                _userHandle.Send(command);
            }
            catch (Exception exp)
            {
                showInfo.ShowMessage($"{Me.UserName} - Ошибка при отправке строки: " + exp.Message);
            }
        }

        private void Send(byte[] buffer) // Отослать сообщение в формате массива байтов
        {
            try
            {
                _userHandle.Send(buffer);
            }
            catch (Exception exp) 
            {
                showInfo.ShowMessage($"{Me.UserName} - Ошибка при отправке массива байт: " + exp.Message);
            }
        }

        public void End() // Завершение сессии сокета
        {
            try
            {
                _userHandle.Close(); // Закрытие подключение сокета пользователя
            }
            catch (Exception exp) 
            {
                showInfo.ShowMessage($"{Me.UserName} - Ошибка при завершении сессии: " + exp.Message);
            }

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
    }
}