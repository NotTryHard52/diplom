using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Category : Form
    {
        string connectionString;                     // Строка подключения к базе данных
        int selectedId = -1;                         // Выбранный ID категории (-1 = ничего не выбрано)

        public Category()
        {
            InitializeComponent();                   // Инициализация компонентов формы
        }

        private void Category_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();         // Создаём объект для получения строки подключения
            connectionString = connect.ConnectDB();  // Получаем строку подключения
            LoadCategory();                          // Загружаем список категорий в таблицу

            var hoverEffect = new HoverDataGridView(dataGridView1); // Подключаем эффект подсветки строк
        }

        private void LoadCategory()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();                          // Открываем соединение с БД

                DataTable t = new DataTable();       // Создаём таблицу для данных
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Category;", con);
                // Команда выборки категорий

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);  // Адаптер для заполнения таблицы
                da.Fill(t);                                     // Загружаем данные в DataTable

                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                // Выделение всей строки при клике

                dataGridView1.DataSource = t;        // Привязываем таблицу к DataGridView

                dataGridView1.Columns[0].Visible = false;  // Скрываем ID
                dataGridView1.Columns[1].HeaderText = "Наименование"; // Переименовываем заголовок

                label2.Text = $"Количество записей: {t.Rows.Count}";  // Отображаем количество записей
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian(sender, e);           // Разрешаем только русские буквы
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string categoryName = textBox1.Text.Trim();  // Получаем введённое название

            if (string.IsNullOrEmpty(categoryName))      // Проверка на пустоту
            {
                MessageBox.Show("Поле не должно быть пустым!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();                              // Открываем соединение

                string checkQuery = "SELECT COUNT(*) FROM Category WHERE Name = @name";
                // Проверяем, существует ли уже такая категория

                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@name", categoryName);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar()); // Получаем количество совпадений

                if (count > 0)
                {
                    MessageBox.Show("Такая запись уже существует!", "Дубликат",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string insertQuery = "INSERT INTO Category (Name) VALUES (@name)";
                // SQL-запрос добавления
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@name", categoryName);
                insertCmd.ExecuteNonQuery();              // Выполняем вставку

                MessageBox.Show("Запись успешно добавлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();                             // Очищаем поле ввода
            LoadCategory();                               // Обновляем таблицу
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)                          // Проверяем, что нажата строка, а не заголовок
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];  // Получаем выбранную строку

                selectedId = Convert.ToInt32(row.Cells[0].Value);      // Сохраняем ID категории
                textBox1.Text = row.Cells[1].Value.ToString();         // Заполняем текстбокс значением
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)                          // Проверка выбранной записи
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newName = textBox1.Text.Trim();         // Новое имя категории
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Поле не должно быть пустым!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            // Имя категории до изменений

            if (newName == currentName)                    // Проверка, были ли изменения
            {
                MessageBox.Show("Вы не внесли изменений!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery =
                    "SELECT COUNT(*) FROM Category WHERE Name = @name AND IdCategory != @id";
                // Проверка уникальности при редактировании

                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@name", newName);
                checkCmd.Parameters.AddWithValue("@id", selectedId);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("Такая запись уже существует!", "Дубликат",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string updateQuery =
                    "UPDATE Category SET Name = @name WHERE IdCategory = @id";
                // SQL-запрос обновления категории

                MySqlCommand updateCmd = new MySqlCommand(updateQuery, con);
                updateCmd.Parameters.AddWithValue("@name", newName);
                updateCmd.Parameters.AddWithValue("@id", selectedId);
                updateCmd.ExecuteNonQuery();               // Выполнение UPDATE

                MessageBox.Show("Запись успешно обновлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            selectedId = -1;
            LoadCategory();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)                          // Если ничего не выбрано
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string categoryName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            // Получаем имя категории

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery =
                    "SELECT COUNT(*) FROM Services WHERE Category = @categoryId";
                // Проверяем, используется ли категория в услугах

                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@categoryId", selectedId);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show(
                        "Нельзя удалить эту категорию, так как она используется в других записях!",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить запись: \"{categoryName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;

                string deleteQuery = "DELETE FROM Category WHERE idCategory = @id";
                // SQL-запрос удаления

                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con);
                deleteCmd.Parameters.AddWithValue("@id", selectedId);
                deleteCmd.ExecuteNonQuery();               // Выполняем DELETE

                MessageBox.Show("Запись успешно удалена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            selectedId = -1;
            LoadCategory();
        }
    }
}
