using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Login : Form
    {
        // Строка подключения к базе данных
        string connectionString;

        public Login()
        {
            InitializeComponent();
        }

        // Кнопка "Войти"
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем объект для подключения
                Connect connect = new Connect();
                connectionString = connect.ConnectDB();

                // Проверка на заполнение полей логин и пароль
                if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("Заполните все поля!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string login = textBox1.Text;
                string password = textBox2.Text;

                // Хешируем введённый пароль с помощью SHA256
                string hash_password;
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                    hash_password = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // Запрос на получение данных пользователя по логину
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT idUsers, Password, Role, Surname, Name, Lastname " +
                        "FROM users WHERE login = @login", con);
                    cmd.Parameters.AddWithValue("@login", login);

                    MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sda.Fill(dt);

                    // Если пользователь не найден
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Пользователь не найден!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        textBox1.Clear();
                        textBox2.Clear();
                        return;
                    }

                    DataRow userRow = dt.Rows[0];

                    int userId = Convert.ToInt32(userRow["idUsers"]);
                    string dbPasswordHash = userRow["Password"].ToString();
                    string role = userRow["Role"].ToString();
                    string FIO = $"{userRow["Surname"]} {userRow["Name"]} {userRow["Lastname"]}";

                    // Проверка пароля
                    if (hash_password != dbPasswordHash)
                    {
                        MessageBox.Show("Неверный пароль!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        textBox2.Clear();
                        return;
                    }

                    // Успешный вход — открытие формы в зависимости от роли пользователя
                    this.Hide();
                    if (role == "1") // Администратор
                    {
                        Menu admin = new Menu(FIO, userId, role);
                        admin.ShowDialog();
                    }
                    else if (role == "2") // Регистратор
                    {
                        Menu_registrator reg = new Menu_registrator(FIO, userId, role);
                        reg.ShowDialog();
                    }
                    else if (role == "3") // Главный врач
                    {
                        Form1 glav = new Form1(FIO, userId, role);
                        glav.ShowDialog();
                    }
                    this.Close();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Кнопка "Выход"
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Клик по иконке настроек
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Settingscs sett = new Settingscs();
            this.Hide();
            sett.ShowDialog();
            this.Show();
        }

        // Загрузка формы Login
        private void Login_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectNoDB(); // Подключение без указания базы данных
            MySqlConnection con = new MySqlConnection(connectionString);

            try
            {
                con.Open();
            }
            catch (Exception)
            {
                // Ошибка подключения — предложить настройки
                DialogResult res = MessageBox.Show(
                    $"Ошибка подключения к {connectionString}\nНастроить подключение?",
                    "Ошибка подключения",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                );

                Settingscs error = new Settingscs();

                if (res == DialogResult.Yes)
                    error.ShowDialog();
                else
                    Application.Exit();
            }
        }

        // Чекбокс "Показать пароль"
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !checkBox1.Checked;
        }

        // Ограничение ввода в поле логина — только английские символы
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.English_Symbol(sender, e);
        }
    }
}
