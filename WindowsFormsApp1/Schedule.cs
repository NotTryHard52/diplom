using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Schedule : Form
    {
        // Строка подключения к базе данных
        string connectionString;
        // Таблица с данными расписания
        DataTable scheduleTable;
        // ID выбранной записи расписания
        int selectedScheduleId = -1;
        // Событие, передающее выбранную запись (для формы талона)
        public event Action<int, string, string, string> ScheduleSelected;
        // Статус выбранного приёма ("Свободно", "Занято")
        string status;
        // Флаг: открыто из формы талона
        private bool openedFromTalon = false;
        // Хранит индексы свернутых групп врачей
        private HashSet<int> collapsedGroups = new HashSet<int>();

        // Конструктор формы
        public Schedule(bool fromTalon = false)
        {
            InitializeComponent();
            openedFromTalon = fromTalon;
            // Кнопка выбора записи видна только если форма открыта из талона
            button4.Visible = openedFromTalon;
        }

        // Загрузка формы
        private void Schedule_Load(object sender, EventArgs e)
        {
            FillStatuses();  // Заполнение ComboBox статусов приёмов
            LoadSchedule();  // Загрузка расписания из базы
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();  // Получаем строку подключения
        }

        // Загрузка расписания из базы
        private void LoadSchedule()
        {
            InputLimit.DateOrder(dateTimePicker1); // Ограничение выбора даты
            comboBox2.SelectedIndex = 0; // Сбрасываем фильтр по статусу
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // SQL-запрос для получения расписания с именами врачей и статусами
                string query = @"
                    SELECT 
                        s.idSchedule, 
                        s.idDoctor, 
                        CONCAT(d.Surname, ' ', d.Name, ' ', d.Lastname) AS Врач,
                        s.date AS `Дата приема`, 
                        s.time AS `Время приема`, 
                        st.statusname AS `Статус`
                    FROM Schedule s
                    JOIN Doctors d ON s.idDoctor = d.idDoctors
                    JOIN Statuses st ON s.Status = st.idStatuses
                    ORDER BY d.Surname, s.date, s.time;";

                scheduleTable = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(query, con);
                da.Fill(scheduleTable); // Заполняем DataTable

                // Очистка DataGridView перед заполнением
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();

                // Добавляем колонки вручную
                foreach (DataColumn col in scheduleTable.Columns)
                    dataGridView1.Columns.Add(col.ColumnName, col.ColumnName);

                // Скрываем служебные колонки
                dataGridView1.Columns["idSchedule"].Visible = false;
                dataGridView1.Columns["idDoctor"].Visible = false; // Для ComboBox выбора врача

                // Отключаем сортировку по клику
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;

                // --- Группировка по врачу ---
                string currentDoctor = "";
                int groupRowIndex = -1;
                collapsedGroups.Clear(); // Очищаем свернутые группы

                foreach (DataRow row in scheduleTable.Rows)
                {
                    string doctor = row["Врач"].ToString();

                    // Если новый врач, добавляем строку группы
                    if (doctor != currentDoctor)
                    {
                        currentDoctor = doctor;
                        groupRowIndex = dataGridView1.Rows.Add("", "", doctor, "", "", "");
                        var groupRow = dataGridView1.Rows[groupRowIndex];
                        groupRow.DefaultCellStyle.BackColor = Color.LightGray; // Цвет группы
                        groupRow.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                        groupRow.Tag = "GROUP"; // Метка группы
                        groupRow.Cells[2].Value = "▼ " + doctor; // Стрелка вниз
                    }

                    // Добавляем обычную строку расписания
                    int rowIndex = dataGridView1.Rows.Add(
                        row["idSchedule"],
                        row["idDoctor"],
                        "   " + row["Врач"], // Отступ для визуальной группировки
                        ((DateTime)row["Дата приема"]).ToString("dd.MM.yyyy"),
                        ((TimeSpan)row["Время приема"]).ToString(@"hh\:mm"),
                        row["Статус"]
                    );

                    var normalRow = dataGridView1.Rows[rowIndex];
                    // Подсветка статуса: зелёный — свободно, красный — занято
                    if (row["Статус"].ToString() == "Свободно")
                        normalRow.DefaultCellStyle.BackColor = Color.LightGreen;
                    else
                        normalRow.DefaultCellStyle.BackColor = Color.LightCoral;

                    // Если группа свернута, скрываем строку
                    if (collapsedGroups.Contains(groupRowIndex))
                        normalRow.Visible = false;
                }

                label2.Text = $"Количество записей: {scheduleTable.Rows.Count}";

                // Заполняем ComboBox врачей для добавления/редактирования
                string docQuery = "SELECT idDoctors, CONCAT(Surname, ' ', Name, ' ', Lastname) AS DoctorName FROM Doctors;";
                MySqlDataAdapter docDa = new MySqlDataAdapter(docQuery, con);
                DataTable docTable = new DataTable();
                docDa.Fill(docTable);

                comboBox1.DisplayMember = "DoctorName";
                comboBox1.ValueMember = "idDoctors";
                comboBox1.DataSource = docTable;
                comboBox1.SelectedIndex = -1; // Сброс выбора
            }
        }
        // Метод для построения DataGridView с группировкой по врачу
        private void BuildGroupedGrid(DataView dv = null)
        {
            if (scheduleTable == null) return;

            // Сброс DataGridView
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            if (dv == null) dv = scheduleTable.DefaultView;

            // Добавляем колонки
            foreach (DataColumn col in scheduleTable.Columns)
                dataGridView1.Columns.Add(col.ColumnName, col.ColumnName);

            // Скрываем служебные колонки
            dataGridView1.Columns["idSchedule"].Visible = false;
            dataGridView1.Columns["idDoctor"].Visible = false;

            // Отключаем сортировку
            foreach (DataGridViewColumn col in dataGridView1.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            // Группировка по врачу
            string currentDoctor = "";
            int groupRowIndex = -1;

            foreach (DataRowView drv in dv)
            {
                string doctor = drv["Врач"].ToString();

                // Если новый врач, добавляем строку группы
                if (doctor != currentDoctor)
                {
                    currentDoctor = doctor;
                    groupRowIndex = dataGridView1.Rows.Add("", "", doctor, "", "", "");
                    var groupRow = dataGridView1.Rows[groupRowIndex];
                    groupRow.DefaultCellStyle.BackColor = Color.LightGray;
                    groupRow.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                    groupRow.Tag = "GROUP"; // Метка группы
                    groupRow.Cells[2].Value = "▼ " + doctor;
                }

                // Добавляем обычную строку расписания
                int rowIndex = dataGridView1.Rows.Add(
                    drv["idSchedule"],
                    drv["idDoctor"],
                    "   " + drv["Врач"],
                    ((DateTime)drv["Дата приема"]).ToString("dd.MM.yyyy"),
                    ((TimeSpan)drv["Время приема"]).ToString(@"hh\:mm"),
                    drv["Статус"]
                );

                var normalRow = dataGridView1.Rows[rowIndex];
                if (drv["Статус"].ToString() == "Свободно")
                    normalRow.DefaultCellStyle.BackColor = Color.LightGreen;
                else
                    normalRow.DefaultCellStyle.BackColor = Color.LightCoral;

                // Скрываем строки, если группа свернута
                if (collapsedGroups.Contains(groupRowIndex))
                    normalRow.Visible = false;
            }
        }

        // Метод для заполнения ComboBox2 статусами приёмов
        private void FillStatuses()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT DISTINCT statusname FROM Statuses;";
                MySqlCommand cmd = new MySqlCommand(query, con);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    comboBox2.Items.Clear();
                    comboBox2.Items.Add("Все"); // Для отображения всех записей
                    while (reader.Read())
                    {
                        string statuses = reader["statusname"].ToString();
                        comboBox2.Items.Add(statuses);
                    }
                }
            }
        }

        // Обработчик изменения статуса в ComboBox2
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort(); // Применяем фильтр по выбранному статусу
        }

        // Метод фильтрации и обновления DataGridView
        private void ApplyFilterAndSort()
        {
            if (scheduleTable == null) return;

            string filterExpr = "";

            string selectedStatus = comboBox2.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Все")
            {
                // SQL-подобный фильтр для DataView
                filterExpr = $"Статус = '{selectedStatus.Replace("'", "''")}'";
            }

            DataView dv = new DataView(scheduleTable);
            dv.RowFilter = filterExpr; // Применяем фильтр
            BuildGroupedGrid(dv);      // Перестраиваем DataGridView
        }

        // Добавление новой записи расписания
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите врача!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int doctorId = Convert.ToInt32(comboBox1.SelectedValue);
            DateTime date = dateTimePicker1.Value.Date;
            TimeSpan time = dateTimePicker2.Value.TimeOfDay;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка на существующую запись у этого врача в это время
                string checkQuery = @"
                    SELECT COUNT(*) FROM Schedule
                    WHERE idDoctor = @idDoctor AND date = @date AND time = @time";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@idDoctor", doctorId);
                    checkCmd.Parameters.AddWithValue("@date", date);
                    checkCmd.Parameters.AddWithValue("@time", time);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("У этого врача уже есть запись на указанное время!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Получаем ID статуса "Свободно"
                int statusId = 0;
                string getStatusQuery = "SELECT idStatuses FROM Statuses WHERE statusname = 'Свободно' LIMIT 1;";
                using (MySqlCommand statusCmd = new MySqlCommand(getStatusQuery, con))
                {
                    object result = statusCmd.ExecuteScalar();
                    if (result == null)
                    {
                        MessageBox.Show("Не найден статус 'Свободно' в таблице Statuses!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    statusId = Convert.ToInt32(result);
                }

                // Вставка новой записи
                string insertQuery = @"
                    INSERT INTO Schedule (idDoctor, date, time, Status)
                    VALUES (@idDoctor, @date, @time, @status)";
                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@idDoctor", doctorId);
                    insertCmd.Parameters.AddWithValue("@date", date);
                    insertCmd.Parameters.AddWithValue("@time", time);
                    insertCmd.Parameters.AddWithValue("@status", statusId);
                    insertCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            LoadSchedule(); // Перезагрузка расписания
        }
        // Обработчик изменения времени в dateTimePicker2
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            // Если выбранная дата сегодня, проверяем, чтобы время не было в прошлом
            if (dateTimePicker1.Value.Date == DateTime.Now.Date)
            {
                DateTime selectedDateTime = dateTimePicker1.Value.Date + dateTimePicker2.Value.TimeOfDay;
                if (selectedDateTime < DateTime.Now)
                {
                    dateTimePicker2.Value = DateTime.Now; // Подставляем текущее время
                }
            }
        }

        // Обработчик кнопки выбора записи для талона
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите запись!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataGridViewRow row = dataGridView1.SelectedRows[0];

                int scheduleId = Convert.ToInt32(row.Cells["idSchedule"].Value);
                string doctorName = row.Cells["Врач"].Value.ToString();

                // Проверка корректности формата даты и времени
                if (!DateTime.TryParse(row.Cells["Дата приема"].Value.ToString(), out DateTime date))
                {
                    MessageBox.Show("Неверный формат даты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!TimeSpan.TryParse(row.Cells["Время приема"].Value.ToString(), out TimeSpan time))
                {
                    MessageBox.Show("Неверный формат времени!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DateTime appointmentDateTime = date.Date + time;

                // Проверка на прошлое время
                if (appointmentDateTime < DateTime.Now)
                {
                    MessageBox.Show("Нельзя выбрать прошлое время!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string status = row.Cells["Статус"].Value.ToString();

                if (status != "Свободно")
                {
                    MessageBox.Show("Выбранный слот уже занят!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Вызов события для передачи данных выбранной записи
                ScheduleSelected?.Invoke(scheduleId, doctorName, date.ToString("yyyy-MM-dd"), time.ToString());

                this.Close(); // Закрываем форму после выбора
            }
            catch (FormatException)
            {
                MessageBox.Show("Нельзя выбрать данную строку!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        // Обработчик кнопки "Сброс фильтра статуса"
        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0; // Выбираем "Все"
        }

        // Редактирование выбранной записи расписания
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверяем, что запись свободна
            if (status != "Свободно")
            {
                MessageBox.Show("Редактирование невозможно — расписание уже занято!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                selectedScheduleId = -1;
                return;
            }

            if (selectedScheduleId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите врача!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int doctorId = Convert.ToInt32(comboBox1.SelectedValue);
            DateTime date = dateTimePicker1.Value.Date;
            TimeSpan time = dateTimePicker2.Value.TimeOfDay;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка, что у врача нет другой записи в это время
                string checkQuery = @"
                    SELECT COUNT(*) FROM Schedule
                    WHERE idDoctor = @idDoctor AND date = @date AND time = @time 
                          AND idSchedule <> @idSchedule";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@idDoctor", doctorId);
                    checkCmd.Parameters.AddWithValue("@date", date);
                    checkCmd.Parameters.AddWithValue("@time", time);
                    checkCmd.Parameters.AddWithValue("@idSchedule", selectedScheduleId);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("У этого врача уже есть запись на указанное время!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Обновление записи в базе
                string updateQuery = @"
                    UPDATE Schedule
                    SET idDoctor = @doctorId, date = @date, time = @time
                    WHERE idSchedule = @idSchedule";
                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, con))
                {
                    updateCmd.Parameters.AddWithValue("@doctorId", doctorId);
                    updateCmd.Parameters.AddWithValue("@date", date);
                    updateCmd.Parameters.AddWithValue("@time", time);
                    updateCmd.Parameters.AddWithValue("@idSchedule", selectedScheduleId);
                    updateCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            selectedScheduleId = -1;
            LoadSchedule(); // Перезагружаем расписание
        }

        // Обработчик клика по DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Игнорируем заголовки

            DataGridViewRow clickedRow = dataGridView1.Rows[e.RowIndex];

            // --- Обработка групповой строки (сворачивание/разворачивание) ---
            if (clickedRow.Tag != null && clickedRow.Tag.ToString() == "GROUP")
            {
                bool collapsed = clickedRow.Cells[2].Value.ToString().StartsWith("▼ ");
                clickedRow.Cells[2].Value = (collapsed ? "► " : "▼ ") + clickedRow.Cells[2].Value.ToString().Substring(2);

                if (collapsed)
                    collapsedGroups.Add(e.RowIndex);
                else
                    collapsedGroups.Remove(e.RowIndex);

                // Скрываем или показываем подстроки группы
                for (int i = e.RowIndex + 1; i < dataGridView1.Rows.Count; i++)
                {
                    var r = dataGridView1.Rows[i];
                    if (r.Tag != null && r.Tag.ToString() == "GROUP") break;
                    r.Visible = !collapsed;
                }
                return;
            }

            if (!clickedRow.Visible) return;

            // --- Выбор конкретной записи ---
            selectedScheduleId = Convert.ToInt32(clickedRow.Cells["idSchedule"].Value);
            status = clickedRow.Cells["Статус"].Value.ToString();

            // Устанавливаем врача в ComboBox
            if (clickedRow.Cells["idDoctor"] != null && int.TryParse(clickedRow.Cells["idDoctor"].Value.ToString(), out int doctorId))
            {
                comboBox1.SelectedValue = doctorId;
            }

            // Дата и время для редактирования
            if (DateTime.TryParse(clickedRow.Cells["Дата приема"].Value.ToString(), out DateTime dateFromDB))
            {
                DateTime dateValue = dateFromDB < DateTime.Today ? DateTime.Today : dateFromDB;
                dateTimePicker1.Value = dateValue;
            }

            if (TimeSpan.TryParse(clickedRow.Cells["Время приема"].Value.ToString(), out TimeSpan timeFromDB))
            {
                dateTimePicker2.Value = dateTimePicker1.Value.Date.Add(timeFromDB);
            }
        }

        // Удаление выбранной записи расписания
        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedScheduleId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (status != "Свободно")
            {
                MessageBox.Show("Нельзя удалить занятую запись расписания!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Вы уверены, что хотите удалить выбранную запись расписания?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No) return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string deleteQuery = "DELETE FROM Schedule WHERE idSchedule = @id";
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@id", selectedScheduleId);
                    deleteCmd.ExecuteNonQuery();
                    MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            selectedScheduleId = -1;
            LoadSchedule(); // Обновляем расписание
        }
    }
}