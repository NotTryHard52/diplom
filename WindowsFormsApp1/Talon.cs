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
                           s.`Base Price` AS Price,
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
            Schedule scheduleForm = new Schedule();
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
            Patient patientForm = new Patient();
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
            if (dataGridView2.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну услугу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (selectedScheduleId == 0 || selectedPatientId == 0)
            {
                MessageBox.Show("Выберите расписание и пациента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // Сумма
                    decimal total = 0;
                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        total += Convert.ToDecimal(row.Cells["Price"].Value);
                    }

                    // Вставка в Order
                    string insertOrderQuery = @"
                        INSERT INTO `Order` (sum, schedule, user, Patients_idPatients, Status)
                        VALUES (@sum, @schedule, @user, @patient, 'Создан')";
                    MySqlCommand cmdOrder = new MySqlCommand(insertOrderQuery, con, transaction);
                    cmdOrder.Parameters.AddWithValue("@sum", total);
                    cmdOrder.Parameters.AddWithValue("@schedule", selectedScheduleId);
                    cmdOrder.Parameters.AddWithValue("@user", currentUserId);
                    cmdOrder.Parameters.AddWithValue("@patient", selectedPatientId);
                    cmdOrder.ExecuteNonQuery();

                    long orderId = cmdOrder.LastInsertedId;

                    // Вставка в OrderServices
                    string insertServiceQuery = "INSERT INTO OrderServices (OrderId, ServicesId) VALUES (@orderId, @serviceId)";
                    MySqlCommand cmdService = new MySqlCommand(insertServiceQuery, con, transaction);

                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        int serviceId = Convert.ToInt32(row.Cells["ServiceId"].Value);
                        cmdService.Parameters.Clear();
                        cmdService.Parameters.AddWithValue("@orderId", orderId);
                        cmdService.Parameters.AddWithValue("@serviceId", serviceId);
                        cmdService.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Талон успешно сохранён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Очистка формы
                    dataGridView2.Rows.Clear();
                    UpdateTotal();
                    label1.Text = "Врач:";
                    label7.Text = "Дата приема:";
                    label8.Text = "Время приема:";
                    label9.Text = "Пациент:";
                    selectedScheduleId = 0;
                    selectedPatientId = 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при сохранении талона:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
