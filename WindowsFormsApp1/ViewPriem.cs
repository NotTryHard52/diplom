using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace WindowsFormsApp1
{
    public partial class ViewPriem : Form
    {
        private int orderId;
        string connectionString;
        decimal totalSum = 0;
        private bool isGlav;

        public ViewPriem(int orderId, bool isGlav = false)
        {
            InitializeComponent();
            this.orderId = orderId;
            this.isGlav = isGlav;
        }

        private void ViewPriem_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            LoadOrderData();
            LoadServices();
            LoadStatuses();
            CalculateTotal();
            if (isGlav) { DisableForGlav(); }
            if (comboBox1.Text == "Завершен" || comboBox1.Text == "Отменен")
            {
                DisableEditing();
            }
            if (comboBox1.Text == "Создан" || comboBox1.Text == "Отменен")
            {
                button5.Enabled = false;
            }
            var hoverEffect = new HoverDataGridView(dataGridView1);
        }
        private void DisableForGlav()
        {
            button1.Visible = false; 
            button2.Visible = false; 
            button4.Visible = false; 
            button6.Visible = false;
            button5.Visible = false; 

            comboBox1.Enabled = false;
            dataGridView1.Enabled = false;
        }
        private void DisableEditing()
        {
            dataGridView1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button4.Enabled = false;
            button6.Enabled = false;
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
                            o.sum,
                            o.Discount,
                            o.TotalSum,
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
                        decimal sum = reader.GetDecimal("sum");
                        decimal discount = reader.GetDecimal("Discount");
                        decimal total = reader.GetDecimal("TotalSum");

                        label_total.Text = $"Итого: {total:N2} руб.";
                        label5.Text = $"Скидка: {discount:N2} руб.";
                        label10.Text = $"К оплате: {total:N2} руб.";
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
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Нельзя закрыть приём без оказанных услуг!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
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
                button4.Enabled = false;
                button5.Enabled = true;
                button6.Enabled = false;

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
            Services servicesForm = new Services(true);
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
                UpdateOrderTotalsInDatabase();
            }
        }

        private void RemoveSelectedService(int orderId)
        {
            if (dataGridView1.SelectedRows.Count == 0)
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

                string serviceName = dataGridView1.SelectedRows[0].Cells["Услуга"].Value.ToString();

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

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

                LoadServices();
                CalculateTotal();
                UpdateOrderTotalsInDatabase();

                MessageBox.Show("Услуга удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void UpdateOrderTotalsInDatabase()
        {
            decimal sum = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Цена"]?.Value != null && decimal.TryParse(row.Cells["Цена"].Value.ToString(), out decimal value))
                    sum += value;
            }

            decimal discount = 0;
            if (sum > 1000) discount = sum * 0.05m;

            decimal total = sum - discount;

            label_total.Text = $"Итого: {total:N2} руб.";
            label5.Text = $"Скидка: {discount:N2} руб.";
            label10.Text = $"К оплате: {total:N2} руб.";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string updateQuery = @"
            UPDATE `Order` 
            SET sum=@sum, Discount=@discount, TotalSum=@total 
            WHERE idOrder=@orderId";
                using (MySqlCommand cmd = new MySqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@sum", sum);
                    cmd.Parameters.AddWithValue("@discount", discount);
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Нет услуг для печати чека!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Word.Application wordApp = new Word.Application();
                Word.Document doc = wordApp.Documents.Add();

                doc.PageSetup.PageWidth = wordApp.CentimetersToPoints(8);  
                doc.PageSetup.PageHeight = wordApp.CentimetersToPoints(15); 
                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(0.5f);
                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(0.5f);
                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(0.5f);
                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(0.5f);

                string tempLogoPath = Path.Combine(Path.GetTempPath(), "temp_logo.png");
                Properties.Resources.logo.Save(tempLogoPath, ImageFormat.Png);

                Word.Range r = doc.Content;
                r.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                Word.Paragraph logoParagraph = doc.Paragraphs.Add(r);
                var shape = logoParagraph.Range.InlineShapes.AddPicture(tempLogoPath, false, true);
                shape.Width = 60;
                shape.Height = 60;
                logoParagraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                logoParagraph.SpaceAfter = 6;

                r = doc.Content;
                r.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                Word.Paragraph title = doc.Paragraphs.Add(r);
                title.Range.Text = "ЧЕК ПРИЁМА";
                title.Range.Font.Size = 14;
                title.Range.Font.Bold = 1;
                title.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                title.SpaceAfter = 6;

                string orderNumber = label_number.Text.Replace("Номер талона: ", "");
                string patientName = label_patient.Text.Replace("Пациент: ", "");
                string doctorName = label_doctor.Text.Replace("Врач: ", "");
                string date = label_data.Text.Replace("Дата: ", "");
                string time = label_time.Text.Replace("Время: ", "");

                r = doc.Content;
                r.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                Word.Paragraph info = doc.Paragraphs.Add(r);
                info.Range.Text =
                    $"Талон №: {orderNumber}\n" +
                    $"Пациент: {patientName}\n" +
                    $"Врач: {doctorName}\n" +
                    $"Дата: {date}  Время: {time}";
                info.Range.Font.Size = 10;
                info.SpaceAfter = 4;

                r = doc.Content;
                r.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                int rows = dataGridView1.Rows.Count + 1;
                Word.Table table = doc.Tables.Add(r, rows, 2);
                table.Borders.Enable = 1;

                table.Columns[1].Width = wordApp.CentimetersToPoints(5);
                table.Columns[2].Width = wordApp.CentimetersToPoints(2);

                table.Cell(1, 1).Range.Text = "Услуга";
                table.Cell(1, 2).Range.Text = "Цена";
                table.Rows[1].Range.Font.Bold = 1;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text =
                        dataGridView1.Rows[i].Cells["Услуга"].Value.ToString();

                    table.Cell(i + 2, 2).Range.Text =
                        dataGridView1.Rows[i].Cells["Цена"].Value.ToString();
                }

                table.Range.ParagraphFormat.SpaceAfter = 6;

                r = doc.Content;
                r.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                Word.Paragraph totals = doc.Paragraphs.Add(r);
                totals.Range.Text =
                    $"{label_total.Text}\n" +
                    $"{label5.Text}\n" +
                    $"{label10.Text}";
                totals.Range.Font.Size = 12;
                totals.Range.Font.Bold = 1;
                totals.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                wordApp.Visible = true;

                MessageBox.Show("Чек подготовлен для печати!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (File.Exists(tempLogoPath))
                    File.Delete(tempLogoPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании чека: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string statusQuery = "SELECT idStatusesPriem FROM StatusesPriem WHERE name='Отменен' LIMIT 1;";
                int statusId = Convert.ToInt32(new MySqlCommand(statusQuery, con).ExecuteScalar());

                string updateQuery = "UPDATE `Order` SET Status=@status WHERE idOrder=@orderId;";
                using (MySqlCommand cmd = new MySqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@status", statusId);
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                comboBox1.Text = "Отменен";

                dataGridView1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button4.Enabled = false;
                button6.Enabled = false;

                MessageBox.Show("Приём отменен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
