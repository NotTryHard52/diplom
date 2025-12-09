using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace WindowsFormsApp1
{
    public partial class Talon : Form
    {
        // Строка подключения к базе данных
        private string connectionString;

        // Информация о текущем пользователе
        private string currentUserFullName;
        private int currentUserId;         // id текущего пользователя

        // Выбранные элементы формы
        private int selectedScheduleId;    // id выбранного расписания
        private int selectedPatientId;     // id выбранного пациента

        // Конструктор формы
        public Talon(string userFullName, int userId)
        {
            InitializeComponent();
            currentUserFullName = userFullName;  // Сохраняем имя текущего пользователя
            currentUserId = userId;              // Сохраняем id текущего пользователя
        }

        // Событие загрузки формы
        private void Talon_Load(object sender, EventArgs e)
        {
            // Отображение текущего пользователя
            label3.Text = "Пользователь: " + currentUserFullName;

            // Подключение к базе данных
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();

            // Загрузка услуг из базы данных
            LoadServices();

            // Настройка dataGridView2 для выбранных услуг
            dataGridView2.Columns.Clear();
            dataGridView2.Columns.Add("ServiceName", "Наименование");
            dataGridView2.Columns.Add("Price", "Цена");
            dataGridView2.Columns.Add("CategoryName", "Категория");
            dataGridView2.Columns.Add("ServiceId", "ServiceId"); // скрытый столбец для id услуги
            dataGridView2.Columns["ServiceId"].Visible = false;
            dataGridView2.AllowUserToAddRows = false;

            // Добавляем эффект подсветки при наведении на строки
            var hoverEffect = new HoverDataGridView(dataGridView1);
            var hoverEffect2 = new HoverDataGridView(dataGridView2);
        }

        // Загрузка услуг из базы данных в dataGridView1
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

                // Настройка DataGridView
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // выделение всей строки
                dataGridView1.DataSource = t; // привязка данных
                dataGridView1.Columns["ServiceId"].Visible = false; // скрываем id
                dataGridView1.Columns["ServiceName"].HeaderText = "Наименование";
                dataGridView1.Columns["Price"].HeaderText = "Цена";
                dataGridView1.Columns["CategoryName"].HeaderText = "Категория";
            }
        }

        // Выбор расписания приёмов
        private void button4_Click(object sender, EventArgs e)
        {
            Schedule scheduleForm = new Schedule(true); // передаём true для выбора
            scheduleForm.ScheduleSelected += (scheduleId, doctorName, date, time) =>
            {
                // Получаем данные выбранного расписания
                selectedScheduleId = scheduleId;
                label1.Text = "Врач: " + doctorName;
                label7.Text = "Дата приема: " + DateTime.Parse(date).ToString("dd.MM.yyyy");
                label8.Text = "Время приема: " + TimeSpan.Parse(time).ToString(@"hh\:mm");
            };
            scheduleForm.ShowDialog(); // показываем форму выбора
        }

        // Выбор пациента
        private void button5_Click(object sender, EventArgs e)
        {
            Patient patientForm = new Patient(true); // передаём true для выбора
            patientForm.PatientSelected += (patientId, fullName) =>
            {
                // Получаем данные выбранного пациента
                selectedPatientId = patientId;
                label9.Text = "Пациент: " + fullName;
            };
            patientForm.ShowDialog(); // показываем форму выбора
        }

        // Добавление услуги в талон
        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка, выбрана ли услуга
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

            // Добавляем услугу в список выбранных услуг
            dataGridView2.Rows.Add(serviceName, price, category, serviceId);
            UpdateTotal(); // пересчёт суммы
        }

        // Подсчёт суммы и скидки
        private void UpdateTotal()
        {
            decimal total = 0;

            // Суммируем цену всех выбранных услуг
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (decimal.TryParse(row.Cells["Price"].Value.ToString(), out decimal price))
                    total += price;
            }

            decimal discount = 0;

            // Применяем скидку 5% если сумма больше 1000
            if (total > 1000)
            {
                discount = total * 0.05m;
            }

            decimal finalTotal = total - discount;

            // Отображение итогов на форме
            label6.Text = $"Итого: {total:N2} руб.";
            label5.Text = $"Скидка: {discount:N2} руб.";
            label10.Text = $"К оплате: {finalTotal:N2} руб.";
        }

        // Сохранение талона в базу и возможность печати
        private void button7_Click(object sender, EventArgs e)
        {
            // Проверка выбора пациента и расписания
            if (selectedPatientId == 0 || selectedScheduleId == 0)
            {
                MessageBox.Show("Выберите пациента и расписание!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Расчёт общей суммы и скидки
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
                using (MySqlTransaction transaction = con.BeginTransaction()) // Используем транзакцию для целостности данных
                {
                    try
                    {
                        int orderId;

                        // Вставляем запись в Order
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

                        // Вставляем выбранные услуги в OrderServices
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

                        // Обновление статуса расписания
                        string updateSchedule = "UPDATE Schedule SET Status = 2 WHERE idSchedule = @scheduleId";
                        using (MySqlCommand cmd = new MySqlCommand(updateSchedule, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@scheduleId", selectedScheduleId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit(); // Подтверждаем транзакцию
                        MessageBox.Show("Талон успешно оформлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Возможность печати талона в Word
                        DialogResult printResult = MessageBox.Show(
                            "Хотите распечатать талон?",
                            "Печать талона",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (printResult == DialogResult.Yes)
                        {
                            if (selectedPatientId == 0 || selectedScheduleId == 0)
                            {
                                MessageBox.Show("Невозможно распечатать. Выберите пациента и расписание.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            try
                            {
                                // Создание документа Word
                                Word.Application wordApp = new Word.Application();
                                Word.Document doc = wordApp.Documents.Add();

                                // Добавление логотипа
                                string tempLogoPath = Path.Combine(Path.GetTempPath(), "temp_logo.png");
                                Properties.Resources.logo.Save(tempLogoPath, ImageFormat.Png);
                                Word.Paragraph logoParagraph = doc.Content.Paragraphs.Add();
                                var shape = logoParagraph.Range.InlineShapes.AddPicture(tempLogoPath, LinkToFile: false, SaveWithDocument: true);
                                shape.Width = 60;
                                shape.Height = 60;
                                logoParagraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                                // Настройка размера страницы
                                doc.PageSetup.PageWidth = wordApp.CentimetersToPoints(8);
                                doc.PageSetup.PageHeight = wordApp.CentimetersToPoints(8);
                                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(0.5f);
                                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(0.5f);
                                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(0.5f);
                                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(0.5f);

                                // Заголовок талона
                                Word.Paragraph para = doc.Content.Paragraphs.Add();
                                para.Range.Text = "ЗАПИСЬ НА ПРИЁМ";
                                para.Range.Font.Size = 12;
                                para.Range.Font.Bold = 1;
                                para.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                                para.Range.InsertParagraphAfter();

                                // Информация о пациенте, враче и времени
                                string patStr = label9.Text.Replace("Пациент: ", "");
                                string docStr = label1.Text.Replace("Врач: ", "");
                                string dateStr = DateTime.Parse(label7.Text.Replace("Дата приема: ", "")).ToString("dd.MM.yyyy");
                                string timeStr = label8.Text.Replace("Время приема: ", "").Trim();

                                Word.Paragraph infoPara = doc.Content.Paragraphs.Add();
                                infoPara.Range.Text = $"Пациент: {patStr}\n" +
                                                      $"Врач: {docStr}\n" +
                                                      $"Дата: {dateStr}  Время: {timeStr}";
                                infoPara.Range.Font.Size = 12;
                                infoPara.Range.Font.Bold = 1;
                                infoPara.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                                infoPara.Range.InsertParagraphAfter();
                                wordApp.Visible = true; // показываем Word

                                MessageBox.Show("Талон подготовлен в Word.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Ошибка при создании документа Word: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        // Очистка формы после сохранения
                        dataGridView2.Rows.Clear();
                        label6.Text = "Итого: 0 руб.";
                        label5.Text = "Скидка: 0 руб.";
                        label10.Text = "К оплате: 0 руб.";
                        label1.Text = "Врач: ";
                        label7.Text = "Дата приема: ";
                        label8.Text = "Время приема: ";
                        label9.Text = "Пациент: ";

                        selectedPatientId = selectedScheduleId = 0; // сброс выбора
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // откат транзакции при ошибке
                        MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Удаление услуги из талона
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

            // Подтверждение удаления
            DialogResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранную услугу из талона?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dataGridView2.SelectedRows)
                {
                    dataGridView2.Rows.Remove(row); // удаляем выбранные строки
                }

                UpdateTotal(); // пересчёт суммы
            }
        }
    }
}
