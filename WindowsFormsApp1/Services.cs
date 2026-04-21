using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Services : Form
    {
        string connectionString; // Строка подключения к БД
        int selectedId; // ID выбранной записи для редактирования или удаления
        public int SelectedServiceId { get; private set; } // ID выбранной услуги для передачи в другую форму
        public string SelectedServiceName { get; private set; } // Название выбранной услуги
        public decimal SelectedServicePrice { get; private set; } // Цена выбранной услуги
        private bool openedFromTalon = false; // Флаг, открыта ли форма для выбора услуги из талона

        public Services(bool fromTalon = false)
        {
            InitializeComponent();
            openedFromTalon = fromTalon;

            // Кнопка выбора услуги видна только если форма открыта из талона
            button4.Visible = openedFromTalon;
        }

        // Загрузка формы
        private void Services_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB(); // Получаем строку подключения
            LoadServices(); // Загружаем услуги в DataGridView
            var hoverEffect = new HoverDataGridView(dataGridView1); // Подсветка строки при наведении
        }

        // Метод загрузки данных из таблицы Services
        private void LoadServices()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Загружаем услуги вместе с категорией
                DataTable t = new DataTable();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT s.idServices, s.Name, s.Price, c.Name AS Category " +
                    "FROM Services s JOIN Category c ON s.Category = c.idCategory;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);

                // Настройки DataGridView
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;
                dataGridView1.Columns[0].Visible = false; // Скрываем ID
                dataGridView1.Columns[1].HeaderText = "Наименование";
                dataGridView1.Columns[2].HeaderText = "Цена";
                dataGridView1.Columns[3].HeaderText = "Категория";

                groupBox1.Text = $"Количество записей: {t.Rows.Count}";

                // Загружаем категории для ComboBox
                string categoryQuery = "SELECT idCategory, Name FROM Category;";
                MySqlCommand categoryCmd = new MySqlCommand(categoryQuery, con);
                MySqlDataAdapter categoryDa = new MySqlDataAdapter(categoryCmd);
                DataTable categoryTable = new DataTable();
                categoryDa.Fill(categoryTable);

                comboBox1.DisplayMember = "Name"; // Отображаемое поле
                comboBox1.ValueMember = "idCategory"; // Значение
                comboBox1.DataSource = categoryTable;
                comboBox1.SelectedIndex = -1; // Сброс выбора
            }
        }

        // Ограничение ввода: только русские буквы
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian(sender, e);
        }

        // Ограничение ввода: только числа
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);
        }

        // Добавление новой услуги
        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка, чтобы все поля были заполнены
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка корректности цены
            if (!decimal.TryParse(textBox2.Text.Trim(), out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string serviceName = textBox1.Text.Trim();
            int categoryId = Convert.ToInt32(comboBox1.SelectedValue);

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Проверка на дубликат
                    string checkQuery = "SELECT COUNT(*) FROM Services WHERE Name = @name";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@name", serviceName);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Вставка новой услуги
                    string insertQuery = "INSERT INTO Services (Name, Price, Category) VALUES (@name, @price, @category)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                    {
                        insertCmd.Parameters.AddWithValue("@name", serviceName);
                        insertCmd.Parameters.AddWithValue("@price", price);
                        insertCmd.Parameters.AddWithValue("@category", categoryId);
                        insertCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadServices(); // Обновляем таблицу
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении услуги:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Сброс полей после добавления
            textBox1.Clear();
            textBox2.Clear();
            comboBox1.SelectedIndex = -1;
        }

        // Обработка выбора строки в DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                selectedId = Convert.ToInt32(row.Cells["idServices"].Value);
                textBox1.Text = row.Cells["Name"].Value.ToString();
                textBox2.Text = Convert.ToInt32(row.Cells["Price"].Value).ToString();
                string categoryName = row.Cells["Category"].Value.ToString();
                comboBox1.SelectedIndex = comboBox1.FindStringExact(categoryName);
            }
        }

        // Редактирование выбранной услуги
        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = textBox1.Text.Trim();
            string priceText = textBox2.Text.Trim();

            // Проверка заполненности полей
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priceText) || comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка корректности цены
            if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int categoryId = Convert.ToInt32(comboBox1.SelectedValue);

            // Проверка, были ли изменения
            DataGridViewRow currentRow = dataGridView1.SelectedRows.Count > 0 ? dataGridView1.SelectedRows[0] : null;
            if (currentRow != null)
            {
                string currentName = currentRow.Cells["Name"].Value.ToString();
                string currentPrice = currentRow.Cells["Price"].Value.ToString();
                string currentCategory = currentRow.Cells["Category"].Value.ToString();
                string newCategory = comboBox1.Text;

                bool changed = name != currentName || priceText != currentPrice || newCategory != currentCategory;

                if (!changed)
                {
                    MessageBox.Show("Вы не внесли никаких изменений!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка на дубликат при редактировании
                string checkQuery = "SELECT COUNT(*) FROM Services WHERE Name = @name AND idServices <> @id";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@name", name);
                    checkCmd.Parameters.AddWithValue("@id", selectedId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Обновление записи
                string updateQuery = @"UPDATE Services 
                                       SET Name = @name, Price = @price, Category = @category 
                                       WHERE idServices = @id";
                using (MySqlCommand cmd = new MySqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@category", categoryId);
                    cmd.Parameters.AddWithValue("@id", selectedId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadServices();

                // Сброс полей
                selectedId = -1;
                textBox1.Clear();
                textBox2.Clear();
                comboBox1.SelectedIndex = -1;
            }
        }

        // Выбор услуги для талона (если форма открыта из Talon)
        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите услугу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dataGridView1.SelectedRows[0];
            SelectedServiceId = Convert.ToInt32(row.Cells["idServices"].Value);
            SelectedServiceName = row.Cells["Name"].Value.ToString();
            SelectedServicePrice = Convert.ToDecimal(row.Cells["Price"].Value);

            this.DialogResult = DialogResult.OK;
            this.Close(); // Закрываем форму после выбора услуги
        }

        // Удаление выбранной услуги
        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ServicesName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            // Подтверждение удаления
            DialogResult result = MessageBox.Show(
                $"Вы уверены, что хотите удалить запись: \"{ServicesName}\"?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
                return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка связей с таблицей OrderServices
                string checkQuery = "SELECT COUNT(*) FROM OrderServices WHERE ServicesId = @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@id", selectedId);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show(
                        "Невозможно удалить запись, так как существуют связанные данные в таблице талонов!",
                        "Ошибка удаления",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // Удаление услуги
                string deleteQuery = "DELETE FROM Services WHERE idServices = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con);
                deleteCmd.Parameters.AddWithValue("@id", selectedId);

                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Сброс полей
            textBox1.Clear();
            textBox2.Clear();
            comboBox1.SelectedIndex = -1;
            selectedId = -1;
            LoadServices();
        }
    }
}
