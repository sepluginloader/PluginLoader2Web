namespace PluginLoader2Web.Data.Configs
{
    public class SQLCredentials
    {

        public string SQLHost { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "PluginLoader2";

        public string Username { get; set; } = "Username";
        public string Password { get; set; } = "Password";





        public string ConnectionString => $"Host={SQLHost};Port={Port};Database={Database};Username={Username};Password={Password};Include Error Detail=true";


    }
}
