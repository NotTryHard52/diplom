using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Patient : Form
    {
        string connectionString; // Строка подключения к базе данных
        DataTable patientTable; // Таблица для хранения данных пациентов
        public event Action<int, string> PatientSelected; // Событие для передачи выбранного пациента
        int selectedId = -1; // Id выбранного пациента
        private bool openedFromTalon = false; // Флаг, был ли вызов формы из создания талона
        private Timer unmaskTimer;
        private int unmaskedRowIndex = -1;

        public Patient(bool fromTalon = false)
        {
            InitializeComponent();

            openedFromTalon = fromTalon;

            // Кнопка "Выбрать" отображается только если форма открыта из талона
            button4.Visible = openedFromTalon;
        }

        // Загрузка формы
        private void Patient_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0; // Установка сортировки по умолчанию
            LoadPatient(); // Загрузка данных пациентов
            var hoverEffect = new HoverDataGridView(dataGridView1); // Визуальный эффект наведения на строки

            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
        }

        // Загрузка данных пациентов из базы данных
        private void LoadPatient()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                patientTable = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Patients;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(patientTable);

                // Маскируем имя и отчество в таблице
                MaskNameAndPatronymic();

                // Настройка DataGridView
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = patientTable;

                // Настройка заголовков колонок
                dataGridView1.Columns[0].Visible = false; // Скрываем Id
                dataGridView1.Columns[1].HeaderText = "Фамилия";
                dataGridView1.Columns[2].HeaderText = "Имя";
                dataGridView1.Columns[3].HeaderText = "Отчество";
                dataGridView1.Columns[4].HeaderText = "Дата рождения";
                dataGridView1.Columns[5].HeaderText = "Номер телефона";
                dataGridView1.Columns[6].HeaderText = "Номер полиса";

                // Показываем количество записей
                label9.Text = $"Количество записей: {patientTable.Rows.Count}";
            }
        }

        // Метод для маскировки имени и отчества
        private void MaskNameAndPatronymic()
        {
            if (patientTable == null || patientTable.Rows.Count == 0) return;

            foreach (DataRow row in patientTable.Rows)
            {
                string name = row["Name"]?.ToString();
                string patronymic = row["Lastname"]?.ToString();
                string phone = row["Phone_number"]?.ToString();
                string policy = row["Number_policy"]?.ToString();

                // Маскируем имя
                if (!string.IsNullOrEmpty(name) && name.Length > 1)
                {
                    row["Name"] = $"{name[0]}{new string('*', name.Length - 1)}";
                }

                // Маскируем отчество
                if (!string.IsNullOrEmpty(patronymic) && patronymic.Length > 1)
                {
                    row["Lastname"] = $"{patronymic[0]}{new string('*', patronymic.Length - 1)}";
                }

                // Маскируем номер телефона - показываем только последние 4 цифры
                if (!string.IsNullOrEmpty(phone) && phone.Length > 5)
                {
                    row["Phone_number"] = new string('*', phone.Length - 5) + phone.Substring(phone.Length - 5);
                }

                // Маскируем номер полиса - показываем только последние 4 цифры
                if (!string.IsNullOrEmpty(policy) && policy.Length > 4)
                {
                    row["Number_policy"] = new string('*', policy.Length - 4) + policy.Substring(policy.Length - 4);
                }
            }
        }

        // Редактирование выбранного пациента
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["idPatients"].Value);
            EditPatient ed = new EditPatient(id);
            ed.ShowDialog();

            LoadPatient(); // Обновляем таблицу после редактирования
        }

        // Смена параметра сортировки
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        // Применение фильтра и сортировки к таблице
        private void ApplyFilterAndSort()
        {
            if (patientTable == null) return;

            string filterExpr = "";

            // Поиск по номеру полиса
            string searchText = textBox5.Text.Trim().Replace("'", "''");
            if (!string.IsNullOrEmpty(searchText))
            {
                if (int.TryParse(searchText, out int policyNumber))
                {
                    filterExpr = $"Number_policy = {policyNumber}";
                }
            }

            // Сортировка по фамилии
            string sortExpr = "";
            if (comboBox2.SelectedIndex == 1)
                sortExpr = "Surname ASC";
            else if (comboBox2.SelectedIndex == 2)
                sortExpr = "Surname DESC";

            DataView dv = patientTable.DefaultView;
            dv.RowFilter = filterExpr;
            dv.Sort = sortExpr;

            dataGridView1.DataSource = dv;
            dataGridView1.Refresh();
        }

        // Изменение текста поиска
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        // Ввод только чисел для поиска по полису
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);
        }

        // Добавление нового пациента
        private void button1_Click(object sender, EventArgs e)
        {
            AddPatient ad = new AddPatient();
            ad.ShowDialog();

            LoadPatient(); // Обновляем таблицу после добавления
        }

        // Выбор пациента для передачи в форму талона
        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пациента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dataGridView1.SelectedRows[0];
            int patientId = Convert.ToInt32(row.Cells["idPatients"].Value);
            string fullName = $"{row.Cells["Surname"].Value} {row.Cells["Name"].Value} {row.Cells["Lastname"].Value}";

            // Вызываем событие для передачи данных
            PatientSelected?.Invoke(patientId, fullName);

            this.Close(); // Закрываем форму
        }

        // Сброс фильтров
        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            textBox5.Text = "";
        }

        // Удаление выбранного пациента
        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка, используется ли пациент в заказах
                string checkQuery = "SELECT COUNT(*) FROM `Order` WHERE Patients_idPatients = @patientId";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@patientId", selectedId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Нельзя удалить этого пациента, так как он используется в приемах!",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    "Вы уверены, что хотите удалить запись?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;

                // Удаление пациента
                string deleteQuery = "DELETE FROM Patients WHERE idPatients = @id";
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@id", selectedId);
                    deleteCmd.ExecuteNonQuery();
                    MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            selectedId = -1;
            LoadPatient(); // Обновляем таблицу после удаления
        }

        // Получение Id выбранной строки при клике
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedId = Convert.ToInt32(row.Cells["idPatients"].Value);
            }
        }
        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Проверяем, что клик не по заголовку

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            int patientId = Convert.ToInt32(row.Cells["idPatients"].Value);

            // Загружаем полные данные пациента из БД без маскировки
            ShowFullPatientData(patientId, row);
        }

        private void ShowFullPatientData(int patientId, DataGridViewRow row)
        {
            Connect connect = new Connect();
            string connString = connect.ConnectDB();

            using (MySqlConnection con = new MySqlConnection(connString))
            {
                con.Open();
                string query = @"
            SELECT Surname, Name, Lastname, Date_birth, Phone_number, Number_policy 
            FROM Patients 
            WHERE idPatients = @id";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", patientId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Обновляем ячейки реальными данными из БД
                            row.Cells["Name"].Value = reader["Name"].ToString();
                            row.Cells["Lastname"].Value = reader["Lastname"].ToString();
                            row.Cells["Phone_number"].Value = reader["Phone_number"].ToString();
                            row.Cells["Number_policy"].Value = reader["Number_policy"].ToString();

                            // Обновляем DataGridView
                            dataGridView1.Refresh();
                        }
                    }
                }
            }
        }

        private void dataGridView1_CellDoubleClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Останавливаем предыдущий таймер, если он работает
            StopUnmaskTimer();

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            int patientId = Convert.ToInt32(row.Cells["idPatients"].Value);

            unmaskedRowIndex = e.RowIndex; // Сохраняем индекс строки

            ShowFullPatientData(patientId, row); // Показываем полные данные

            // Запускаем таймер на 20 секунд (20000 мс)
            unmaskTimer = new Timer();
            unmaskTimer.Interval = 20000;
            unmaskTimer.Tick += UnmaskTimer_Tick;
            unmaskTimer.Start();
        }
        // Обработчик таймера - возвращает маскировку через 20 секунд
        private void UnmaskTimer_Tick(object sender, EventArgs e)
        {
            StopUnmaskTimer();

            if (unmaskedRowIndex >= 0 && unmaskedRowIndex < dataGridView1.Rows.Count)
            {
                // Перезагружаем таблицу для возврата маскировки
                LoadPatient();
                dataGridView1.ClearSelection(); // Снимаем выделение
            }
        }

        // Метод для остановки таймера
        private void StopUnmaskTimer()
        {
            if (unmaskTimer != null)
            {
                unmaskTimer.Stop();
                unmaskTimer.Dispose();
                unmaskTimer = null;
            }
            unmaskedRowIndex = -1;
        }
    }
}
