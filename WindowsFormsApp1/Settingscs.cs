using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Settingscs : Form
    {
        public Settingscs()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            currentConfig.AppSettings.Settings["host"].Value = textBox1.Text;
            currentConfig.AppSettings.Settings["uid"].Value = textBox2.Text;
            currentConfig.AppSettings.Settings["pwd"].Value = textBox4.Text;

            currentConfig.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");

            MessageBox.Show($"Новая строка подключения сохранена\nhost={textBox1.Text};uid={textBox2.Text};pwd={textBox4.Text};", "Сохранить", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            Application.Restart();
        }

        private void Settingscs_Load(object sender, EventArgs e)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            textBox1.Text = currentConfig.AppSettings.Settings["host"].Value;
            textBox2.Text = currentConfig.AppSettings.Settings["uid"].Value;
            textBox4.Text = currentConfig.AppSettings.Settings["pwd"].Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = $"host={textBox1.Text};uid={textBox2.Text};pwd={textBox4.Text}";

            MySqlConnection con = new MySqlConnection(connectionString);

            try
            {
                con.Open();

                MessageBox.Show($"Успешное подключение к СУБД", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к {connectionString}\n{ex.Message}", "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
    }
}
