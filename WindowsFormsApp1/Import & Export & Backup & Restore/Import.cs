using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
                comboBox1.DisplayMember = dt2.Columns[0].ColumnName;
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

                    if (!File.Exists(csvPath))
                    {
                        MessageBox.Show("Файл не найден!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    label2.Text = "*файл выбран";
                    label3.Text = csvPath;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите таблицу!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(csvPath))
            {
                MessageBox.Show("Выберите CSV файл!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(csvPath))
            {
                MessageBox.Show("Файл не найден!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string tableName = comboBox1.Text;

            try
            {
                var lines = File.ReadLines(csvPath, Encoding.UTF8).ToArray();

                if (lines.Length < 2)
                {
                    MessageBox.Show("CSV файл пустой!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] headers = lines[0].Split(';');

                List<int> validIndexes = new List<int>();
                List<string> dbColumns = new List<string>();

                for (int i = 0; i < headers.Length; i++)
                {
                    string col = headers[i].Trim();

                    if (col.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                        continue; // игнорируем id

                    validIndexes.Add(i);
                    dbColumns.Add($"`{col}`");
                }

                string columns = string.Join(",", dbColumns);
                string parameters = string.Join(",", Enumerable.Range(0, validIndexes.Count).Select(i => $"@p{i}"));

                string query = $"INSERT INTO `{tableName}` ({columns}) VALUES ({parameters})";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    using (MySqlTransaction transaction = con.BeginTransaction())
                    {
                        int success = 0;
                        int failed = 0;

                        try
                        {
                            for (int i = 1; i < lines.Length; i++)
                            {
                                if (string.IsNullOrWhiteSpace(lines[i]))
                                    continue;

                                string[] values = lines[i].Split(';');

                                try
                                {
                                    using (MySqlCommand cmd = new MySqlCommand(query, con, transaction))
                                    {
                                        for (int j = 0; j < validIndexes.Count; j++)
                                        {
                                            string raw = validIndexes[j] < values.Length
                                                ? values[validIndexes[j]]
                                                : null;

                                            if (string.IsNullOrWhiteSpace(raw))
                                            {
                                                cmd.Parameters.AddWithValue($"@p{j}", DBNull.Value);
                                                continue;
                                            }

                                            raw = raw.Trim().Replace(',', '.'); 

                                            if (int.TryParse(raw, System.Globalization.NumberStyles.Any,
                                                System.Globalization.CultureInfo.InvariantCulture, out int intVal))
                                            {
                                                cmd.Parameters.AddWithValue($"@p{j}", intVal);
                                            }
                                            else if (decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                                                System.Globalization.CultureInfo.InvariantCulture, out decimal decVal))
                                            {
                                                cmd.Parameters.AddWithValue($"@p{j}", decVal);
                                            }
                                            else
                                            {
                                                cmd.Parameters.AddWithValue($"@p{j}", raw);
                                            }
                                        }

                                        cmd.ExecuteNonQuery();
                                        success++;
                                    }
                                }
                                catch
                                {
                                    failed++;
                                    continue;
                                }
                            }

                            transaction.Commit();

                            MessageBox.Show(
                                $"Импорт завершён!\nУспешно: {success}\nОшибок: {failed}",
                                "Готово",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                comboBox1.SelectedIndex = -1;
                csvPath = null;
                label3.Text = "";
                label2.Text = "*файл не выбран";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
