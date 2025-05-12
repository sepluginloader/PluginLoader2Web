using System.Runtime.Serialization;
using System.Text;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace PluginLoader2Web.Data.Configs
{
    public class ConfigFile : ITomlMetadataProvider
    {
        private string? filePath;

        public SQLCredentialsCfgs Database { get; set; } = new SQLCredentialsCfgs();

        public GithubOAuthCfgs Github { get; set; } = new GithubOAuthCfgs();

        public WebServerCfgs Web { get; set; } = new WebServerCfgs();

        public Task SaveAsync()
        {
            return File.WriteAllTextAsync(filePath!, Toml.FromModel(this));
        }

        public void Save()
        {
            File.WriteAllText(filePath!, Toml.FromModel(this));
        }

        public static async Task<ConfigFile?> TryLoadAsync(string filePath)
        {
            string? folder = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            ConfigFile config = new ConfigFile();
            if (!File.Exists(filePath))
            {
                config.filePath = filePath;
                await config.SaveAsync();
                return config;
            }

            string fileText;
            try
            {
                fileText = await File.ReadAllTextAsync(filePath);
            }
            catch (Exception e)
            {
                Log.Error("Error occurred while reading the config: ", e);
                return null;
            }


            DocumentSyntax documentSyntax = Toml.Parse(fileText, filePath);
            if (documentSyntax.HasErrors)
            {
                Log.Error(DiagnosticsToString("Syntax errors were found in the config file: ", documentSyntax.Diagnostics));
                return null;
            }

            TomlModelOptions modelOptions = new TomlModelOptions()
            {
                IgnoreMissingProperties = true,
            };

            if (!documentSyntax.TryToModel(out ConfigFile? existingConfig, out DiagnosticsBag diagnostics, modelOptions))
            {
                Log.Error(DiagnosticsToString("Errors were found in the config file: ", diagnostics));
                return null;
            }

            config = existingConfig;
            config.filePath = filePath;
            await config.SaveAsync();
            return config;
        }

        private static string DiagnosticsToString(string msg, DiagnosticsBag diagnostics)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(msg);
            foreach (DiagnosticMessage message in diagnostics)
                sb.Append(message).AppendLine();
            return sb.ToString();
        }


        [IgnoreDataMember]
        public TomlPropertiesMetadata? PropertiesMetadata { get; set; }
    }
}
