using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PluginLoader2Web.Data.Models
{
    [Table("UserAccounts")]
    public class UserAccountItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong UserId { get; set; } // Unique identifier for the user

        [MaxLength(256)]
        public string Username { get; set; } = string.Empty; // Username of the user

        [MaxLength(256)]
        public string Email { get; set; } = string.Empty; // Email address of the user

        public byte Role { get; set; } // Role of the user (e.g., admin, user, etc.)
        public UserAccountItem() { }
        public UserAccountItem(ulong userId, string username, string email, byte role)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Role = role;
        }


    }
}
