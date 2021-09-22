using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Server.Crypt;

namespace Server
{
    public class UsersFunc
    {
        private User me;

        public User Me
        {
            get { return me; }
        }

        private readonly ApplicationContext db;

        private Socket _userHandle;

        RSAParameters publicKey;
        RSAParameters privateKey;

        byte[] session_key;

        private Thread _userThread;

        private bool Handshake = false;

        private bool AuthSuccess = false;

        private Dictionary<UsersFunc, string> DataChat;

        public UsersFunc(Socket handle)
        {
            db = new ApplicationContext();

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
                    byte[] buffer = new byte[2048];
                    int bytesReceive = _userHandle.Receive(buffer); // Количество полученных байтов

                    byte[] mess = new byte[bytesReceive];
                    Array.Copy(buffer, mess, bytesReceive);

                    if (Handshake)
                    {
                        string command = AESCrypt.AESDecrypt(mess, session_key);
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

                    #region Блок регистрации и аутентификации

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
                                    me = new User { UserName = info[1], Email = info[2], Password = info[3], PrivateID = Guid.NewGuid().ToString() }; // подумать про валидацию
                                    DataChat = new Dictionary<UsersFunc, string>();
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
                                if (info[2].Equals(member.Password)) // не забыть сделать шифровку дешифровку
                                {
                                    me = member;
                                    DataChat = new Dictionary<UsersFunc, string>();
                                    Server.NewUser(this);

                                    AuthSuccess = true;

                                    Send("#connect");

                                    string userList = "";

                                    foreach (var fr in Me.Friends)
                                    {
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

                    #endregion

                    #region Блок добавления и удаления контактов 

                    if (currentCommand.Contains("findbynick")) // Найти пользователя
                    {
                        string TargetNick = currentCommand.Split('|')[1];

                        UsersFunc targetUser = Server.GetUserByNick(TargetNick);
                        if (targetUser == null || Me.Friends.FirstOrDefault(f => f.Name == TargetNick) != null)
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
                            DataChat.Add(friend, null);

                            me.Friends.Add(new Friend { Name = friendtNick });
                            friend.Me.Friends.Add(new Friend { Name = Me.UserName });

                            db.Users.Update(friend.Me);
                            db.SaveChanges();

                            Send($"#addtolist|{friendtNick}");

                            friend.DataChat.Add(this, null);
                            friend.Send($"#addtolist|{Me.UserName}");
                            friend.Send($"#addusersuccess|{Me.UserName}");
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
                                unfr.Send($"#remtolist|{Me.UserName}");
                        }

                        continue;
                    }

                    #endregion

                    #region Блок передачи сообщений

                    if (currentCommand.Contains("message")) // Обработка отправленного сообщения
                    {
                        string[] Arguments = currentCommand.Split('|');
                        string TargetName = Arguments[1];
                        string Content = Arguments[2];

                        UsersFunc targetUser = Server.GetUserByNick(TargetName);

                        if (targetUser == null || Me.Friends.FirstOrDefault(fr => fr.Name == TargetName) == null)
                        {
                            Send($"#unknownuser|{TargetName}");
                            continue;
                        }

                        SendMessage(Me.UserName, Content);
                        targetUser.SendMessage(Me.UserName, Content);

                        DataChat[targetUser] += Environment.NewLine + Me.UserName + ": " + Content;
                        targetUser.DataChat[this] += Environment.NewLine + Me.UserName + ": " + Content;

                        continue;
                    }

                    if (currentCommand.Contains("getchat")) // Подгрузка чата
                    {
                        string friendtNick = currentCommand.Split('|')[1];
                        
                        UsersFunc friend = Server.GetUserByNick(friendtNick);
                        
                        if (friend != null && Me.Friends.FirstOrDefault(fr => fr.Name == friendtNick) != null)
                        {
                            string mess;
                            
                            if (DataChat.TryGetValue(friend, out mess))
                                Send($"#chat|{mess}");
                            else
                            {
                                DataChat.Add(friend, null);
                                DataChat.TryGetValue(friend, out mess);
                                Send($"#chat|{mess}");
                            }

                        }

                        continue;
                    }

                    #endregion

                    #region Блок передачи файлов

                    if (currentCommand.Contains("sendfileto")) // Обработка отправленного файла
                    {
                        string[] Arguments = currentCommand.Split('|');
                        string TargetName = Arguments[1];
                        int FileSize = int.Parse(Arguments[2]);
                        string FileName = Arguments[3];
                        byte[] fileBuffer = new byte[FileSize];
                        _userHandle.Receive(fileBuffer); // Получаем байты файла

                        UsersFunc targetUser = Server.GetUserByNick(TargetName);

                        if (targetUser == null || Me.Friends.FirstOrDefault(fr => fr.Name == TargetName) == null)
                        {
                            Send($"#unknownuser|{TargetName}");
                            continue;
                        }

                        FileD newFile = new FileD
                        {
                            ID = Server.Files.Count + 1,
                            FileName = FileName,
                            From = me.UserName,
                            To = TargetName,
                            fileBuffer = fileBuffer,
                            FileSize = fileBuffer.Length
                        };

                        Server.Files.Add(newFile);

                        targetUser.Send($"#getfile|{newFile.FileName}|{newFile.From}|{newFile.fileBuffer.Length}|{newFile.ID}");

                        continue;
                    }

                    if (currentCommand.Contains("accfile")) // Отправка файла пользователю
                    {
                        string id = currentCommand.Split('|')[1];
                        FileD file = Server.GetFileByID(int.Parse(id));

                        if (file.ID == 0 || file.To != Me.UserName || Me.Friends.FirstOrDefault(fr => fr.Name == file.From) == null)
                        {
                            Send($"#unknownfile");
                            continue;
                        }

                        Send(file.fileBuffer);
                        Server.Files.Remove(file);

                        continue;
                    }

                    if (currentCommand.Contains("notaccfile")) // Удаление файла в случае отказа принятия
                    {
                        string id = currentCommand.Split('|')[1];
                        FileD file = Server.GetFileByID(int.Parse(id));

                        if (file.ID == 0 || file.To != me.UserName)
                        {
                            continue;
                        }

                        Server.Files.Remove(file);

                        continue;
                    }

                    if (currentCommand.Contains("endsession")) // Завершить сессию
                    {
                        Server.EndUser(this);
                        return;
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

        public void SendMessage(string from, string message) // Отослать сообщение 
        {
            Send($"#msg|{from}|{message}");
        }

        public void Send(string buffer) // Отослать сообщение в формате строки
        {
            try
            {
                _userHandle.Send(Encoding.Unicode.GetBytes(buffer));
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