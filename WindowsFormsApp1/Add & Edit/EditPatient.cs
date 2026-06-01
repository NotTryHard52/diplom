using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class EditPatient : Form
    {
        string connectionString;      // Строка подключения к базе данных
        int selectedPatientId;        // ID редактируемого пациента

        // Старые значения полей пациента для проверки изменений
        string oldSurname;
        string oldName;
        string oldLastname;
        DateTime oldBirthday;
        string oldPhone;
        string oldPolicy;

        public EditPatient(int patientId)
        {
            InitializeComponent();      // Инициализация компонентов формы
            selectedPatientId = patientId; // Сохраняем ID пациента
        }

        // Ограничение ввода фамилии: русские буквы и дефис
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian_Hyphen(sender, e);
        }

        // Ограничение ввода имени: только русские буквы
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.OnlyRussian(sender, e);
        }

        // Ограничение ввода телефона: только цифры
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);
        }

        private void EditPatient_Load(object sender, EventArgs e)
        {
            InputLimit.Date(dateTimePicker1);  // Ограничение ввода даты
            Connect connect = new Connect();
            connectionString = connect.ConnectDB(); // Получаем строку подключения

            LoadPatientData(); // Загружаем данные пациента из БД
        }

        // Метод для загрузки данных выбранного пациента из базы
        private void LoadPatientData()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT * FROM Patients WHERE idPatients = @id;";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", selectedPatientId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Сохраняем старые значения для последующей проверки изменений
                            oldSurname = reader["Surname"].ToString();
                            oldName = reader["Name"].ToString();
                            oldLastname = reader["Lastname"].ToString();
                            oldBirthday = Convert.ToDateTime(reader["Date_birth"]);
                            oldPhone = reader["Phone_number"].ToString();
                            oldPolicy = reader["Number_policy"].ToString();

                            // Заполняем поля формы текущими данными пациента
                            textBox1.Text = oldSurname;
                            textBox2.Text = oldName;
                            textBox3.Text = oldLastname;
                            dateTimePicker1.Value = oldBirthday;
                            maskedTextBox2.Text = oldPhone;
                            maskedTextBox1.Text = oldPolicy;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных пациента: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Сохранение изменений пациента
        private void button6_Click(object sender, EventArgs e)
        {
            // Проверка, что все обязательные поля заполнены
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                !maskedTextBox2.MaskFull ||
                !maskedTextBox1.MaskFull)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем новые значения из формы
            string newSurname = textBox1.Text.Trim();
            string newName = textBox2.Text.Trim();
            string newLastname = textBox3.Text.Trim();
            DateTime newBirthday = dateTimePicker1.Value.Date;
            string newPhone = maskedTextBox2.Text.Trim();
            string newPolicy = maskedTextBox1.Text.Trim();

            // Проверяем, были ли внесены изменения
            bool changed = newSurname != oldSurname ||
                           newName != oldName ||
                           newLastname != oldLastname ||
                           newBirthday != oldBirthday ||
                           newPhone != oldPhone ||
                           newPolicy != oldPolicy;

            if (!changed)
            {
                MessageBox.Show("Вы не внесли никаких изменений!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Подключение к базе данных и обновление записи
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // Проверка на дубликаты по фамилии, имени, отчеству и номеру полиса
                    string checkQuery = @"
                                SELECT COUNT(*) FROM Patients 
                                WHERE Surname = @surname 
                                  AND Name = @name 
                                  AND Lastname = @lastname 
                                  AND Number_policy = @policy 
                                  AND idPatients <> @id;";

                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@surname", newSurname);
                        checkCmd.Parameters.AddWithValue("@name", newName);
                        checkCmd.Parameters.AddWithValue("@lastname", newLastname);
                        checkCmd.Parameters.AddWithValue("@policy", newPolicy);
                        checkCmd.Parameters.AddWithValue("@id", selectedPatientId);

                        int duplicateCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (duplicateCount > 0)
                        {
                            MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Запрос на обновление данных пациента
                    string query = @"
                                    UPDATE Patients
                                    SET Surname = @surname,
                                        Name = @name,
                                        Lastname = @lastname,
                                        Date_birth = @birth,
                                        Phone_number = @phone,
                                        Number_policy = @policy
                                    WHERE idPatients = @id;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@surname", newSurname);
                        cmd.Parameters.AddWithValue("@name", newName);
                        cmd.Parameters.AddWithValue("@lastname", newLastname);
                        cmd.Parameters.AddWithValue("@birth", newBirthday);
                        cmd.Parameters.AddWithValue("@phone", newPhone);
                        cmd.Parameters.AddWithValue("@policy", newPolicy);
                        cmd.Parameters.AddWithValue("@id", selectedPatientId);

                        cmd.ExecuteNonQuery(); // Выполняем обновление
                    }
                }

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Закрываем форму после сохранения
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных пациента: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
