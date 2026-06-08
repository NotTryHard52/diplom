using MySql.Data.MySqlClient;                  
using System;                                      
using System.Data;                               
using System.Drawing;                           
using System.Drawing.Imaging;
using System.IO;                          
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1                      
{
    public partial class AddDoctor : Form          // Класс формы AddDoctor
    {
        string connectionString;                   // Строка подключения к базе
        string selectedPhotoFileName;              // Имя выбранного файла фото
        string selectedPhotoFullPath;              // Полный путь к фото на диске
        string photoFolder;                        // Папка для хранения фото
        string placeholderPath = Path.Combine(Application.StartupPath, "photo", "upload.png"); // Путь к изображению-заглушке
        long maxSize = 1 * 1024 * 1024; // 1 МБ

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

        private Image CompressImage(Image image, long quality = 75L)
        {
            ImageCodecInfo jpgEncoder = ImageCodecInfo.GetImageDecoders()
                .First(c => c.FormatID == ImageFormat.Jpeg.Guid);

            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, jpgEncoder, encoderParameters);
                return Image.FromStream(new MemoryStream(ms.ToArray()));
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||     // Проверка фамилии
                string.IsNullOrWhiteSpace(textBox2.Text) ||      // Проверка имени
                !maskedTextBox1.MaskFull ||                      // Проверка заполнения телефона
                comboBox2.SelectedValue == null)                 // Проверка выбора специальности
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    using (Image img = Image.FromFile(selectedPhotoFullPath))
                    {
                        Image finalImage = img;

                        // если нужно сжатие
                        FileInfo fileInfo = new FileInfo(selectedPhotoFullPath);

                        if (fileInfo.Length > maxSize)
                        {
                            finalImage = CompressImage(img, 60L);
                        }

                        string fileName = Path.GetFileNameWithoutExtension(selectedPhotoFileName) + ".jpg";
                        string destPath = Path.Combine(photoFolder, fileName);
                        // если файл существует — делаем уникальное имя
                        if (File.Exists(destPath))
                        {
                            string nameOnly = Path.GetFileNameWithoutExtension(selectedPhotoFileName);
                            string ext = ".jpg"; // важно: сохраняем как JPG после сжатия
                            string uniqueName = $"{nameOnly}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";

                            selectedPhotoFileName = uniqueName;
                            destPath = Path.Combine(photoFolder, uniqueName);
                        }

                        // сохраняем именно СЖАТОЕ изображение
                        finalImage.Save(destPath, ImageFormat.Jpeg);

                        finalPhotoFileName = fileName;

                        finalImage.Dispose();
                    }
                }
                catch (Exception ex)                                   // Ловим возможные ошибки
                {
                    MessageBox.Show($"Ошибка копирования фото: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                finalPhotoFileName = "";            // Если фото нет — используем заглушку
            }

            try
            {
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
                            MessageBox.Show("Такой номер уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string insertQuery = @"INSERT INTO Doctors (Surname, Name, Lastname, Phone_number, Speciality, Photo)
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

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)                                   // Ловим возможные ошибки при работе с базой
            {
                MessageBox.Show($"Ошибка при добавлении записи: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            textBox1.Clear();                                      // Очищаем поля
            textBox2.Clear();
            textBox3.Clear();
            maskedTextBox1.Clear();
            comboBox2.SelectedIndex = -1;                          // Сбрасываем выбор
            if (File.Exists(placeholderPath))
            {
                pictureBox1.Image = Image.FromFile(placeholderPath);
            }   // Ставим заглушку
            label6.Text = "Фото: нет";

            selectedPhotoFileName = null;                          // Сбрасываем переменные
            selectedPhotoFullPath = null;
        }

        private void AddDoctor_Load(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)                                   // Ловим возможные ошибки при загрузке формы
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void UploadPhoto()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                ofd.Title = "Выберите новое фото";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(ofd.FileName);

                        using (Image originalImage = Image.FromFile(ofd.FileName))
                        {
                            // Проверка 400x400
                            if (originalImage.Width != 400 || originalImage.Height != 400)
                            {
                                MessageBox.Show("Изображение должно быть 400x400 пикселей!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            Image finalImage;

                            // если больше 1MB — сжимаем
                            if (fileInfo.Length > maxSize)
                            {
                                double sizeMb = fileInfo.Length / 1024.0 / 1024.0;

                                DialogResult result = MessageBox.Show(
                                    $"Размер изображения составляет {sizeMb:F2} МБ.\n" +
                                    $"Допустимый размер — не более 1 МБ.\n\n" +
                                    $"Сжать изображение?",
                                    "Превышен размер файла",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);

                                if (result == DialogResult.No)
                                    return;

                                finalImage = CompressImage(originalImage, 60L);

                                // определяем размер после сжатия
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    finalImage.Save(ms, ImageFormat.Jpeg);

                                    double compressedSizeMb = ms.Length / 1024.0 / 1024.0;

                                    MessageBox.Show(
                                        $"Изображение успешно сжато.\n" +
                                        $"Новый размер: {compressedSizeMb:F2} МБ.",
                                        "Сжатие выполнено",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                finalImage = new Bitmap(originalImage);
                            }

                            pictureBox1.Image?.Dispose();
                            pictureBox1.Image = finalImage;
                        }

                        selectedPhotoFullPath = ofd.FileName;
                        selectedPhotoFileName = Path.GetFileName(ofd.FileName);
                        label6.Text = $"Фото: {selectedPhotoFileName}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UploadPhoto();
        }
    }
}
