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

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // 1️⃣ Данные о талоне
                string orderInfoQuery = @"
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
                    WHERE o.idOrder = @orderId;
                ";

                using (MySqlCommand cmd = new MySqlCommand(orderInfoQuery, con))
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

                // 2️⃣ Загрузка услуг
                string serviceQuery = @"
                    SELECT 
                        s.Name AS 'Услуга',
                        s.`Base Price` AS 'Цена',
                        s.`Base Price` AS 'Сумма'
                    FROM OrderServices os
                    INNER JOIN Services s ON os.ServicesId = s.idServices
                    WHERE os.OrderId = @orderId;
                ";

                using (MySqlCommand cmd2 = new MySqlCommand(serviceQuery, con))
                {
                    cmd2.Parameters.AddWithValue("@orderId", orderId);
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd2).Fill(dt);
                    dataGridView1.DataSource = dt;
                    dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                }

                // 3️⃣ Загружаем статусы
                string statusQuery = "SELECT name FROM StatusesPriem;";
                using (MySqlCommand cmd3 = new MySqlCommand(statusQuery, con))
                using (MySqlDataReader reader = cmd3.ExecuteReader())
                {
                    comboBox1.Items.Clear();
                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader["name"].ToString());
                    }
                }

                // 4️⃣ Считаем итог
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            totalSum = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Сумма"]?.Value != null && decimal.TryParse(row.Cells["Сумма"].Value.ToString(), out decimal value))
                {
                    totalSum += value;
                }
            }

            label_total.Text = $"Итого: {totalSum:F2} ₽";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
