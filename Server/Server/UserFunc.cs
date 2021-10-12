using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Server.Models;
using System.Security.Cryptography;
using Server.Crypt;
using Server.Helpers;
using Server.Interfaces;

namespace Server
{
    public class UsersFunc
    {
        private Socket _userHandle;

        private Thread _userThread;

        RSAParameters publicKey;
        RSAParameters privateKey;

        byte[] session_key;

        IShowInfo showInfo = new ShowInfo();

        public User Me
        {
            get { return me; }
        }
        private User me;

        private bool Handshake = false;     
        private bool AuthSuccess = false;
        private bool Silence_mode = false;

        public UsersFunc(Socket handle)
        {
            _userHandle = handle;

            RSA RSA = RSA.Create();
            publicKey = RSA.ExportParameters(false);
            privateKey = RSA.ExportParameters(true);
            
            Send(publicKey.Modulus);

            _userThread = new Thread(Listner)
            {
                IsBackground = true
            };
            _userThread.Start();      
        }

        private void Listner()
        {
            try
            {
                while (_userHandle.Connected)
                {
                    byte[] buffer = new byte[32768];
                    int bytesReceive = _userHandle.Receive(buffer); // Количество полученных байтов

                    byte[] mess = new byte[bytesReceive];
                    Array.Copy(buffer, mess, bytesReceive);

                    if (Handshake)
                    {
                        string command = AESCrypt.AESDecrypt(mess, session_key).Result;
                        HandleCommand(command); // Отправка сообщения на обработку
                    }   
                    else
                    {
                        session_key = RSACrypt.RSADecrypt(mess, privateKey);
                        Handshake = true;
                    }

                }
            }
            catch (Exception exp)
            {
                if(me != null)
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
                                    Send("#usernamenotaccess");
                                     continue;
                                case 2:
                                    Send("#emailnotaccess");
                                    continue;
                            }                      

                            byte[] salt = Hash.GenerateSalt();

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

                            Send("#connect");
                        }
                        else if (currentCommand.Contains("login"))
                        {
                            string[] info = currentCommand.Split('|');

                            var member = Server.GetUserGlobalByNick(info[1]).Result;
                            if (member == null)
                            {
                                Send("#logfault");
                                continue;
                            }

                            if (HashManager.Access(info[2], member.Salt, member.Password))
                            {
                                if (Server.ContainsNick(info[1]))
                                {
                                    Send("#sessionbusy");
                                    continue;
                                }

                                me = member;
                                Server.NewUser(this);

                                AuthSuccess = true;

                                Send("#connect");

                                string userList = FriendsList.GetList(ref me);

                                if (userList != null)
                                    Send("#userlist|" + userList);
                            }
                            else
                                Send("#logfault");
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
                        if (unfriend != null)
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

                    if (currentCommand.Contains("sendRSA")) // Обмен ключами
                    {
                        string TargetName = currentCommand.Split('|')[1];
                        string publickey = currentCommand.Split('|')[2];

                        UsersFunc targetUser = Server.GetUserByNick(TargetName);

                        if (targetUser == null || !Me.Friends.Any(fr => fr.Name == TargetName))
                        {
                            Send($"#unknownuser|{TargetName}");
                            continue;
                        }

                        targetUser.Send($"#giveRSA|{Me.UserName}|{publickey}");

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

        #region Блок обработки основных сценариев

        public void Send(string buffer) // Отослать сообщение в формате строки
        {
            try
            {
                byte[] command = AESCrypt.AESEncrypt(buffer, session_key).Result;
                _userHandle.Send(command);
            }
            catch (Exception exp)
            {
                showInfo.ShowMessage($"{Me.UserName} - Ошибка при отправке строки: " + exp.Message);
            }
        }

        public void Send(byte[] buffer) // Отослать сообщение в формате массива байтов
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
    }
}