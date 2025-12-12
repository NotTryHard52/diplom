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
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Обновляем значения подключения
            currentConfig.AppSettings.Settings["host"].Value = textBox1.Text;
            currentConfig.AppSettings.Settings["uid"].Value = textBox2.Text;
            currentConfig.AppSettings.Settings["pwd"].Value = textBox4.Text;

            // Обновляем таймаут бездействия
            if (int.TryParse(textBox3.Text, out int idleTimeout))
            {
                if (idleTimeout < 30)
                    idleTimeout = 30; // минимум 30 сек
                else if (idleTimeout > 3600)
                    idleTimeout = 3600; // максимум 3600 сек

                if (currentConfig.AppSettings.Settings["IdleTimeoutSeconds"] != null)
                    currentConfig.AppSettings.Settings["IdleTimeoutSeconds"].Value = idleTimeout.ToString();
                else
                    currentConfig.AppSettings.Settings.Add("IdleTimeoutSeconds", idleTimeout.ToString());
            }
            else
            {
                MessageBox.Show("Некорректное значение таймаута", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Сохраняем конфигурацию
            currentConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            MessageBox.Show(
                $"Настройки сохранены.\nТаймаут бездействия: {idleTimeout} с",
                "Сохранено",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            this.Close();
            Application.Restart();
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

            string timeoutValue = currentConfig.AppSettings.Settings["IdleTimeoutSeconds"]?.Value;
            if (!string.IsNullOrEmpty(timeoutValue))
            {
                textBox3.Text = timeoutValue;
            }
            else
            {
                textBox3.Text = "30"; // значение по умолчанию
            }
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

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.English_Symbol(sender, e);
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);
        }
    }
}
