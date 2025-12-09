using MySql.Data.MySqlClient;            
using System;                            
using System.Windows.Forms;                  

namespace WindowsFormsApp1                 
{
    public partial class AddPatient : Form    // Класс формы для добавления пациента
    {
        string connectionString;              // Строка подключения к базе данных
        public AddPatient()                   // Конструктор формы
        {
            InitializeComponent();            // Инициализация элементов интерфейса
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian_Hyphen(sender, e);   // Ограничение ввода: русские буквы + дефис
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.OnlyRussian(sender, e);       // Ограничение ввода: только русские буквы
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);           // Ограничение ввода: только цифры
        }

        private void AddPatient_Load(object sender, EventArgs e)
        {
            InputLimit.Date(dateTimePicker1);        // Установка ограничений на дату (например, запрет будущей)
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||    // Проверка: фамилия не пустая
                string.IsNullOrWhiteSpace(textBox2.Text) ||    // Проверка: имя не пустое
                !maskedTextBox2.MaskFull ||                    // Проверка, что номер телефона полностью заполнен
                !maskedTextBox1.MaskFull)                      // Проверка, что полис полностью введён
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);  // Сообщение об ошибке
                return;                                             // Прерывание выполнения метода
            }

            string surname = textBox1.Text.Trim();                // Получение фамилии
            string name = textBox2.Text.Trim();                   // Имени
            string lastname = textBox3.Text.Trim();               // Отчества
            string phone_number = maskedTextBox2.Text.Trim();     // Номера телефона
            string policy = maskedTextBox1.Text.Trim();           // Номера полиса
            DateTime birthDate = dateTimePicker1.Value.Date;      // Даты рождения

            Connect connect = new Connect();                      // Создание объекта подключения
            connectionString = connect.ConnectDB();               // Получение строки подключения

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();                                      // Открытие соединения с БД

                string checkQuery = "SELECT COUNT(*) FROM Patients WHERE Number_policy = @policy";
                // SQL-запрос проверки полиса
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@policy", policy); // Передаём параметр полиса
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar()); // Получаем количество совпадений

                    if (count > 0)                                       // Если пациент уже существует
                    {
                        MessageBox.Show("Пациент с таким полисом уже существует!", "Дубликат",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string insertQuery = @"
            INSERT INTO Patients (Surname, Name, Lastname, Date_birth, Phone_number, Number_policy)
            VALUES (@surname, @name, @lastname, @birthDate, @phone_number, @policy)";
                // SQL-запрос вставки записи

                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@surname", surname);      // Подготовка параметров
                    insertCmd.Parameters.AddWithValue("@name", name);
                    insertCmd.Parameters.AddWithValue("@lastname", lastname);
                    insertCmd.Parameters.AddWithValue("@birthDate", birthDate.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@phone_number", phone_number);
                    insertCmd.Parameters.AddWithValue("@policy", policy);

                    insertCmd.ExecuteNonQuery();                                // Выполнение INSERT
                }

                MessageBox.Show("Запись успешно добавлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);          // Сообщение об успехе
            }

            textBox1.Clear();           // Очистка поля фамилии
            textBox2.Clear();           // Очистка поля имени
            textBox3.Clear();           // Очистка отчества
            maskedTextBox2.Clear();     // Очистка телефона
            maskedTextBox1.Clear();     // Очистка полиса
        }
    }
}
