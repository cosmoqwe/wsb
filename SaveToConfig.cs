using System.Configuration;
namespace ConsoleApp2
{
    internal class SaveToConfig
    {
        
        public static void Save(string username,string password)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("Username");
            config.AppSettings.Settings.Remove("Password");
            config.AppSettings.Settings.Add("Username", username);
            config.AppSettings.Settings.Add("Password", password);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        public static void Remove()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("Username");
            config.AppSettings.Settings.Remove("Password");
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
