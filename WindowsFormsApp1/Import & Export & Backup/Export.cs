using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Export : Form
    {
        string connectionString;
        string savePath;
        public Export()
        {
            InitializeComponent();
        }

        private void Export_Load(object sender, EventArgs e)
        {
            LoadTables();
            comboBox1.SelectedIndex = -1;
        }

        void LoadTables()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SHOW tables;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt2 = new DataTable();
                da.Fill(dt2);
                comboBox1.DataSource = dt2;
                comboBox1.DisplayMember = "Tables_in_vkr";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Выберите место для сохранения";
                sfd.Filter = "CSV файл (*.csv)|*.csv";
                sfd.FileName = "export";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    savePath = sfd.FileName;
                    label2.Text = "*путь выбран";
                    label3.Text = savePath;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите таблицу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(savePath))
            {
                MessageBox.Show("Выберите путь сохранения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string tableName = comboBox1.Text;

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{tableName}`", con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    using (StreamWriter sw = new StreamWriter(savePath, false, Encoding.UTF8))
                    {
                        // Заголовки столбцов
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            sw.Write(reader.GetName(i));
                            if (i < reader.FieldCount - 1)
                                sw.Write(";");
                        }
                        sw.WriteLine();

                        // Данные
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string value = reader[i].ToString().Replace(";", ","); // чтобы CSV не ломался
                                sw.Write(value);

                                if (i < reader.FieldCount - 1)
                                    sw.Write(";");
                            }
                            sw.WriteLine();
                        }
                    }
                }

                MessageBox.Show("Экспорт успешно завершён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult result = MessageBox.Show("Экспорт успешно завершён!\nОткрыть файл?", "Успех",MessageBoxButtons.YesNo,MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = savePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                comboBox1.SelectedIndex = -1;
                savePath = null;
                label3.Text = "";
                label2.Text = "*путь не выбран";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
