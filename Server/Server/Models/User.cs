
using System.Collections.Generic;

namespace Server.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PrivateID { get; set; }

        public List<Friend> Friends { get; set; } = new List<Friend>();

    }
}