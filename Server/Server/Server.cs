using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Helpers;
using Server.Interfaces;
using Server.Models;

namespace Server
{
    public static class Server
    {
        static IShowInfo showInfo = new ShowInfo();

        #region Server info

        public static Socket ServerSocket;
        public static readonly string Host = ConfigurationManager.AppSettings["Host"];
        public static readonly int Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
        public static bool Work = true;

        #endregion

        #region Users info

        private static readonly ApplicationContext db = new ApplicationContext();

        private static List<UsersFunc> UserList = new List<UsersFunc>();

        public delegate void UserEvent(string Name);

        public static event UserEvent UserConnected = (Username) => // Подключение пользователя
        {
            showInfo.ShowMessage($"Пользователь {Username} подключился.");
        };

        public static event UserEvent UserDisconnected = (Username) => // Отключение пользователя
        {
            showInfo.ShowMessage($"Пользователь {Username} отключился.");
        };

        #endregion

        #region Блок работы с БД

        public static int ContainsUserGlobal(string NickName, string Email)
        {
            if (db.Users.Any(u => u.UserName == NickName))
                return 1;

            if (db.Users.Any(u => u.Email == Email))
                return 2;

            return 0;
        }

        public static async void AddUserGlobal(User user)
        {
            await db.Users.AddAsync(user);
            SaveChangeGlobal();
        }

        public static async Task<User> GetUserGlobalByNick(string name)
        {
            return await db.Users.Include(u => u.Friends).FirstOrDefaultAsync(u => u.UserName == name);
        }

        public static void RemoveFriendShip(string unfriend1, string unfriend2)
        {
            var unfr1 = GetUserGlobalByNick(unfriend1).Result;
            var unfr2 = GetUserGlobalByNick(unfriend2).Result;

            unfr1.Friends.Remove(unfr1.Friends.FirstOrDefault(un => un.Name == unfriend2));
            unfr2.Friends.Remove(unfr2.Friends.FirstOrDefault(un => un.Name == unfriend1));
            SaveChangeGlobal();
        }

        public static async void SaveChangeGlobal()
        {
           await db.SaveChangesAsync();
        }

        #endregion

        #region Блок работы с подключенными пользователями
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

        public static bool ContainsNick(string Name) // Существует ли пользователь
        {
            return UserList.Any(u => u.Me.UserName == Name);
        }

        #endregion
    }
}