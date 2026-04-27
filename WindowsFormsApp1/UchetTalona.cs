using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class UchetTalona : Form
    {
        // Строка подключения к базе данных
        string connectionString;

        // Таблица для хранения данных о талонах
        DataTable orderTable;
        int currentPage = 1;
        int pageSize = 10;
        int totalRecords = 0;
        int totalPages = 1;
        bool isGlav = false;

        public UchetTalona(bool isGlav = false)
        {
            InitializeComponent();
            this.isGlav = isGlav;
        }

        private int GetTotalCount(string filterSql)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string query = "SELECT COUNT(*) FROM `Order` o " +
                               "JOIN StatusesPriem st ON o.Status = st.idStatusesPriem " +
                               filterSql;

                MySqlCommand cmd = new MySqlCommand(query, con);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private string BuildFilterSql()
        {
            string where = "WHERE 1=1";

            string status = comboBox1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(status) && status != "Все")
            {
                where += $" AND st.name = '{status.Replace("'", "''")}'";
            }

            string search = textBox5.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                where += $" AND o.idOrder LIKE '%{search}%'";
            }

            return where;
        }
        private string GetSortSql()
        {
            if (comboBox2.SelectedIndex == 1)
                return "ORDER BY sc.date ASC";
            else if (comboBox2.SelectedIndex == 2)
                return "ORDER BY sc.date DESC";

            return "ORDER BY o.idOrder DESC";
        }

        // Событие загрузки формы
        private void UchetTalona_Load(object sender, EventArgs e)
        {
            FillStatus();          // Заполнение comboBox статусами
            comboBox1.SelectedIndex = 0; // По умолчанию "Все"
            comboBox2.SelectedIndex = 0; // По умолчанию без сортировки
            ReloadOrderTable();    // Загрузка данных из базы
            if(!isGlav)
            {
                label2.Visible = false;
                button6.Visible = false;
            }
        }

        // Подсчёт общей выручки (не учитываются отменённые и созданные талоны)
        private void UpdateRevenueSum()
        {
            if (dataGridView1.DataSource == null) return;

            decimal total = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Сумма"].Value == null || row.Cells["Сумма"].Value == DBNull.Value)
                    continue;

                string status = row.Cells["Статус"].Value?.ToString()?.ToLower();

                // Пропускаем отменённые и только созданные талоны
                if (status != null && (status.Contains("отмен") || status.Contains("создан")))
                    continue;

                decimal value;
                if (decimal.TryParse(row.Cells["Сумма"].Value.ToString(), out value))
                {
                    total += value;
                }
            }

            label2.Text = $"Общая выручка: {total:N2} руб.";
        }

        // Заполнение comboBox1 статусами из базы
        private void FillStatus()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT DISTINCT Name FROM StatusesPriem;";
                MySqlCommand cmd = new MySqlCommand(query, con);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Все"); // Добавляем опцию "Все"
                    while (reader.Read())
                    {
                        string status = reader["name"].ToString();
                        comboBox1.Items.Add(status); // Добавляем каждый статус
                    }
                }
            }
        }

        // Событие изменения выбора в comboBox1
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort(); // Применяем фильтр и сортировку
        }

        // Применение фильтров и сортировки к DataGridView
        private void ApplyFilterAndSort()
        {
            // Переключаем на серверную фильтрацию/сортировку и сбрасываем на первую страницу
            currentPage = 1;
            ReloadOrderTable();
        }

        // Событие изменения выбора сортировки
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        // Событие изменения текста поиска
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        // Кнопка "Открыть талон"
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int orderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Номер талона"].Value);
                if (!isGlav)
                {
                    ViewPriem v = new ViewPriem(orderId, false);
                    var result = v.ShowDialog(); // открываем форму просмотра талона
                }
                else
                {
                    ViewPriem v = new ViewPriem(orderId, true);
                    var result = v.ShowDialog(); // открываем форму просмотра талона
                }
                ReloadOrderTable(); // обновляем таблицу после просмотра
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите талон.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Загрузка данных о талонах из базы
        private void ReloadOrderTable()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();

            string filterSql = BuildFilterSql();
            string sortSql = GetSortSql();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // считаем записи
                totalRecords = GetTotalCount(filterSql);
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // защита от выхода за границы
                if (currentPage > totalPages)
                    currentPage = totalPages == 0 ? 1 : totalPages;

                int offset = (currentPage - 1) * pageSize;

                string query = $@"
                    SELECT 
                        o.idOrder AS 'Номер талона',
                        CONCAT(d.surname, ' ', d.name, ' ', d.lastname) AS 'Врач',
                        DATE_FORMAT(sc.date, '%d.%m.%Y') AS 'Дата',
                        DATE_FORMAT(sc.time, '%H:%i') AS 'Время',
                        CONCAT(r.surname, ' ', r.name, ' ', r.lastname) AS 'Регистратор',
                        CONCAT(p.surname, ' ', p.name, ' ', p.lastname) AS 'Пациент',
                        st.name AS 'Статус',
                        o.sum AS 'Сумма',
                        o.Discount AS 'Скидка',
                        o.TotalSum AS 'К оплате'
                    FROM `Order` o
                    JOIN Schedule sc ON o.Schedule = sc.idSchedule
                    JOIN Doctors d ON sc.idDoctor = d.idDoctors
                    JOIN `Users` r ON o.User = r.idUsers
                    JOIN Patients p ON o.Patients_idPatients = p.idPatients
                    JOIN StatusesPriem st ON o.Status = st.idStatusesPriem
                    {filterSql}
                    {sortSql}
                    LIMIT @offset, @pageSize;
                ";

                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@offset", offset);
                cmd.Parameters.AddWithValue("@pageSize", pageSize);

                DataTable table = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(table);

                orderTable = table;

                dataGridView1.DataSource = table;
                dataGridView1.Columns["Сумма"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView1.Columns["К оплате"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView1.Columns["Скидка"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView1.Columns["Время"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns["Статус"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns["Врач"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns["Пациент"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns["Регистратор"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns["Номер талона"].Width = 75;
                dataGridView1.Columns["Сумма"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["Скидка"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["К оплате"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["Дата"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["Время"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["Статус"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                int start = (currentPage - 1) * pageSize + 1;
                int end = Math.Min(currentPage * pageSize, totalRecords);

                if (totalRecords == 0)
                {
                    start = 0;
                    end = 0;
                }

                groupBox2.Text = $"Количество записей: {start}-{end} из {totalRecords}";
                label1.Text = $"Страница {currentPage} из {totalPages}";
                textBox1.Clear();
                UpdateRevenueSum();
            }
        }

        // Кнопка "Сброс фильтров"
        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            textBox5.Text = "";
        }

        // Изменение цвета строк в зависимости от статуса
        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells["Статус"].Value == null)
                return;

            string status = dataGridView1.Rows[e.RowIndex].Cells["Статус"].Value.ToString().ToLower();

            if (status.Contains("заверш"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen; // завершён
            }
            else if (status.Contains("отмен"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral; // отменён
            }
            else if (status.Contains("создан"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow; // создан
            }
            else
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White; // прочие
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                ReloadOrderTable();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                ReloadOrderTable();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int page;
            if (int.TryParse(textBox1.Text, out page))
            {
                if (page >= 1 && page <= totalPages)
                {
                    currentPage = page;
                    ReloadOrderTable();
                }
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OtchetExcel v = new OtchetExcel();
            v.ShowDialog();
        }
    }
}
