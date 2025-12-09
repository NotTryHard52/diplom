using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Status : Form
    {
        string connectionString; // Строка подключения к базе данных
        int selectedId = -1; // Id выбранного статуса, используется для редактирования и удаления

        // Конструктор формы
        public Status()
        {
            InitializeComponent();
        }

        // Событие загрузки формы
        private void Status_Load(object sender, EventArgs e)
        {
            // Получаем строку подключения через класс Connect
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();

            // Загружаем данные статусов в DataGridView
            LoadStatus();

            // Добавляем эффект подсветки строк при наведении мыши
            var hoverEffect = new HoverDataGridView(dataGridView1);
        }

        // Метод для загрузки всех статусов из базы данных
        private void LoadStatus()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                DataTable t = new DataTable();

                // Выбираем все записи из таблицы Statuses
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Statuses;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);

                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;

                // Скрываем колонку с id
                dataGridView1.Columns[0].Visible = false;

                // Переименовываем колонку с названием статуса
                dataGridView1.Columns[1].HeaderText = "Наименование";

                label2.Text = $"Количество записей: {t.Rows.Count}";
            }
        }

        // Ограничение ввода в текстовое поле только русскими буквами
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian(sender, e);
        }

        // Добавление нового статуса
        private void button1_Click(object sender, EventArgs e)
        {
            string statusName = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(statusName))
            {
                MessageBox.Show("Поле не должно быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка на дубликат
                string checkQuery = "SELECT COUNT(*) FROM Statuses WHERE StatusName = @name";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@name", statusName);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Вставка новой записи
                string insertQuery = "INSERT INTO Statuses (StatusName) VALUES (@name)";
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@name", statusName);
                insertCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            LoadStatus(); // Перезагружаем список статусов
        }

        // Выбор записи при клике на DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedId = Convert.ToInt32(row.Cells[0].Value);
                textBox1.Text = row.Cells[1].Value.ToString();
            }
        }

        // Редактирование выбранного статуса
        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newName = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Поле не должно быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            if (newName == currentName)
            {
                MessageBox.Show("Вы не внесли изменений!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка на дубликат среди других записей
                string checkQuery = "SELECT COUNT(*) FROM Statuses WHERE StatusName = @name AND IdStatuses != @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@name", newName);
                checkCmd.Parameters.AddWithValue("@id", selectedId);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Обновление записи
                string updateQuery = "UPDATE Statuses SET StatusName = @name WHERE IdStatuses = @id";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, con);
                updateCmd.Parameters.AddWithValue("@name", newName);
                updateCmd.Parameters.AddWithValue("@id", selectedId);
                updateCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            selectedId = -1;
            LoadStatus();
        }

        // Удаление выбранного статуса
        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string statusName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка, используется ли этот статус в расписании
                string checkQuery = "SELECT COUNT(*) FROM Schedule WHERE Status = @statusId";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@statusId", selectedId);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show("Нельзя удалить этот статус, так как он используется в расписании!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить запись: \"{statusName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;

                // Удаление записи
                string deleteQuery = "DELETE FROM Statuses WHERE idStatuses = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con);
                deleteCmd.Parameters.AddWithValue("@id", selectedId);
                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            selectedId = -1;
            LoadStatus();
        }
    }
}
