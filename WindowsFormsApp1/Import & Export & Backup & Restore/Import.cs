using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Import : Form
    {
        string connectionString;
        string csvPath;

        public Import()
        {
            InitializeComponent();
        }

        private void Import_Load(object sender, EventArgs e)
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
                comboBox1.DisplayMember = "Tables_in_dentistryDB";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите CSV файл";
                ofd.Filter = "CSV файл (*.csv)|*.csv";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    csvPath = ofd.FileName;

                    label2.Text = "*файл выбран";
                    label3.Text = csvPath;
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

            if (string.IsNullOrEmpty(csvPath))
            {
                MessageBox.Show("Выберите CSV файл!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string tableName = comboBox1.Text;

            try
            {
                var lines = System.IO.File.ReadAllLines(csvPath, Encoding.UTF8);

                if (lines.Length < 2)
                {
                    MessageBox.Show("CSV файл пустой!", "Ошибка");
                    return;
                }

                // Заголовки (первая строка)
                string[] headers = lines[0].Split(';');

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                            continue;

                        string[] values = lines[i].Split(';');

                        // Формируем запрос
                        string columns = string.Join(",", headers.Select(h => $"`{h}`"));
                        string parameters = string.Join(",", headers.Select((h, index) => $"@param{index}"));

                        string query = $"INSERT INTO `{tableName}` ({columns}) VALUES ({parameters})";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            for (int j = 0; j < headers.Length; j++)
                            {
                                string value = j < values.Length ? values[j] : null;
                                cmd.Parameters.AddWithValue($"@param{j}", value);
                            }

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Импорт успешно завершён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                comboBox1.SelectedIndex = -1;
                csvPath = null;
                label3.Text = "";
                label2.Text = "*файл не выбран";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
