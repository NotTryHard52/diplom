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
                MessageBox.Show("Выберите таблицу!");
                return;
            }

            if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
            {
                MessageBox.Show("Выберите CSV файл!");
                return;
            }

            string tableName = comboBox1.Text;

            try
            {
                var lines = File.ReadAllLines(csvPath, Encoding.UTF8);

                if (lines.Length < 2)
                {
                    MessageBox.Show("CSV пустой");
                    return;
                }

                string[] headers = lines[0].Split(';');

                List<int> validIndexes = new List<int>();
                List<string> dbColumns = new List<string>();

                for (int i = 0; i < headers.Length; i++)
                {
                    string col = headers[i].Trim();

                    if (col.Equals("idSchedule", StringComparison.OrdinalIgnoreCase))
                        continue;

                    validIndexes.Add(i);
                    dbColumns.Add($"`{col}`");
                }

                string columns = string.Join(",", dbColumns);
                string parameters = string.Join(",", Enumerable.Range(0, validIndexes.Count)
                    .Select(i => $"@p{i}"));

                string query = $"INSERT INTO `{tableName}` ({columns}) VALUES ({parameters})";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    using (MySqlTransaction tr = con.BeginTransaction())
                    {
                        int success = 0;

                        List<string> errors = new List<string>();

                        try
                        {
                            for (int i = 1; i < lines.Length; i++)
                            {
                                if (string.IsNullOrWhiteSpace(lines[i]))
                                    continue;

                                string[] values = lines[i].Split(';');

                                using (MySqlCommand cmd = new MySqlCommand(query, con, tr))
                                {
                                    cmd.CommandTimeout = 60;

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

                                        cmd.Parameters.AddWithValue($"@p{j}", raw);
                                    }

                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                        success++;
                                    }
                                    catch (MySqlException ex)
                                    {
                                        errors.Add($"Строка {i + 1}: {ex.Message} | {lines[i]}");
                                        throw; // ❗ важно: ломаем транзакцию
                                    }
                                }
                            }

                            tr.Commit();

                            MessageBox.Show(
                                $"Импорт успешен!\nЗаписей: {success}");
                        }
                        catch
                        {
                            tr.Rollback();

                            MessageBox.Show(
                                "Импорт ОТМЕНЁН (rollback)\n\n" +
                                string.Join("\n", errors.Take(10)),
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
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
                MessageBox.Show($"Ошибка импорта: {ex.Message}");
            }
        }
    }
}
