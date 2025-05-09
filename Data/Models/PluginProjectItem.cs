using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PluginLoader2Web.Data.Models
{

    [Table("PluginProjects")]
    public class PluginProjectItem
    {
        public PluginProjectItem()
        {
            PluginId = Guid.NewGuid();
        }


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PluginId { get; set; }


        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        public ulong AuthorID { get; set; } // Username of the plugin author

        public string ToolTip { get; set; } = string.Empty;

        public string LongDescription { get; set; } = string.Empty;

        public string RepoURL { get; set; } = string.Empty;
    }
}
