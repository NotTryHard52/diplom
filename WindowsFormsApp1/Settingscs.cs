using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Settingscs : Form
    {
        // Конструктор формы
        public Settingscs()
        {
            InitializeComponent();
        }

        // Событие кнопки "Сохранить" настройки подключения
        private void button2_Click(object sender, EventArgs e)
        {
            // Загружаем текущую конфигурацию приложения
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Обновляем значения параметров в appSettings
            currentConfig.AppSettings.Settings["host"].Value = textBox1.Text;
            currentConfig.AppSettings.Settings["uid"].Value = textBox2.Text;
            currentConfig.AppSettings.Settings["pwd"].Value = textBox4.Text;

            // Сохраняем изменения в конфигурации
            currentConfig.Save(ConfigurationSaveMode.Modified);

            // Обновляем раздел appSettings в памяти приложения
            ConfigurationManager.RefreshSection("appSettings");

            // Показываем сообщение об успешном сохранении
            MessageBox.Show(
                $"Новая строка подключения сохранена\nhost={textBox1.Text};uid={textBox2.Text};pwd={textBox4.Text};",
                "Сохранить",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            this.Close(); // Закрываем форму настроек
            Application.Restart(); // Перезапускаем приложение, чтобы применились новые настройки
        }

        // Событие загрузки формы
        private void Settingscs_Load(object sender, EventArgs e)
        {
            // Загружаем текущую конфигурацию приложения
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Подставляем текущие значения подключения в текстовые поля
            textBox1.Text = currentConfig.AppSettings.Settings["host"].Value;
            textBox2.Text = currentConfig.AppSettings.Settings["uid"].Value;
            textBox4.Text = currentConfig.AppSettings.Settings["pwd"].Value;
        }

        // Событие кнопки "Проверить подключение"
        private void button1_Click(object sender, EventArgs e)
        {
            // Формируем строку подключения на основе введенных значений
            string connectionString = $"host={textBox1.Text};uid={textBox2.Text};pwd={textBox4.Text}";

            MySqlConnection con = new MySqlConnection(connectionString);

            try
            {
                // Пытаемся открыть соединение с базой данных
                con.Open();

                // Если удалось, выводим сообщение об успешном подключении
                MessageBox.Show("Успешное подключение к СУБД", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                con.Close(); // Закрываем соединение
            }
            catch (Exception ex)
            {
                // Если подключение не удалось, выводим сообщение об ошибке
                MessageBox.Show(
                    $"Ошибка подключения к {connectionString}\n{ex.Message}",
                    "ОШИБКА",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
