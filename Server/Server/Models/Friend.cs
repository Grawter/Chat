using System.Collections.Generic;

namespace Server.Models
{
    public class Friend
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }  
    }
}