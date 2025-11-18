using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

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

            decimal discount = 0;

            if (total > 1000)
            {
                discount = total * 0.05m;
            }

            decimal finalTotal = total - discount;

            // Отображаем итоги
            label6.Text = $"Итого: {total:N2} руб.";
            label5.Text = $"Скидка: {discount:N2} руб.";
            label10.Text = $"К оплате: {finalTotal:N2} руб.";
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

            decimal discount = 0;
            if (total > 1000)
            {
                discount = total * 0.05m; // 5% скидка
            }

            decimal finalTotal = total - discount;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                using (MySqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        int orderId;

                        // Вставляем запись в Order с учетом Discount и TotalSum
                        string insertOrder = @"
                    INSERT INTO `Order` (sum, Discount, TotalSum, schedule, Patients_idPatients, Status, User)
                    VALUES (@sum, @discount, @totalSum, @schedule, @patientId, @status, @userId);
                    SELECT LAST_INSERT_ID();";

                        using (MySqlCommand cmd = new MySqlCommand(insertOrder, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@sum", total);
                            cmd.Parameters.AddWithValue("@discount", discount);
                            cmd.Parameters.AddWithValue("@totalSum", finalTotal);
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

                        // Обновляем статус расписания на "занято" (Status = 2)
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
                        label5.Text = "Скидка: 0 руб.";
                        label10.Text = "К оплате: 0 руб.";
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

        private void button6_Click(object sender, EventArgs e)
        {
            if (selectedPatientId == 0 || selectedScheduleId == 0)
            {
                MessageBox.Show("Невозможно распечатать. Выберите пациента и расписание.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Word.Application wordApp = new Word.Application();
                Word.Document doc = wordApp.Documents.Add();
                wordApp.Visible = true;

                // Заголовок
                Word.Paragraph para = doc.Content.Paragraphs.Add();
                para.Range.Text = "ПРИГЛАШЕНИЕ НА ПРИЁМ";
                para.Range.Font.Size = 18;
                para.Range.Font.Bold = 1;
                para.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.InsertParagraphAfter();

                // Форматируем дату и время
                string dateStr = DateTime.Parse(label7.Text.Replace("Дата приема: ", "")).ToString("dd.MM.yyyy");
                string timeStr = TimeSpan.Parse(label8.Text.Replace("Время приема: ", "")).ToString(@"hh\:mm");

                // Информация о пациенте и приёме
                Word.Paragraph infoPara = doc.Content.Paragraphs.Add();
                infoPara.Range.Text = $"Пациент: {label9.Text.Replace("Пациент: ", "")}\n" +
                                      $"Врач: {label1.Text.Replace("Врач: ", "")}\n" +
                                      $"Дата приёма: {dateStr}\n" +
                                      $"Время приёма: {timeStr}";
                infoPara.Range.Font.Size = 12;
                infoPara.Range.Font.Bold = 0;
                infoPara.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                infoPara.Range.InsertParagraphAfter();

                if (dataGridView2.Rows.Count > 0)
                {
                    int rows = dataGridView2.Rows.Count + 1; // +1 для заголовка
                    int cols = 2; // Наименование и Категория
                    Word.Range tableRange = doc.Content.Paragraphs.Add().Range;
                    Word.Table table = doc.Tables.Add(tableRange, rows, cols);
                    table.Borders.Enable = 1;

                    // Заголовки таблицы
                    table.Cell(1, 1).Range.Text = "Наименование услуги";
                    table.Cell(1, 2).Range.Text = "Категория";
                    table.Rows[1].Range.Font.Bold = 1;
                    table.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Заполнение таблицы
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        table.Cell(i + 2, 1).Range.Text = dataGridView2.Rows[i].Cells["ServiceName"].Value.ToString();
                        table.Cell(i + 2, 2).Range.Text = dataGridView2.Rows[i].Cells["CategoryName"].Value.ToString();

                        // Выравнивание текста в ячейках
                        table.Cell(i + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                        table.Cell(i + 2, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    }
                }

                MessageBox.Show("Талон подготовлен в Word.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании документа Word: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
