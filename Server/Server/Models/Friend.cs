using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Friend
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }  
    }
}