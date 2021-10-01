using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Server.Crypt;
using System.Text;
using Server.Helpers;

namespace Server
{
    public class UsersFunc
    {
        private User me;

        public User Me
        {
            get { return me; }
        }

        private ApplicationContext db;

        private Socket _userHandle;

        RSAParameters publicKey;
        RSAParameters privateKey;

        byte[] session_key;

        private Thread _userThread;

        private bool Handshake = false;     

        private bool AuthSuccess = false;

        private bool Silence_mode = false;

        public UsersFunc(Socket handle)
        {
            //db = new ApplicationContext();

            _userHandle = handle;

            RSA RSA = RSA.Create();

            publicKey = RSA.ExportParameters(false);
            privateKey = RSA.ExportParameters(true);
            
            Send(publicKey.Modulus);
            
            _userThread = new Thread(listner);
            _userThread.IsBackground = true;
            _userThread.Start();      
        }

        private void listner()
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
                        if (db == null)
                            db = new ApplicationContext();

                        string command = AESCrypt.AESDecrypt(mess, session_key).Result;
                        handleCommand(command); // Отправка сообщения на обработку
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
                    Console.WriteLine($"{Me.UserName} - Ошибка блока прослушивания: " + exp.Message);
                
                Server.EndUser(this);
            }
        }

        private void handleCommand(string cmd)
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

                            var member = db.Users.FirstOrDefault(u => u.UserName == info[1]);

                            if (member == null)
                            {
                                var member2 = db.Users.FirstOrDefault(u => u.Email == info[2]);

                                if (member2 == null)
                                {
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

                                    db.Users.Add(me);
                                    db.SaveChanges();

                                    AuthSuccess = true;

                                    Send("#connect");
                                }
                                else
                                    Send("#emailnotaccess");
                            }
                            else
                                Send("#usernamenotaccess");

                        }
                        else if (currentCommand.Contains("login"))
                        {
                            string[] info = currentCommand.Split('|');

                            var member = db.Users.Include(u => u.Friends).FirstOrDefault(u => u.UserName == info[1]);

                            if (member != null)
                            {                          
                                if (HashManager.Access(info[2], member.Salt, member.Password))
                                {
                                    me = member;
                                    Server.NewUser(this);

                                    AuthSuccess = true;

                                    Send("#connect");

                                    string userList = "";

                                    foreach (var fr in Me.Friends)
                                    {
                                        if (fr == Me.Friends.Last())
                                            userList += fr.Name;
                                        else
                                            userList += fr.Name + ",";
                                    }

                                    if (userList != "")
                                        Send("#userlist|" + userList);
                                }
                                else
                                    Send("#logfault");
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

                    #region Блок добавления, удаления и блокирования контактов 

                    if (currentCommand.Contains("findbynick")) // Найти пользователя
                    {
                        string TargetNick = currentCommand.Split('|')[1];

                        UsersFunc targetUser = Server.GetUserByNick(TargetNick);
                        if (targetUser == null || Me.Friends.FirstOrDefault(f => f.Name == TargetNick) != null || targetUser.Silence_mode)
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

                            db.Users.Update(friend.Me);
                            db.SaveChanges();

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
                        using (var db = new ApplicationContext())
                        {
                            string unfriendlyNick = currentCommand.Split('|')[1];

                            me.Friends.Remove(me.Friends.FirstOrDefault(uf => uf.Name == unfriendlyNick));

                            for (int j = 0; j < db.Friends.Count(fr => fr.Name == unfriendlyNick && fr.UserId == me.Id); j++)
                            {
                                db.Friends.Remove(db.Friends.FirstOrDefault(fr => fr.Name == unfriendlyNick && fr.UserId == me.Id));
                            }

                            var unfriend = db.Users.Include(u => u.Friends).FirstOrDefault(uf => uf.UserName == unfriendlyNick);
                            unfriend.Friends.Remove(unfriend.Friends.FirstOrDefault(uf => uf.Name == Me.UserName));

                            db.SaveChanges();

                            UsersFunc unfr = Server.GetUserByNick(unfriendlyNick);
                            if (unfr != null)
                            {
                                unfr.me = unfriend;
                                unfr.Send($"#remtolist|{Me.UserName}");
                            }             
                        }

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
                        UsersFunc targetUser = Server.GetUserByNick(friend);

                        if (targetUser == null)
                            Send($"#offline");

                        continue;
                    }

                    #endregion

                    #region Блок передачи сообщений

                    if (currentCommand.Contains("message")) // Обработка отправленного сообщения
                    {
                        string TargetName = currentCommand.Split('|')[1];
                        string Content = currentCommand.Split('|')[2];

                        UsersFunc targetUser = Server.GetUserByNick(TargetName);

                        if (targetUser == null || Me.Friends.FirstOrDefault(fr => fr.Name == TargetName) == null)
                        {
                            Send($"#unknownuser|{TargetName}");
                            continue;
                        }

                        targetUser.Send($"#msg|{Me.UserName}|{Content}");

                        continue;
                    }

                    if (currentCommand.Contains("secmess")) // Обработка отправленного сообщения с двойной шифровкой
                    {
                        string TargetName = currentCommand.Split('|')[1];
                        
                        UsersFunc targetUser = Server.GetUserByNick(TargetName);

                        if (targetUser == null || Me.Friends.FirstOrDefault(fr => fr.Name == TargetName) == null)
                        {
                            Send($"#unknownuser|{TargetName}");
                            continue;
                        }

                        byte[] mess = new byte[32768];
                        int bytesReceive = _userHandle.Receive(mess);
                        byte[] enc_mess = new byte[bytesReceive];
                        Array.Copy(mess, enc_mess, bytesReceive);

                        byte[] secure_mess = AESCrypt.AESEncrypt(enc_mess, targetUser.session_key).Result;
                        targetUser.Send($"#secmess|{Me.UserName}");
                        targetUser.Send(secure_mess);

                        continue;
                    }

                    #endregion
                }

            }
            catch (Exception exp) 
            { 
                Console.WriteLine($"{Me.UserName} - Ошибка обработчика команд: " + exp.Message); 
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
                Console.WriteLine($"{Me.UserName} - Ошибка при отправке строки: " + exp.Message);
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
                Console.WriteLine($"{Me.UserName} - Ошибка при отправке массива байт: " + exp.Message);
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
                Console.WriteLine($"{Me.UserName} - Ошибка при завершении сессии: " + exp.Message);
            }

        }

        #endregion
    }
}