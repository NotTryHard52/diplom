using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ViewPriem : Form
    {
        private int orderId;
        string connectionString;
        decimal totalSum = 0;

        public ViewPriem(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
        }

        private void ViewPriem_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            LoadOrderData();
            LoadServices();
            LoadStatuses();
            CalculateTotal();
            if (comboBox1.Text == "Завершен" || comboBox1.Text == "Отменен")
            {
                DisableEditing();
            }
        }
        private void DisableEditing()
        {
            dataGridView1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button4.Enabled = false;
            comboBox1.Enabled = false;
        }

        private void LoadOrderData()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT 
                        o.idOrder,
                        CONCAT(p.surname, ' ', p.name, ' ', p.lastname) AS patient_name,
                        CONCAT(d.surname, ' ', d.name, ' ', d.lastname) AS doctor_name,
                        DATE_FORMAT(sc.date, '%d.%m.%Y') AS date,
                        sc.time,
                        st.name AS status
                    FROM `Order` o
                    JOIN Schedule sc ON o.Schedule = sc.idSchedule
                    JOIN Doctors d ON sc.idDoctor = d.idDoctors
                    JOIN Patients p ON o.Patients_idPatients = p.idPatients
                    JOIN StatusesPriem st ON o.Status = st.idStatusesPriem
                    WHERE o.idOrder = @orderId;";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            label_number.Text = "Номер талона: " + reader["idOrder"].ToString();
                            label_patient.Text = "Пациент: " + reader["patient_name"].ToString();
                            label_doctor.Text = "Врач: " + reader["doctor_name"].ToString();
                            label_data.Text = "Дата: " + reader["date"].ToString();
                            label_time.Text = "Время: " + reader["time"].ToString();
                            comboBox1.Text = reader["status"].ToString();
                        }
                    }
                }
            }
        }

        private void LoadServices()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string serviceQuery = @"
                    SELECT s.Name AS 'Услуга', s.Price AS 'Цена'
                    FROM OrderServices os
                    INNER JOIN Services s ON os.ServicesId = s.idServices
                    WHERE os.OrderId = @orderId;";
                using (MySqlCommand cmd = new MySqlCommand(serviceQuery, con))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;
                    dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                }
            }
        }

        private void LoadStatuses()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string statusQuery = "SELECT name FROM StatusesPriem;";
                using (MySqlCommand cmd = new MySqlCommand(statusQuery, con))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    comboBox1.Items.Clear();
                    while (reader.Read())
                        comboBox1.Items.Add(reader["name"].ToString());
                }
            }
        }

        private void CalculateTotal()
        {
            totalSum = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Цена"]?.Value != null && decimal.TryParse(row.Cells["Цена"].Value.ToString(), out decimal value))
                    totalSum += value;
            }
            label_total.Text = $"Итого: {totalSum:F2} ₽";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string statusQuery = "SELECT idStatusesPriem FROM StatusesPriem WHERE name='Завершен' LIMIT 1;";
                int statusId = Convert.ToInt32(new MySqlCommand(statusQuery, con).ExecuteScalar());

                string updateQuery = "UPDATE `Order` SET Status=@status WHERE idOrder=@orderId;";
                using (MySqlCommand cmd = new MySqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@status", statusId);
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                comboBox1.Text = "Завершен";

                dataGridView1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;

                MessageBox.Show("Приём завершён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddServiceToOrder(orderId);
            LoadServices();
            CalculateTotal();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RemoveSelectedService(orderId);
            LoadServices();
            CalculateTotal();
        }

        private void AddServiceToOrder(int orderId)
        {
            Services servicesForm = new Services();
            if (servicesForm.ShowDialog() == DialogResult.OK)
            {
                int serviceId = servicesForm.SelectedServiceId;
                string serviceName = servicesForm.SelectedServiceName;
                decimal servicePrice = servicesForm.SelectedServicePrice;

                DataTable dt = (DataTable)dataGridView1.DataSource;

                foreach (DataRow existingRow in dt.Rows)
                {
                    if (existingRow["Услуга"].ToString() == serviceName)
                    {
                        MessageBox.Show("Эта услуга уже добавлена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                DataRow newRow = dt.NewRow();
                newRow["Услуга"] = serviceName;
                newRow["Цена"] = servicePrice;
                dt.Rows.Add(newRow);

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string insertQuery = "INSERT INTO OrderServices (OrderId, ServicesId) VALUES (@orderId, @serviceId)";
                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        cmd.Parameters.AddWithValue("@serviceId", serviceId);
                        cmd.ExecuteNonQuery();
                    }
                }

                CalculateTotal();
            }
        }

        private void RemoveSelectedService(int orderId)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите услугу для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string serviceName = dataGridView1.SelectedRows[0].Cells["Услуга"].Value.ToString();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Просто удаляем по названию
                string deleteQuery = @"
            DELETE FROM OrderServices 
            WHERE OrderId = @orderId 
              AND ServicesId = (SELECT idServices FROM Services WHERE Name = @serviceName LIMIT 1)
            LIMIT 1;";

                using (MySqlCommand cmd = new MySqlCommand(deleteQuery, con))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.Parameters.AddWithValue("@serviceName", serviceName);
                    cmd.ExecuteNonQuery();
                }
            }

            // Обновляем таблицу и сумму
            LoadServices();
            CalculateTotal();

            MessageBox.Show("Услуга удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
