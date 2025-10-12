using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Connect
    {
        public string ConnectNoDB()
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string h = currentConfig.AppSettings.Settings["host"].Value;
            string u = currentConfig.AppSettings.Settings["uid"].Value;
            string p = currentConfig.AppSettings.Settings["pwd"].Value;

            string connectionString = $"host={h};uid={u};pwd={p}";

            return connectionString;
        }
        public string ConnectDB()
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string h = currentConfig.AppSettings.Settings["host"].Value;
            string u = currentConfig.AppSettings.Settings["uid"].Value;
            string p = currentConfig.AppSettings.Settings["pwd"].Value;
            string db = currentConfig.AppSettings.Settings["database"].Value;

            string connectionString = $"host={h};uid={u};pwd={p};database={db}";

            return connectionString;
        }
    }
}
