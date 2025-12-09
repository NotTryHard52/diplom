using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Speciality : Form
    {
        string connectionString; // Строка подключения к базе данных
        int selectedId = -1; // Id выбранной записи, используется для редактирования/удаления

        // Конструктор формы
        public Speciality()
        {
            InitializeComponent();
        }

        // Событие загрузки формы
        private void Speciality_Load(object sender, EventArgs e)
        {
            // Получаем строку подключения через класс Connect
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();

            // Загружаем данные специальностей в DataGridView
            LoadSpeciality();

            // Добавляем эффект подсветки строк при наведении мыши
            var hoverEffect = new HoverDataGridView(dataGridView1);
        }

        // Метод для загрузки всех специальностей
        private void LoadSpeciality()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                DataTable t = new DataTable();

                // Выбираем все записи из таблицы Speciality
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Speciality;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);

                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;

                // Скрываем колонку с id
                dataGridView1.Columns[0].Visible = false;

                // Переименовываем колонку с названием специальности
                dataGridView1.Columns[1].HeaderText = "Наименование";

                label2.Text = $"Количество записей: {t.Rows.Count}";
            }
        }

        // Ограничение ввода в текстовое поле только русскими буквами
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian(sender, e);
        }

        // Добавление новой специальности
        private void button1_Click(object sender, EventArgs e)
        {
            string specialityName = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(specialityName))
            {
                MessageBox.Show("Поле не должно быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка на дубликат
                string checkQuery = "SELECT COUNT(*) FROM Speciality WHERE SpecialityName = @name";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@name", specialityName);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Вставка новой записи
                string insertQuery = "INSERT INTO Speciality (SpecialityName) VALUES (@name)";
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@name", specialityName);
                insertCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            LoadSpeciality(); // Перезагружаем список специальностей
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

        // Редактирование выбранной специальности
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
                string checkQuery = "SELECT COUNT(*) FROM Speciality WHERE SpecialityName = @name AND IdSpeciality != @id";
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
                string updateQuery = "UPDATE Speciality SET SpecialityName = @name WHERE IdSpeciality = @id";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, con);
                updateCmd.Parameters.AddWithValue("@name", newName);
                updateCmd.Parameters.AddWithValue("@id", selectedId);
                updateCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            selectedId = -1;
            LoadSpeciality();
        }

        // Удаление выбранной специальности
        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string SpecName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверка, используется ли эта специальность у врачей
                string checkQuery = "SELECT COUNT(*) FROM Doctors WHERE Speciality = @specialityId";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@specialityId", selectedId);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show(
                        "Нельзя удалить эту специальность, так как она используется в других записях!",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить запись: \"{SpecName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                    return;

                // Удаление записи
                string deleteQuery = "DELETE FROM Speciality WHERE idSpeciality = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con);
                deleteCmd.Parameters.AddWithValue("@id", selectedId);
                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            selectedId = -1;
            LoadSpeciality();
        }
    }
}
