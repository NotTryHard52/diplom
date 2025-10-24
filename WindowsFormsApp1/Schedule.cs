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
        public event Action<int, string, string, string> ScheduleSelected;
        public Schedule()
        {
            InitializeComponent();
        }

        private void Schedule_Load(object sender, EventArgs e)
        {
            FillStatuses();
            LoadSchedule();
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
        }
        private void LoadSchedule()
        {
            InputLimit.DateOrder(dateTimePicker1);
            int count = CountData.GetTableCount("Schedule");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
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
                label2.Text = $"Количество записей: {scheduleTable.Rows.Count}";

                string docQuery = "SELECT idDoctors, CONCAT(Surname, ' ', Name, ' ', Lastname) AS DoctorName FROM Doctors;";
                MySqlCommand docCmd = new MySqlCommand(docQuery, con);
                MySqlDataAdapter docDa = new MySqlDataAdapter(docCmd);
                DataTable docTable = new DataTable();
                docDa.Fill(docTable);

                comboBox1.DisplayMember = "DoctorName";
                comboBox1.ValueMember = "idDoctors";
                comboBox1.DataSource = docTable;
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите врача!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int doctorId = Convert.ToInt32(comboBox1.SelectedValue);
            DateTime date = dateTimePicker1.Value.Date;
            TimeSpan time = dateTimePicker2.Value.TimeOfDay;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = @"
            SELECT COUNT(*) FROM Schedule
            WHERE idDoctor = @idDoctor AND date = @date AND time = @time";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@idDoctor", doctorId);
                    checkCmd.Parameters.AddWithValue("@date", date);
                    checkCmd.Parameters.AddWithValue("@time", time);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("У этого врача уже есть запись на указанное время!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                int statusId = 0;
                string getStatusQuery = "SELECT idStatuses FROM Statuses WHERE statusname = 'Свободно' LIMIT 1;";
                using (MySqlCommand statusCmd = new MySqlCommand(getStatusQuery, con))
                {
                    object result = statusCmd.ExecuteScalar();
                    if (result == null)
                    {
                        MessageBox.Show("Не найден статус 'Свободно' в таблице Statuses!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    statusId = Convert.ToInt32(result);
                }

                string insertQuery = @"
            INSERT INTO Schedule (idDoctor, date, time, Status)
            VALUES (@idDoctor, @date, @time, @status)";
                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@idDoctor", doctorId);
                    insertCmd.Parameters.AddWithValue("@date", date);
                    insertCmd.Parameters.AddWithValue("@time", time);
                    insertCmd.Parameters.AddWithValue("@status", statusId);
                    insertCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            LoadSchedule();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker1.Value.Date == DateTime.Now.Date)
            {
                DateTime selectedDateTime = dateTimePicker1.Value.Date + dateTimePicker2.Value.TimeOfDay;
                if (selectedDateTime < DateTime.Now)
                {
                    MessageBox.Show("Нельзя выбрать время в прошлом!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dateTimePicker2.Value = DateTime.Now;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dataGridView1.SelectedRows[0];

            int scheduleId = Convert.ToInt32(row.Cells["idSchedule"].Value);
            string doctorName = row.Cells["Врач"].Value.ToString();
            string date = row.Cells["Дата приема"].Value.ToString();
            string time = row.Cells["Время приема"].Value.ToString();
            string status = row.Cells["Статус"].Value.ToString();

            if (status != "Свободно")
            {
                MessageBox.Show("Выбранный слот уже занят!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Вызываем событие
            ScheduleSelected?.Invoke(scheduleId, doctorName, date, time);

            this.Close();
        }
    }
}
