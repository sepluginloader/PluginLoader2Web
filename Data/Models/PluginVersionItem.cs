using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PluginLoader2Web.Data.Models
{
    [Table("PluginProjectVersions")]
    public class PluginVersionItem
    {
        PluginVersionItem()
        {
            VersionId = Guid.NewGuid();
            ReleaseDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid VersionId { get; set; }  // Unique internal identifier for the version

        public Guid PluginID { get; set; }  // PluginID to tie to plugin project table

        [MaxLength(50)]
        public string PluginVersion { get; set; } = string.Empty;

        [MaxLength(50)]
        public string CommitID { get; set; } = string.Empty; // CommitID of release

        public bool Beta { get; set; } = false;

        public DateTime ReleaseDate { get; set; } // Release date of the version

        public string ChangeLog { get; set; } = string.Empty; // Changelog for the version





    }
}
