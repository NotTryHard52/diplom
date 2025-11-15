using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Talon : Form
    {
        private string connectionString;
        private string currentUserFullName;
        private int currentUserId;         // id текущего пользователя
        private int selectedScheduleId;    // id выбранного расписания
        private int selectedPatientId;     // id выбранного пациента

        public Talon(string userFullName, int userId)
        {
            InitializeComponent();
            currentUserFullName = userFullName;
            currentUserId = userId;
        }

        private void Talon_Load(object sender, EventArgs e)
        {
            label3.Text = "Пользователь: " + currentUserFullName;

            // Подключение к базе
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();

            // Загрузка услуг
            LoadServices();

            // Настройка dataGridView2 для выбранных услуг
            dataGridView2.Columns.Clear();
            dataGridView2.Columns.Add("ServiceName", "Наименование");
            dataGridView2.Columns.Add("Price", "Цена");
            dataGridView2.Columns.Add("CategoryName", "Категория");
            dataGridView2.Columns.Add("ServiceId", "ServiceId");
            dataGridView2.Columns["ServiceId"].Visible = false;
            dataGridView2.AllowUserToAddRows = false;
        }

        private void LoadServices()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                DataTable t = new DataTable();
                MySqlCommand cmd = new MySqlCommand(@"
                    SELECT s.idServices AS ServiceId,
                           s.Name AS ServiceName,
                           s.Price,
                           c.Name AS CategoryName
                    FROM Services s
                    JOIN Category c ON s.Category = c.idCategory;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);

                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;
                dataGridView1.Columns["ServiceId"].Visible = false;
                dataGridView1.Columns["ServiceName"].HeaderText = "Наименование";
                dataGridView1.Columns["Price"].HeaderText = "Цена";
                dataGridView1.Columns["CategoryName"].HeaderText = "Категория";
            }
        }

        // Выбор расписания
        private void button4_Click(object sender, EventArgs e)
        {
            Schedule scheduleForm = new Schedule(true);
            scheduleForm.ScheduleSelected += (scheduleId, doctorName, date, time) =>
            {
                selectedScheduleId = scheduleId;
                label1.Text = "Врач: " + doctorName;
                label7.Text = "Дата приема: " + date;
                label8.Text = "Время приема: " + time;
            };
            scheduleForm.ShowDialog();
        }

        // Выбор пациента
        private void button5_Click(object sender, EventArgs e)
        {
            Patient patientForm = new Patient(true);
            patientForm.PatientSelected += (patientId, fullName) =>
            {
                selectedPatientId = patientId;
                label9.Text = "Пациент: " + fullName;
            };
            patientForm.ShowDialog();
        }

        // Добавление услуги в талон
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите услугу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            string serviceName = selectedRow.Cells["ServiceName"].Value.ToString();
            string price = selectedRow.Cells["Price"].Value.ToString();
            string category = selectedRow.Cells["CategoryName"].Value.ToString();
            int serviceId = Convert.ToInt32(selectedRow.Cells["ServiceId"].Value);

            // Проверка на дубликат
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (Convert.ToInt32(row.Cells["ServiceId"].Value) == serviceId)
                {
                    MessageBox.Show("Эта услуга уже добавлена в талон!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            dataGridView2.Rows.Add(serviceName, price, category, serviceId);
            UpdateTotal();
        }

        // Подсчёт суммы
        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (decimal.TryParse(row.Cells["Price"].Value.ToString(), out decimal price))
                    total += price;
            }
            label6.Text = $"Итого: {total} руб.";
        }

        // Сохранение талона в базу
        private void button7_Click(object sender, EventArgs e)
        {
            if (selectedPatientId == 0 || selectedScheduleId == 0 || dataGridView2.Rows.Count == 0)
            {
                MessageBox.Show("Выберите пациента, расписание и добавьте хотя бы одну услугу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal total = 0;
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                total += Convert.ToDecimal(row.Cells["Price"].Value);
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                using (MySqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        int orderId;

                        // Вставляем запись в Order
                        string insertOrder = @"
                    INSERT INTO `Order` (sum, schedule, Patients_idPatients, Status, User)
                    VALUES (@sum, @schedule, @patientId, @status, @userId);
                    SELECT LAST_INSERT_ID();";

                        using (MySqlCommand cmd = new MySqlCommand(insertOrder, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@sum", total);
                            cmd.Parameters.AddWithValue("@schedule", selectedScheduleId);
                            cmd.Parameters.AddWithValue("@patientId", selectedPatientId);
                            cmd.Parameters.AddWithValue("@status", 3); // 3 = Создан
                            cmd.Parameters.AddWithValue("@userId", currentUserId);

                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Вставляем услуги в OrderServices
                        string insertService = @"
                    INSERT INTO OrderServices (OrderId, ServicesId)
                    VALUES (@orderId, @serviceId)";
                        foreach (DataGridViewRow row in dataGridView2.Rows)
                        {
                            int serviceId = Convert.ToInt32(row.Cells["ServiceId"].Value);
                            using (MySqlCommand cmd = new MySqlCommand(insertService, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@orderId", orderId);
                                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Обновляем статус расписания на "занято" (например, Status = 2)
                        string updateSchedule = "UPDATE Schedule SET Status = 2 WHERE idSchedule = @scheduleId";
                        using (MySqlCommand cmd = new MySqlCommand(updateSchedule, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@scheduleId", selectedScheduleId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Талон успешно оформлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Очистка формы
                        dataGridView2.Rows.Clear();
                        label6.Text = "Итого: 0 руб.";
                        label1.Text = "Врач: ";
                        label7.Text = "Дата приема: ";
                        label8.Text = "Время приема: ";
                        label9.Text = "Пациент: ";

                        selectedPatientId = selectedScheduleId = 0;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 0)
            {
                MessageBox.Show("Нет услуг для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView2.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите услугу для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранную услугу из талона?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dataGridView2.SelectedRows)
                {
                    dataGridView2.Rows.Remove(row);
                }

                UpdateTotal();
            }
        }
    }
}
