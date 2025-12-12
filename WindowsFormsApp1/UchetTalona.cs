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

        public UchetTalona()
        {
            InitializeComponent();
        }

        // Событие загрузки формы
        private void UchetTalona_Load(object sender, EventArgs e)
        {
            FillStatus();          // Заполнение comboBox статусами
            comboBox1.SelectedIndex = 0; // По умолчанию "Все"
            comboBox2.SelectedIndex = 0; // По умолчанию без сортировки
            ReloadOrderTable();    // Загрузка данных из базы
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
            if (orderTable == null) return;

            string filterExpr = "";

            // Фильтр по выбранному статусу
            string selectedSpecialty = comboBox1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedSpecialty) && selectedSpecialty != "Все")
            {
                filterExpr = $"Статус = '{selectedSpecialty.Replace("'", "''")}'";
            }

            // Фильтр по номеру талона (поиск)
            string searchText = textBox5.Text.Trim().Replace("'", "''");
            if (!string.IsNullOrEmpty(searchText))
            {
                if (!string.IsNullOrEmpty(filterExpr))
                {
                    filterExpr += " AND ";
                }
                filterExpr += $"Convert([Номер талона], 'System.String') LIKE '%{searchText}%'";
            }

            // Сортировка по дате
            string sortExpr = "";
            if (comboBox2.SelectedIndex == 1)
                sortExpr = "Дата ASC";
            else if (comboBox2.SelectedIndex == 2)
                sortExpr = "Дата DESC";

            DataView dv = orderTable.DefaultView;
            dv.RowFilter = filterExpr; // применяем фильтр
            dv.Sort = sortExpr;        // применяем сортировку

            dataGridView1.DataSource = dv;
            dataGridView1.Refresh();
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
                ViewPriem v = new ViewPriem(orderId, false);
                var result = v.ShowDialog(); // открываем форму просмотра талона

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

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                orderTable = new DataTable();
                string query = @"
            SELECT 
                o.idOrder AS 'Номер талона',
                o.sum AS 'Сумма',
                o.Discount AS 'Скидка',
                o.TotalSum AS 'К оплате',
                CONCAT(d.surname, ' ', d.name, ' ', d.lastname) AS 'Врач',
                DATE_FORMAT(sc.date, '%d.%m.%Y') AS 'Дата',
                DATE_FORMAT(sc.time, '%H:%i') AS 'Время',
                CONCAT(r.surname, ' ', r.name, ' ', r.lastname) AS 'Регистратор',
                CONCAT(p.surname, ' ', p.name, ' ', p.lastname) AS 'Пациент',
                st.name AS 'Статус'
            FROM `Order` o
            JOIN Schedule sc ON o.Schedule = sc.idSchedule
            JOIN Doctors d ON sc.idDoctor = d.idDoctors
            JOIN `Users` r ON o.User = r.idUsers
            JOIN Patients p ON o.Patients_idPatients = p.idPatients
            JOIN StatusesPriem st ON o.Status = st.idStatusesPriem
        ";

                MySqlCommand cmd = new MySqlCommand(query, con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(orderTable);

                dataGridView1.DataSource = orderTable; // привязываем данные к таблице
                ApplyFilterAndSort(); // применяем фильтр и сортировку
                label9.Text = $"Количество записей: {orderTable.Rows.Count}"; // показываем количество записей
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
    }
}
