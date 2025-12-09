using MySql.Data.MySqlClient;                  
using System;                                      
using System.Data;                               
using System.Drawing;                           
using System.IO;                          
using System.Windows.Forms;                        

namespace WindowsFormsApp1                      
{
    public partial class AddDoctor : Form          // Класс формы AddDoctor
    {
        string connectionString;                   // Строка подключения к базе
        string selectedPhotoFileName;              // Имя выбранного файла фото
        string selectedPhotoFullPath;              // Полный путь к фото на диске
        string photoFolder;                        // Папка для хранения фото
        string placeholderPath = Path.Combine(Application.StartupPath, "photo", "not-image.png");
        // Путь к изображению-заглушке

        public AddDoctor()                         // Конструктор формы
        {
            InitializeComponent();                 // Создание и инициализация элементов формы
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian_Hyphen(sender, e);  // Ограничиваем ввод — только русский + дефис
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.OnlyRussian(sender, e);     // Ограничиваем ввод — только русский язык
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||     // Проверка фамилии
                string.IsNullOrWhiteSpace(textBox2.Text) ||      // Проверка имени
                !maskedTextBox1.MaskFull ||                      // Проверка заполнения телефона
                comboBox2.SelectedValue == null)                 // Проверка выбора специальности
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;                                          // Прерываем выполнение
            }

            string surname = textBox1.Text.Trim();               // Получаем фамилию
            string name = textBox2.Text.Trim();                  // Имя
            string lastname = textBox3.Text.Trim();              // Отчество
            string phone = maskedTextBox1.Text.Trim();           // Телефон
            int specId = Convert.ToInt32(comboBox2.SelectedValue);  // ID специальности

            string finalPhotoFileName = null;                    // Имя файла, которое попадёт в базу

            if (!string.IsNullOrEmpty(selectedPhotoFullPath))    // Если фото выбрано
            {
                try
                {
                    string destPath = Path.Combine(photoFolder, selectedPhotoFileName);
                    // Путь назначения

                    if (File.Exists(destPath))                   // Проверяем, существует ли файл
                    {
                        string nameOnly = Path.GetFileNameWithoutExtension(selectedPhotoFileName);
                        // Имя без расширения
                        string ext = Path.GetExtension(selectedPhotoFileName);
                        // Расширение
                        string uniqueName = $"{nameOnly}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                        // Создаём уникальное имя

                        selectedPhotoFileName = uniqueName;
                        destPath = Path.Combine(photoFolder, uniqueName);
                    }

                    File.Copy(selectedPhotoFullPath, destPath, true);  // Копируем файл в папку
                    finalPhotoFileName = selectedPhotoFileName;        // Сохраняем имя для базы
                }
                catch (Exception ex)                                   // Ловим возможные ошибки
                {
                    MessageBox.Show($"Ошибка копирования фото: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                finalPhotoFileName = "not-image.png";            // Если фото нет — используем заглушку
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();                                      // Открываем соединение

                string checkQuery = "SELECT COUNT(*) FROM Doctors WHERE Phone_number = @phone";
                // Запрос проверки дубликата телефона
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@phone", phone);  // Передаём параметр
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar()); // Получаем количество записей
                    if (exists > 0)                                     // Если телефон найден
                    {
                        MessageBox.Show("Такой номер уже существует!", "Дубликат",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string insertQuery = @"
            INSERT INTO Doctors (Surname, Name, Lastname, Phone_number, Speciality, Photo)
            VALUES (@surname, @name, @lastname, @phone, @spec, @photo);";
                // SQL-запрос добавления врача

                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@surname", surname);   // Параметры SQL
                    insertCmd.Parameters.AddWithValue("@name", name);
                    insertCmd.Parameters.AddWithValue("@lastname", lastname);
                    insertCmd.Parameters.AddWithValue("@phone", phone);
                    insertCmd.Parameters.AddWithValue("@spec", specId);
                    insertCmd.Parameters.AddWithValue("@photo", finalPhotoFileName);

                    insertCmd.ExecuteNonQuery();                  // Выполнение INSERT
                }
            }

            MessageBox.Show("Запись успешно добавлена!", "Успех",   // Сообщение об успехе
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            textBox1.Clear();                                      // Очищаем поля
            textBox2.Clear();
            textBox3.Clear();
            maskedTextBox1.Clear();
            comboBox2.SelectedIndex = -1;                          // Сбрасываем выбор
            pictureBox1.Image = Image.FromFile(placeholderPath);   // Ставим заглушку
            label6.Text = "Фото: нет";

            selectedPhotoFileName = null;                          // Сбрасываем переменные
            selectedPhotoFullPath = null;
        }

        private void AddDoctor_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();                       // Создаём объект для получения строки подключения
            connectionString = connect.ConnectDB();                // Получаем строку подключения
            photoFolder = Path.Combine(Application.StartupPath, "photo");
            // Путь к папке фото

            if (!System.IO.Directory.Exists(photoFolder))          // Если папка не существует —
                System.IO.Directory.CreateDirectory(photoFolder);  // создаём её

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                string specQuery = "SELECT idSpeciality, SpecialityName FROM Speciality;";
                // SQL для загрузки списка специальностей
                MySqlCommand specCmd = new MySqlCommand(specQuery, con);
                MySqlDataAdapter specDa = new MySqlDataAdapter(specCmd);
                DataTable specTable = new DataTable();            // Таблица для данных
                specDa.Fill(specTable);                           // Заполняем таблицу из базы

                comboBox2.DisplayMember = "SpecialityName";       // Что отображается
                comboBox2.ValueMember = "idSpeciality";           // Что является значением
                comboBox2.DataSource = specTable;                 // Привязываем таблицу к ComboBox
                comboBox2.SelectedIndex = -1;                     // Сбрасываем выбор
            }

            pictureBox1.Image = Image.FromFile(placeholderPath);  // Устанавливаем изображение-заглушку
            label6.Text = "Фото: нет";                            // Подпись
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())     // Окно выбора файла
            {
                ofd.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                // Разрешённые форматы
                ofd.Title = "Выберите новое фото";

                if (ofd.ShowDialog() == DialogResult.OK)           // Если пользователь выбрал файл
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(ofd.FileName); // Получаем сведения о файле

                        if (fileInfo.Length > 2 * 1024 * 1024)      // Проверяем размер > 2 МБ
                        {
                            MessageBox.Show("Размер изображения не должен превышать 2 МБ!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Image newImage = Image.FromFile(ofd.FileName);  // Загружаем изображение
                        pictureBox1.Image = new Bitmap(newImage);       // Отображаем в PictureBox

                        selectedPhotoFullPath = ofd.FileName;           // Запоминаем путь
                        selectedPhotoFileName = Path.GetFileName(ofd.FileName);
                        label6.Text = $"Фото: {selectedPhotoFileName}"; // Обновляем подпись
                    }
                    catch (Exception ex)                                // Обработка ошибок
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
