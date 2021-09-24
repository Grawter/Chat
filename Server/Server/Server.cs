using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Server
{
    public static class Server
    {
        // Server info
        public static Socket ServerSocket;
        public const string Host = "127.0.0.1";      
        public const int Port = 2222;
        public static bool Work = true;

        // Users info
        public static List<UsersFunc> UserList = new List<UsersFunc>();

        public delegate void UserEvent(string Name);

        public static event UserEvent UserConnected = (Username) => // Подключение пользователя
        {
            Console.WriteLine($"Пользователь {Username} подключился.");
            // здесь отслеживать online/offline пользователя
            //SendUserList();
        };

        public static event UserEvent UserDisconnected = (Username) => // Отключение пользователя
        {
            Console.WriteLine($"Пользователь {Username} отключился.");

            //SendUserList();
        };

        public static void NewUser(UsersFunc usr) // Добавить юзера при подключении
        {
            if (UserList.Contains(usr))
                return;

            UserList.Add(usr);
            UserConnected(usr.Me.UserName);
        }

        public static void EndUser(UsersFunc usr) // Удалить юзера при отключении
        {
            if (!UserList.Contains(usr))
                return;

            UserList.Remove(usr);
            usr.End();
            UserDisconnected(usr.Me.UserName);

        }

        public static UsersFunc GetUserByNick(string Name) // Найти пользователя по нику
        {
            return UserList.FirstOrDefault(u => u.Me.UserName == Name);
        }

        public static void SendUserList() // Отослать список юзеров (Не используется)
        {
            string userList = "#userlist|";

            foreach (var us in UserList)
            {
                if (us == UserList.Last())
                    userList += us.Me.UserName;
                else
                    userList += us.Me.UserName + ",";
            }

            SendAllUsers(userList);
        }

        public static void SendAllUsers(string data) // Отослать всем пользователям в формате строки (Не используется)
        {
            foreach (var us in UserList)
            {
                us.Send(data);
            }
        }

        public static void SendAllUsers(byte[] data) // Отослать всем пользователям в формате байт (Не используется)
        {
            foreach (var us in UserList)
            {
                us.Send(data);
            }
        }      
    }
}