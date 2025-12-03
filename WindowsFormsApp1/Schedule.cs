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
        int selectedScheduleId = -1;
        public event Action<int, string, string, string> ScheduleSelected;
        string status;
        private bool openedFromTalon = false;
        private HashSet<int> collapsedGroups = new HashSet<int>();
        public Schedule(bool fromTalon = false)
        {
            InitializeComponent();
            openedFromTalon = fromTalon;

            button4.Visible = openedFromTalon;
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
                BuildGroupedGrid();
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns["Дата приема"].DefaultCellStyle.Format = "dd.MM.yyyy";
                label2.Text = $"Количество записей: {scheduleTable.Rows.Count}";

                string docQuery = "SELECT idDoctors, CONCAT(Surname, ' ', Name, ' ', Lastname) AS DoctorName FROM Doctors;";
                MySqlCommand docCmd = new MySqlCommand(docQuery, con);
                MySqlDataAdapter docDa = new MySqlDataAdapter(docCmd);
                DataTable docTable = new DataTable();
                docDa.Fill(docTable);

                comboBox1.DisplayMember = "DoctorName";
                comboBox1.ValueMember = "idDoctors";
                comboBox1.DataSource = docTable;
                comboBox1.SelectedIndex = -1;
            }
        }
        private void BuildGroupedGrid(DataView dv = null)
        {
            if (scheduleTable == null) return;

            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            foreach (DataColumn col in scheduleTable.Columns)
                dataGridView1.Columns.Add(col.ColumnName, col.ColumnName);

            dataGridView1.Columns["idSchedule"].Visible = false;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            string currentDoctor = "";
            int groupRowIndex = -1;

            if (dv == null) dv = scheduleTable.DefaultView;

            foreach (DataRowView drv in dv)
            {
                string doctor = drv["Врач"].ToString();

                if (doctor != currentDoctor)
                {
                    currentDoctor = doctor;
                    groupRowIndex = dataGridView1.Rows.Add("", doctor, "", "", "");
                    var groupRow = dataGridView1.Rows[groupRowIndex];

                    groupRow.DefaultCellStyle.BackColor = Color.LightGray;
                    groupRow.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                    groupRow.Tag = "GROUP";
                    bool collapsed = collapsedGroups.Contains(groupRowIndex);
                    groupRow.Cells[1].Value = (collapsed ? "► " : "▼ ") + doctor;
                }

                int rowIndex = dataGridView1.Rows.Add(
                    drv["idSchedule"],
                    "   " + doctor,
                    drv["Дата приема"],
                    drv["Время приема"],
                    drv["Статус"]
                );

                var normalRow = dataGridView1.Rows[rowIndex];

                if (collapsedGroups.Contains(groupRowIndex))
                    normalRow.Visible = false;

                if (drv["Статус"].ToString() == "Свободно")
                    normalRow.DefaultCellStyle.BackColor = Color.LightGreen;
                else
                    normalRow.DefaultCellStyle.BackColor = Color.LightCoral;
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

            string selectedStatus = comboBox2.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Все")
            {
                filterExpr = $"Статус = '{selectedStatus.Replace("'", "''")}'";
            }

            DataView dv = new DataView(scheduleTable);
            dv.RowFilter = filterExpr;

            BuildGroupedGrid(dv);
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

            if (!DateTime.TryParse(row.Cells["Дата приема"].Value.ToString(), out DateTime date))
            {
                MessageBox.Show("Неверный формат даты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!TimeSpan.TryParse(row.Cells["Время приема"].Value.ToString(), out TimeSpan time))
            {
                MessageBox.Show("Неверный формат времени!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime appointmentDateTime = date.Date + time;

            if (appointmentDateTime < DateTime.Now)
            {
                MessageBox.Show("Нельзя выбрать прошлое время!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string status = row.Cells["Статус"].Value.ToString();

            if (status != "Свободно")
            {
                MessageBox.Show("Выбранный слот уже занят!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ScheduleSelected?.Invoke(scheduleId, doctorName, date.ToString("yyyy-MM-dd"), time.ToString());

            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (status != "Свободно")
            {
                MessageBox.Show("Редактирование невозможно — расписание уже занято!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                selectedScheduleId = -1;
                return;
            }

            if (selectedScheduleId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                    WHERE idDoctor = @idDoctor AND date = @date AND time = @time 
                          AND idSchedule <> @idSchedule";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@idDoctor", doctorId);
                    checkCmd.Parameters.AddWithValue("@date", date);
                    checkCmd.Parameters.AddWithValue("@time", time);
                    checkCmd.Parameters.AddWithValue("@idSchedule", selectedScheduleId);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("У этого врача уже есть запись на указанное время!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string updateQuery = @"
                    UPDATE Schedule
                    SET idDoctor = @doctorId, date = @date, time = @time
                    WHERE idSchedule = @idSchedule";
                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, con))
                {
                    updateCmd.Parameters.AddWithValue("@doctorId", doctorId);
                    updateCmd.Parameters.AddWithValue("@date", date);
                    updateCmd.Parameters.AddWithValue("@time", time);
                    updateCmd.Parameters.AddWithValue("@idSchedule", selectedScheduleId);
                    updateCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            selectedScheduleId = -1;
            LoadSchedule();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow clickedRow = dataGridView1.Rows[e.RowIndex];

            if (clickedRow.Tag != null && clickedRow.Tag.ToString() == "GROUP")
            {
                bool collapsed;

                if (clickedRow.Cells[1].Value.ToString().StartsWith("▼ "))
                {
                    collapsed = true;
                    clickedRow.Cells[1].Value = "► " + clickedRow.Cells[1].Value.ToString().Substring(2);
                }
                else
                {
                    collapsed = false;
                    clickedRow.Cells[1].Value = "▼ " + clickedRow.Cells[1].Value.ToString().Substring(2);
                }

                if (collapsed)
                    collapsedGroups.Add(e.RowIndex);
                else
                    collapsedGroups.Remove(e.RowIndex);

                for (int i = e.RowIndex + 1; i < dataGridView1.Rows.Count; i++)
                {
                    var r = dataGridView1.Rows[i];
                    if (r.Tag != null && r.Tag.ToString() == "GROUP") break;
                    r.Visible = !collapsed;
                }

                return; 
            }

            if (!clickedRow.Visible)
                return;


            selectedScheduleId = Convert.ToInt32(clickedRow.Cells["idSchedule"].Value);

            string doctorFullName = clickedRow.Cells["Врач"].Value.ToString();
            status = clickedRow.Cells["Статус"].Value.ToString();

            comboBox1.SelectedIndex = comboBox1.FindStringExact(doctorFullName);

            if (DateTime.TryParse(clickedRow.Cells["Дата приема"].Value.ToString(), out DateTime dateFromDB))
            {
                dateTimePicker1.Value = dateFromDB < DateTime.Today ? DateTime.Today : dateFromDB;
            }

            if (TimeSpan.TryParse(clickedRow.Cells["Время приема"].Value.ToString(), out TimeSpan timeFromDB))
            {
                DateTime combinedDateTime = dateTimePicker1.Value.Date + timeFromDB;

                if (combinedDateTime < DateTime.Now)
                    dateTimePicker2.Value = DateTime.Now;
                else
                    dateTimePicker2.Value = DateTime.Today.Add(timeFromDB);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedScheduleId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (status != "Свободно")
            {
                MessageBox.Show("Нельзя удалить занятую запись расписания!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись расписания?","Подтверждение удаления",MessageBoxButtons.YesNo,MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string deleteQuery = "DELETE FROM Schedule WHERE idSchedule = @id";
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@id", selectedScheduleId);
                    deleteCmd.ExecuteNonQuery();
                    MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            selectedScheduleId = -1;
            LoadSchedule();
        }
    }
}
