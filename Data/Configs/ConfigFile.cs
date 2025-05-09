using System.Runtime.Serialization;
using System.Text;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace PluginLoader2Web.Data.Configs
{
    public class ConfigFile : ITomlMetadataProvider
    {
        private string filePath;

        public SQLCredentials WebServer { get; set; } = new SQLCredentials();

        public Task SaveAsync()
        {
            return File.WriteAllTextAsync(filePath, Toml.FromModel(this));
        }

        public void Save()
        {
            File.WriteAllText(filePath, Toml.FromModel(this));
        }

        public static async Task<ConfigFile> TryLoadAsync(string filePath)
        {
            string folder = Path.GetDirectoryName(filePath);
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
                Console.WriteLine("Error occurred while reading the config: ", e);
                return null;
            }


            DocumentSyntax documentSyntax = Toml.Parse(fileText, filePath);
            if (documentSyntax.HasErrors)
            {
                Console.WriteLine(DiagnosticsToString("Syntax errors were found in the config file: ", documentSyntax.Diagnostics));
                return null;
            }

            TomlModelOptions modelOptions = new TomlModelOptions()
            {
                IgnoreMissingProperties = true,
            };

            if (!documentSyntax.TryToModel(out config, out DiagnosticsBag diagnostics, modelOptions))
            {
                Console.WriteLine(DiagnosticsToString("Errors were found in the config file: ", diagnostics));
                config = null;
                return null;
            }

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
        public TomlPropertiesMetadata PropertiesMetadata { get; set; }
    }
}
