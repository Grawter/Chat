using Server.Models;
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

        //Files info
        public static List<FileD> Files = new List<FileD>();

        // Users info
        public static int CountUsers = 0;
        public static List<UsersFunc> UserList = new List<UsersFunc>();

        public delegate void UserEvent(string Name);

        public static event UserEvent UserConnected = (Username) => // Подключение пользователя
        {
            Console.WriteLine($"Пользователь {Username} подключился.");
            CountUsers++;
            // здесь отслеживать online/offline пользователя
            //SendUserList();
        };

        public static event UserEvent UserDisconnected = (Username) => // Отключение пользователя
        {
            Console.WriteLine($"Пользователь {Username} отключился.");
            CountUsers--;

            //SendUserList();
        };

        public static FileD GetFileByID(int ID) // Получить файл по id
        {
            for (int i = 0; i < Files.Count; i++)
            {
                if (Files[i].ID == ID)
                    return Files[i];
            }
            return new FileD { ID = 0 };
        }

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

            for (int i = 0; i < CountUsers; i++)
            {
                userList += UserList[i].Me.UserName + ",";
            }

            SendAllUsers(userList);
        }

        public static void SendAllUsers(string data) // Отослать всем пользователям в формате строки (Не используется)
        {
            for (int i = 0; i < CountUsers; i++)
            {
                UserList[i].Send(data);
            }
        }

        public static void SendAllUsers(byte[] data) // Отослать всем пользователям в формате байт (Не используется)
        {
            for (int i = 0; i < CountUsers; i++)
            {
                UserList[i].Send(data);
            }
        }      
    }
}