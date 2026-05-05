using MySql.Data.MySqlClient; // Подключение MySQL клиента
using System;
using System.Data;
using System.Drawing.Imaging; // Для сохранения логотипа
using System.IO; // Работа с файлами
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word; // Используем Word Interop для печати чека

namespace WindowsFormsApp1
{
    public partial class ViewPriem : Form
    {
        private int orderId; // ID текущего приёма
        string connectionString; // Строка подключения к БД
        decimal totalSum = 0; // Сумма всех услуг
        private bool isGlav; // Флаг, если пользователь главный (ограничения на редактирование)

        // Конструктор формы
        public ViewPriem(int orderId, bool isGlav = false)
        {
            InitializeComponent();
            this.orderId = orderId;
            this.isGlav = isGlav;
        }

        // Событие загрузки формы
        private void ViewPriem_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB(); // Получаем строку подключения
            LoadOrderData(); // Загружаем данные о приёме
            LoadServices();  // Загружаем услуги в таблицу
            LoadStatuses();  // Загружаем статусы в comboBox
            CalculateTotal(); // Вычисляем итоговую сумму

            if (isGlav) { DisableForGlav(); } // Блокировка для главного пользователя

            // Блокировка элементов при определённых статусах
            if (comboBox1.Text == "Завершен" || comboBox1.Text == "Отменен")
            {
                DisableEditing();
            }
            if (comboBox1.Text == "Создан" || comboBox1.Text == "Отменен")
            {
                button5.Enabled = false; // Печать чека недоступна
            }

            var hoverEffect = new HoverDataGridView(dataGridView1); // Эффект при наведении на DataGridView
        }

        // Блокировка элементов для главного пользователя
        private void DisableForGlav()
        {
            button1.Visible = false; // Добавить услугу
            button2.Visible = false; // Удалить услугу
            button4.Visible = false; // Завершить приём
            button6.Visible = false; // Отменить приём
            button5.Visible = false; // Печать чека
            comboBox1.Enabled = false;
            dataGridView1.Enabled = false;
        }

        // Полная блокировка редактирования
        private void DisableEditing()
        {
            dataGridView1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button4.Enabled = false;
            button6.Enabled = false;
            comboBox1.Enabled = false;
        }

        // Загрузка данных о приёме из БД
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
                            DATE_FORMAT(sc.time, '%H:%i') AS time,
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
                            // Заполнение меток формы данными из БД
                            label_number.Text = "Номер талона: " + reader["idOrder"].ToString();
                            label_patient.Text = "Пациент: " + reader["patient_name"].ToString();
                            label_doctor.Text = "Врач: " + reader["doctor_name"].ToString();
                            label_data.Text = "Дата: " + reader["date"].ToString();
                            label_time.Text = "Время: " + reader["time"].ToString();
                            comboBox1.Text = reader["status"].ToString();
                        }

                        // Сумма, скидка и итог к оплате
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

        // Загрузка услуг из БД в DataGridView
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
                    dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выбор полной строки
                    dataGridView1.Columns["Услуга"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dataGridView1.Columns["Цена"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
        }

        // Загрузка всех статусов приёма в comboBox
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

        // Вычисление общей суммы услуг
        private void CalculateTotal()
        {
            totalSum = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Цена"]?.Value != null && decimal.TryParse(row.Cells["Цена"].Value.ToString(), out decimal value))
                    totalSum += value;
            }
            label_total.Text = $"Итого: {totalSum:F2} руб.";
        }
        
        // Завершение приёма
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

                // Блокировка элементов формы
                dataGridView1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = true; // Разрешена печать чека
                button6.Enabled = false;

                MessageBox.Show("Приём завершён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Кнопка "Добавить услугу"
        private void button1_Click(object sender, EventArgs e)
        {
            AddServiceToOrder(orderId);
            LoadServices();
            CalculateTotal();
        }

        // Кнопка "Удалить услугу"
        private void button2_Click(object sender, EventArgs e)
        {
            RemoveSelectedService(orderId);
            LoadServices();
            CalculateTotal();
        }

        // Метод добавления услуги к приёму
        private void AddServiceToOrder(int orderId)
        {
            Services servicesForm = new Services(true);
            if (servicesForm.ShowDialog() == DialogResult.OK)
            {
                int serviceId = servicesForm.SelectedServiceId;
                string serviceName = servicesForm.SelectedServiceName;
                decimal servicePrice = servicesForm.SelectedServicePrice;

                DataTable dt = (DataTable)dataGridView1.DataSource;

                // Проверка на дублирование услуги
                foreach (DataRow existingRow in dt.Rows)
                {
                    if (existingRow["Услуга"].ToString() == serviceName)
                    {
                        MessageBox.Show("Эта услуга уже добавлена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Добавление услуги в таблицу
                DataRow newRow = dt.NewRow();
                newRow["Услуга"] = serviceName;
                newRow["Цена"] = servicePrice;
                dt.Rows.Add(newRow);

                // Добавление услуги в базу
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
                UpdateOrderTotalsInDatabase(); // Обновляем общие суммы в БД
            }
        }

        // Метод удаления выбранной услуги
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

        // Закрытие формы
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Обновление итоговой суммы, скидки и оплаты в БД
        private void UpdateOrderTotalsInDatabase()
        {
            decimal sum = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Цена"]?.Value != null && decimal.TryParse(row.Cells["Цена"].Value.ToString(), out decimal value))
                    sum += value;
            }

            decimal discount = 0;
            if (sum > 1000) discount = sum * 0.05m; // Скидка 5% при сумме >1000
            decimal total = sum - discount;

            // Обновление меток
            label_total.Text = $"Итого: {total:N2} руб.";
            label5.Text = $"Скидка: {discount:N2} руб.";
            label10.Text = $"К оплате: {total:N2} руб.";

            // Обновление БД
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

        // Печать чека через Word
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

                // Настройка страницы для чековой ленты
                doc.PageSetup.PageWidth = wordApp.CentimetersToPoints(8);
                doc.PageSetup.PageHeight = wordApp.CentimetersToPoints(15);
                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(0.5f);
                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(0.5f);
                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(0.5f);
                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(0.5f);

                // Временный файл логотипа
                string tempLogoPath = Path.Combine(Path.GetTempPath(), "temp_logo.png");
                Properties.Resources.logo.Save(tempLogoPath, ImageFormat.Png);

                Word.Range r = doc.Content;
                r.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                // Вставка логотипа
                Word.Paragraph logoParagraph = doc.Paragraphs.Add(r);
                var shape = logoParagraph.Range.InlineShapes.AddPicture(tempLogoPath, false, true);
                shape.Width = 60;
                shape.Height = 60;
                logoParagraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                logoParagraph.SpaceAfter = 6;

                // Заголовок чека
                r = doc.Content;
                r.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                Word.Paragraph title = doc.Paragraphs.Add(r);
                title.Range.Text = "ЧЕК ПРИЁМА";
                title.Range.Font.Size = 14;
                title.Range.Font.Bold = 1;
                title.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                title.SpaceAfter = 6;

                // Информация о приёме
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

                // Таблица услуг
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

                // Итоги
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

                wordApp.Visible = true; // Показываем документ Word

                MessageBox.Show("Чек подготовлен для печати!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (File.Exists(tempLogoPath))
                    File.Delete(tempLogoPath); // Удаляем временный логотип
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании чека: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Отмена приёма
        private void button6_Click(object sender, EventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Получаем id статуса "Отменен" для Order
                int orderStatusId = Convert.ToInt32(new MySqlCommand(
                    "SELECT idStatusesPriem FROM StatusesPriem WHERE Name='Отменен' LIMIT 1;", con).ExecuteScalar());

                // Получаем ScheduleId для текущего Order
                MySqlCommand scheduleCmd = new MySqlCommand(
                    "SELECT Schedule FROM `Order` WHERE idOrder=@orderId LIMIT 1;", con);
                scheduleCmd.Parameters.AddWithValue("@orderId", orderId);
                int scheduleId = Convert.ToInt32(scheduleCmd.ExecuteScalar());

                // Получаем id статуса "Свободно" для Schedule
                int freeScheduleStatusId = Convert.ToInt32(new MySqlCommand(
                    "SELECT idStatuses FROM Statuses WHERE StatusName='Свободно' LIMIT 1;", con).ExecuteScalar());

                if (orderStatusId == 0 || scheduleId == 0 || freeScheduleStatusId == 0)
                {
                    MessageBox.Show("Ошибка: не найден нужный статус или расписание.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Обновление Schedule — перевод в статус "Свободно"
                MySqlCommand updateScheduleCmd = new MySqlCommand(
                    "UPDATE Schedule SET Status=@scheduleStatusId WHERE idSchedule=@scheduleId;", con);
                updateScheduleCmd.Parameters.AddWithValue("@scheduleStatusId", freeScheduleStatusId);
                updateScheduleCmd.Parameters.AddWithValue("@scheduleId", scheduleId);
                updateScheduleCmd.ExecuteNonQuery();

                // Обновление Order — перевод в статус "Отменен"
                MySqlCommand updateOrderCmd = new MySqlCommand(
                    "UPDATE `Order` SET Status=@orderStatusId WHERE idOrder=@orderId;", con);
                updateOrderCmd.Parameters.AddWithValue("@orderStatusId", orderStatusId);
                updateOrderCmd.Parameters.AddWithValue("@orderId", orderId);
                updateOrderCmd.ExecuteNonQuery();

                comboBox1.Text = "Отменен";

                // Блокировка элементов формы
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
