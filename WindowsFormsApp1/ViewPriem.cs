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
    public partial class ViewPriem : Form
    {
        private int orderId;
        string connectionString;
        decimal totalSum = 0;
        decimal discount = 0;
        decimal finalSum = 0;

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
                        s.name AS 'Услуга',
                        os.count AS 'Количество',
                        s.`Base price` AS 'Цена за единицу',
                        (os.count * s.`Base price`) AS 'Сумма'
                    FROM orderservices AS os
                    INNER JOIN services AS s ON os.servicesId = s.idServices
                    WHERE os.orderid = @orderId;
                ";

                using (MySqlCommand cmd2 = new MySqlCommand(serviceQuery, con))
                {
                    cmd2.Parameters.AddWithValue("@orderId", orderId);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd2);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
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

            discount = totalSum > 1000 ? totalSum * 0.05m : 0;
            finalSum = totalSum - discount;

            label_total.Text = $"Общая сумма: {totalSum:F2} ₽";
            label_discount.Text = $"Скидка: {discount:F2} ₽";
            label_final.Text = $"Итог к оплате: {finalSum:F2} ₽";
        }

        // 🔄 Пересчет при редактировании количества в DataGrid
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Количество")
            {
                var row = dataGridView1.Rows[e.RowIndex];
                if (decimal.TryParse(row.Cells["Количество"].Value?.ToString(), out decimal count) &&
                    decimal.TryParse(row.Cells["Цена за единицу"].Value?.ToString(), out decimal price))
                {
                    row.Cells["Сумма"].Value = count * price;
                }
                CalculateTotal();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
