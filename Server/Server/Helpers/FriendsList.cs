using Server.Models;
using System.Linq;

namespace Server.Helpers
{
    public class FriendsList
    {
        public static string GetList(ref User user)
        {
            if (user.Friends.Count > 0)
            {
                string list = "";

                foreach (var fr in user.Friends)
                {
                    if (fr == user.Friends.Last())
                        list += fr.Name;
                    else
                        list += fr.Name + ",";
                }

                return list;
            }

            return null;
        }

    }
}