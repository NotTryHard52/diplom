using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class UchetTalona_GL : Form
    {
        string connectionString;
        DataTable orderTable;
        public UchetTalona_GL()
        {
            InitializeComponent();
        }

        private void UchetTalona_GL_Load(object sender, EventArgs e)
        {
            FillStatus();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            int count = CountData.GetTableCount("Order");
            label9.Text = $"Количество записей: {count}";
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                orderTable = new DataTable();
                string query = @"
                        SELECT 
                            o.idOrder AS 'Номер талона',
                            o.sum AS 'Сумма',
                            CONCAT(d.surname, ' ', d.name, ' ', d.lastname) AS 'Врач',
                            DATE_FORMAT(sc.date, '%d.%m.%Y') AS 'Дата',
                            sc.time AS 'Время',
                            CONCAT(r.surname, ' ', r.name, ' ', r.lastname) AS 'Регистратор',
                            CONCAT(p.surname, ' ', p.name, ' ', p.lastname) AS 'Пациент',
                            st.name AS 'Статус'
                        FROM `Order` o
                        JOIN Schedule sc ON o.Schedule = sc.idSchedule
                        JOIN Doctors d ON sc.idDoctor = d.idDoctors
                        JOIN `Users` r ON o.User = r.idUsers
                        JOIN Patients p ON o.Patients_idPatients = p.idPatients
                        JOIN StatusesPriem st ON o.Status = st.idStatusesPriem
                    ";
                MySqlCommand cmd = new MySqlCommand(query, con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(orderTable);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = orderTable;
            }
            UpdateRevenueSum();
        }
        private void UpdateRevenueSum()
        {
            if (dataGridView1.DataSource == null) return;

            decimal total = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Сумма"].Value == null || row.Cells["Сумма"].Value == DBNull.Value)
                    continue;

                string status = row.Cells["Статус"].Value?.ToString()?.ToLower();

                if (status != null && (status.Contains("отмен") || status.Contains("создан")))
                    continue;

                decimal value;
                if (decimal.TryParse(row.Cells["Сумма"].Value.ToString(), out value))
                {
                    total += value;
                }
            }

            label3.Text = $"Общая выручка: {total:N2} руб.";
        }
        private void FillStatus()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT DISTINCT Name FROM StatusesPriem;";
                MySqlCommand cmd = new MySqlCommand(query, con);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Все");
                    while (reader.Read())
                    {
                        string status = reader["name"].ToString();
                        comboBox1.Items.Add(status);
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }
        private void ApplyFilterAndSort()
        {
            if (orderTable == null) return;

            string filterExpr = "";

            string selectedSpecialty = comboBox1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedSpecialty) && selectedSpecialty != "Все")
            {
                filterExpr = $"Статус = '{selectedSpecialty.Replace("'", "''")}'";
            }

            string searchText = textBox5.Text.Trim().Replace("'", "''");
            if (!string.IsNullOrEmpty(searchText))
            {
                if (!string.IsNullOrEmpty(filterExpr))
                {
                    filterExpr += " AND ";
                }
                filterExpr += $"Convert([Номер талона], 'System.String') LIKE '%{searchText}%'";
            }

            string sortExpr = "";
            if (comboBox2.SelectedIndex == 1)
                sortExpr = "Дата ASC";
            else if (comboBox2.SelectedIndex == 2)
                sortExpr = "Дата DESC";

            DataView dv = orderTable.DefaultView;
            dv.RowFilter = filterExpr;
            dv.Sort = sortExpr;

            dataGridView1.DataSource = dv;
            dataGridView1.Refresh();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int orderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Номер талона"].Value);
                ViewPriem v = new ViewPriem(orderId);
                v.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите талон.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            textBox5.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OtchetExcel v = new OtchetExcel();
            v.ShowDialog();
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells["Статус"].Value == null)
                return;

            string status = dataGridView1.Rows[e.RowIndex].Cells["Статус"].Value.ToString().ToLower();

            if (status.Contains("заверш"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
            }
            else if (status.Contains("отмен"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
            }
            else if (status.Contains("создан"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
            }
            else
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
        }
    }
}
