using System.Configuration; 

namespace WindowsFormsApp1
{
    public class Connect
    {
        public string ConnectNoDB()
        {
            // Открываем конфигурационный файл приложения (App.config)
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Считываем параметры подключения без БД (host, uid, pwd)
            string h = currentConfig.AppSettings.Settings["host"].Value;
            string u = currentConfig.AppSettings.Settings["uid"].Value;
            string p = currentConfig.AppSettings.Settings["pwd"].Value;

            // Формируем строку подключения без указания базы данных
            string connectionString = $"host={h};uid={u};pwd={p}";

            // Возвращаем готовую строку подключения
            return connectionString;
        }

        public string ConnectDB()
        {
            // Открываем App.config и получаем настройки подключения
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Читаем параметры сервера, пользователя, пароля и БД
            string h = currentConfig.AppSettings.Settings["host"].Value;
            string u = currentConfig.AppSettings.Settings["uid"].Value;
            string p = currentConfig.AppSettings.Settings["pwd"].Value;
            string db = currentConfig.AppSettings.Settings["database"].Value;

            // Формируем строку подключения *с указанием базы данных*
            string connectionString = $"host={h};uid={u};pwd={p};database={db}";

            // Возвращаем строку подключения
            return connectionString;
        }
    }
}
