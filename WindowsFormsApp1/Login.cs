using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Login : Form
    {
        string connectionString;
        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Connect connect = new Connect();
                connectionString = connect.ConnectDB();

                if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("Заполните все поля!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string login = textBox1.Text;
                string password = textBox2.Text;

                // Хешируем введённый пароль
                string hash_password;
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                    hash_password = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT idUsers, Password, Role, Surname, Name, Lastname " +
                        "FROM users WHERE login = @login", con);
                    cmd.Parameters.AddWithValue("@login", login);

                    MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sda.Fill(dt);

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

                    if (hash_password != dbPasswordHash)
                    {
                        MessageBox.Show("Неверный пароль!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        textBox2.Clear();
                        return;
                    }

                    this.Hide();
                    if (role == "1")
                    {
                        Menu admin = new Menu(FIO, userId);
                        admin.ShowDialog();
                    }
                    else if (role == "2")
                    {
                        Menu_registrator reg = new Menu_registrator(FIO, userId);
                        reg.ShowDialog();
                    }
                    else if (role == "3")
                    {
                        Form1 glav = new Form1(FIO);
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

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Settingscs sett = new Settingscs();
            this.Hide();
            sett.ShowDialog();
            this.Show();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectNoDB();
            MySqlConnection con = new MySqlConnection(connectionString);

            try
            {
                con.Open();
            }
            catch (Exception)
            {
                DialogResult res = MessageBox.Show($"Ошибка подключения к {connectionString}\nНастроить подключение?", "Ошибка подключения", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                Settingscs error = new Settingscs();

                if (res == DialogResult.Yes)
                {
                    error.ShowDialog();
                }
                else
                    Application.Exit();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !checkBox1.Checked;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.English_Symbol(sender, e);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.English_Symbol(sender, e);
        }
    }
}
