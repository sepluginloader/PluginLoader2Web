using System.Runtime.Serialization;
using Tomlyn.Model;

namespace PluginLoader2Web.Data.Configs
{
    public class GithubOAuthCfgs : ITomlMetadataProvider
    {
        public string ClientID { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;


        [IgnoreDataMember]
        public TomlPropertiesMetadata? PropertiesMetadata { get; set; }
    }
}
