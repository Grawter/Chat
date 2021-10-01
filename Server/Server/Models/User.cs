using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public byte[] Salt { get; set; }

        [Required]
        public string PrivateID { get; set; }

        public List<Friend> Friends { get; set; } = new List<Friend>();

    }
}