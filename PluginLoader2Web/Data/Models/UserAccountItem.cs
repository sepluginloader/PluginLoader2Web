using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PluginLoader2Web.Data.Models
{
    [Table("UserAccounts")]
    public class UserAccountItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong GithubID { get; set; } // Unique identifier for the user

        public ulong DiscordID { get; set; } // Possible linked Discord ID of the user

        [MaxLength(256)]
        public string Username { get; set; } = string.Empty; // Username of the user

        [MaxLength(256)]
        public string Email { get; set; } = string.Empty; // Email address of the user

        public byte Role { get; set; } // Role of the user (e.g., admin, user, etc.)

        public DateTime JoinDate { get; set; } = DateTime.UtcNow; // Date when the user joined

        public DateTime LastUpdate { get; set; } = DateTime.UtcNow; // Last update date for the user





        public UserAccountItem() { }
        public UserAccountItem(ulong userId, string username, string email, byte role)
        {
            GithubID = userId;
            Username = username;
            Email = email;
            Role = role;
            
            //Set any missing date times
            JoinDate = DateTime.UtcNow;
            LastUpdate = DateTime.UtcNow;
        }


    }
}
