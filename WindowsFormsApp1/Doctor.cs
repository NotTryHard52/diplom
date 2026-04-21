using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Doctor : Form
    {
        string connectionString;               // Строка подключения к БД
        DataTable doctorsTable;                // Таблица для хранения данных врачей
        int selectedId = -1;                   // ID выбранного врача (-1 = не выбран)

        public Doctor()
        {
            InitializeComponent();             // Инициализация компонентов формы
        }

        private void Doctor_Load(object sender, EventArgs e)
        {
            FillSpecialties();                 // Заполнение списка специальностей
            comboBox2.SelectedIndex = 0;       // Значение сортировки по умолчанию
            comboBox1.SelectedIndex = 0;       // Фильтр специальностей по умолчанию
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged; // Обработчик смены фильтра
            textBox5.TextChanged += textBox5_TextChanged;                    // Обработчик ввода поиска
            LoadDoctor();                      // Загрузка списка врачей
            var hoverEffect = new HoverDataGridView(dataGridView1); // Наведение на строки (эффект подсветки)
            dataGridView1.Visible = false;
            button3.Visible = false;
            button2.Visible = false;

        }

        private void LoadDoctor()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB(); // Получаем строку подключения

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();                         // Открытие соединения

                doctorsTable = new DataTable();     // Таблица для загрузки данных

                // SQL-запрос — выбираем врачей + название специальности
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT d.idDoctors, d.Surname, d.Name, d.Lastname, d.Phone_number, d.Photo, s.SpecialityName " +
                    "FROM Doctors d JOIN Speciality s ON d.Speciality = s.idSpeciality;", con);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd); // Адаптер данных
                da.Fill(doctorsTable);                           // Заполняем DataTable
            }

            LoadDoctorPhotos(doctorsTable);       // Загружаем фото врачей в таблицу

            DisplayCards(doctorsTable); // Привязка таблицы к DataGridView

            //dataGridView1.Columns["idDoctors"].Visible = false; // Скрываем ID
            //dataGridView1.Columns["Surname"].HeaderText = "Фамилия";
            //dataGridView1.Columns["Name"].HeaderText = "Имя";
            //dataGridView1.Columns["Lastname"].HeaderText = "Отчество";
            //dataGridView1.Columns["Phone_number"].HeaderText = "Телефон";
            //dataGridView1.Columns["SpecialityName"].HeaderText = "Специальность";
            //dataGridView1.Columns["Photo"].Visible = false;      // Скрываем текстовое имя фото

            //label9.Text = $"Количество записей: {doctorsTable.Rows.Count}"; // Вывод количества
            groupBox2.Text = $"Количество записей: {doctorsTable.Rows.Count}";

            // Колонка с изображениями
            //DataGridViewImageColumn imgCol = (DataGridViewImageColumn)dataGridView1.Columns["Фото"];
            //imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom; // Масштабирование фото

            //dataGridView1.RowTemplate.Height = 100; // Высота строки из-за фото
            //dataGridView1.Refresh();                // Перерисовка
        }

        private void DisplayCards(DataTable table)
        {
            flowLayoutPanel1.Controls.Clear();

            foreach (DataRow row in table.Rows)
            {
                DoctorCard card = new DoctorCard();

                int id = Convert.ToInt32(row["idDoctors"]);

                string fio = $"{row["Surname"]} {row["Name"]} {row["Lastname"]}";
                string phone = row["Phone_number"].ToString();
                string spec = row["SpecialityName"].ToString();
                Image photo = row["Фото"] as Image;

                card.SetData(id, fio, phone, spec, photo);
                card.EditClicked += Card_EditClicked;
                card.DeleteClicked += Card_DeleteClicked;

                flowLayoutPanel1.Controls.Add(card);
            }

            groupBox2.Text = $"Количество записей: {table.Rows.Count}";
        }

        private void LoadDoctorPhotos(DataTable table)
        {
            if (table == null) return;              // Проверка на null

            if (!table.Columns.Contains("Фото"))    // Если нет колонки "Фото"
                table.Columns.Add("Фото", typeof(Image)); // Создаем колонку с типом Image

            string photoFolder = Path.Combine(Application.StartupPath, "photo"); // Папка с фото

            foreach (DataRow row in table.Rows)
            {
                string fileName = "not-image.png";  // Фото по умолчанию

                if (row["Photo"] != DBNull.Value && !string.IsNullOrEmpty(row["Photo"].ToString()))
                    fileName = row["Photo"].ToString(); // Имя файла из БД

                string photoPath = Path.Combine(photoFolder, fileName); // Полный путь

                if (!File.Exists(photoPath)) // Если фото не существует
                    photoPath = Path.Combine(photoFolder, "not-image.png");

                try
                {
                    // Загружаем изображение через FileStream, чтобы избежать блокировок
                    using (FileStream fs = new FileStream(photoPath, FileMode.Open, FileAccess.Read))
                    {
                        using (var tempImg = Image.FromStream(fs))
                        {
                            row["Фото"] = new Bitmap(tempImg); // Добавляем фото в таблицу
                        }
                    }
                }
                catch
                {
                    row["Фото"] = new Bitmap(100, 100); // В случае ошибки — пустая картинка
                }
            }
        }

        private void FillSpecialties()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB(); // Получаем строку подключения

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string query = "SELECT DISTINCT SpecialityName FROM speciality;"; // Список специальностей
                MySqlCommand cmd = new MySqlCommand(query, con);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    comboBox1.Items.Clear();        // Очищаем список
                    comboBox1.Items.Add("Все специальности");     // Пункт для отображения всех врачей

                    while (reader.Read())
                    {
                        string specialty = reader["SpecialityName"].ToString();
                        comboBox1.Items.Add(specialty); // Добавляем специальность
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)     // Если не выбрана строка
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int doctorId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["idDoctors"].Value); // Получаем ID врача

            Image photo = null;                             // Фото врача
            if (dataGridView1.SelectedRows[0].Cells["Фото"].Value is Image img)
            {
                photo = new Bitmap(img);                    // Делаем копию
            }

            EditDoctor editForm = new EditDoctor(doctorId, photo); // Создаем форму редактирования
            editForm.ShowDialog();                                  // Открываем её

            LoadDoctor();                                           // Обновляем таблицу после закрытия
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort(); // Сортировка
        }

        private void ApplyFilterAndSort()
        {
            if (doctorsTable == null) return;

            string filterExpr = ""; // Строка фильтра

            // Фильтр по специальности
            string selectedSpecialty = comboBox1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedSpecialty) && selectedSpecialty != "Все специальности")
            {
                filterExpr = $"SpecialityName = '{selectedSpecialty.Replace("'", "''")}'";
            }

            // Фильтр по фамилии
            string searchText = textBox5.Text.Trim().Replace("'", "''");
            if (!string.IsNullOrEmpty(searchText))
            {
                if (!string.IsNullOrEmpty(filterExpr))
                    filterExpr += " AND ";

                filterExpr += $"Surname LIKE '%{searchText}%'";
            }

            // Сортировка
            string sortExpr = "";
            if (comboBox2.SelectedIndex == 1)
                sortExpr = "Surname ASC";
            else if (comboBox2.SelectedIndex == 2)
                sortExpr = "Surname DESC";

            DataView dv = doctorsTable.DefaultView; // Представление таблицы
            dv.RowFilter = filterExpr;              // Применяем фильтр
            dv.Sort = sortExpr;                     // Применяем сортировку

            DisplayCards(dv.ToTable());

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort(); // Обновляем отображение при смене специальности
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort(); // Фильтр по фамилии
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddDoctor ad = new AddDoctor(); // Форма добавления врача
            ad.ShowDialog();                // Открываем модально

            LoadDoctor();                   // Перезагружаем таблицу
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian_Hyphen(sender, e); // Ограничиваем ввод ФИО и дефисов
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0; // Сброс сортировки
            comboBox1.SelectedIndex = 0; // Сброс фильтра
            textBox5.Text = "";          // Очистка поиска
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1) // Если врач не выбран
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Проверяем, используется ли врач в расписании
                string checkQuery = "SELECT COUNT(*) FROM Schedule WHERE idDoctor = @doctorId";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@doctorId", selectedId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Нельзя удалить этого врача, так как он используется в расписании!",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                DialogResult result = MessageBox.Show(
                    "Вы уверены, что хотите удалить запись?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No) return;

                // Удаление врача
                string deleteQuery = "DELETE FROM Doctors WHERE idDoctors = @id";
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@id", selectedId);
                    deleteCmd.ExecuteNonQuery();

                    MessageBox.Show("Запись успешно удалена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            selectedId = -1; // Сбрасываем выбор
            LoadDoctor();    // Перезагружаем список
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Если нажата строка, а не заголовок
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedId = Convert.ToInt32(row.Cells["idDoctors"].Value); // Сохраняем выбранный ID
            }
        }

        private void Card_EditClicked(object sender, int id)
        {
            Image photo = null;

            DataRow[] rows = doctorsTable.Select($"idDoctors = {id}");
            if (rows.Length > 0 && rows[0]["Фото"] is Image img)
            {
                photo = new Bitmap(img);
            }

            EditDoctor editForm = new EditDoctor(id, photo);
            editForm.ShowDialog();

            LoadDoctor();
        }

        private void Card_DeleteClicked(object sender, int id)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Schedule WHERE idDoctor = @doctorId";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@doctorId", id);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Нельзя удалить врача, он используется в расписании!");
                        return;
                    }
                }

                if (MessageBox.Show("Удалить врача?", "Подтверждение",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

                string deleteQuery = "DELETE FROM Doctors WHERE idDoctors = @id";
                using (MySqlCommand cmd = new MySqlCommand(deleteQuery, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            LoadDoctor();
        }
    }
}
