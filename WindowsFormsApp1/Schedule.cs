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
    public partial class Schedule : Form
    {
        string connectionString;
        DataTable scheduleTable;
        public Schedule()
        {
            InitializeComponent();
        }

        private void Schedule_Load(object sender, EventArgs e)
        {
            FillStatuses();
            int count = CountData.GetTableCount("Schedule");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            label2.Text = $"Количество записей: {count}";
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                scheduleTable = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT s.idSchedule, CONCAT(d.Surname, ' ', d.Name, ' ', d.Lastname) AS Врач, s.date AS `Дата приема`, s.time AS `Время приема`, st.statusname AS `Статус` FROM Schedule s JOIN Doctors d ON s.idDoctor = d.idDoctors JOIN Statuses st ON s.Status = st.idStatuses;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(scheduleTable);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = scheduleTable;
                dataGridView1.Columns[0].Visible = false;
            }
        }
        private void FillStatuses()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT DISTINCT statusname FROM Statuses;";
                MySqlCommand cmd = new MySqlCommand(query, con);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    comboBox2.Items.Clear();
                    comboBox2.Items.Add("Все");
                    while (reader.Read())
                    {
                        string statuses = reader["statusname"].ToString();
                        comboBox2.Items.Add(statuses);
                    }
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }
        private void ApplyFilterAndSort()
        {
            if (scheduleTable == null) return;

            string filterExpr = "";

            string selectedSpecialty = comboBox2.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedSpecialty) && selectedSpecialty != "Все")
            {
                filterExpr = $"Статус = '{selectedSpecialty.Replace("'", "''")}'";
            }

            DataView dv = scheduleTable.DefaultView;
            dv.RowFilter = filterExpr;

            dataGridView1.DataSource = dv;
            dataGridView1.Refresh();
        }
    }
}
