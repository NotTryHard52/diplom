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
                if (textBox1.TextLength < 1 || textBox2.TextLength < 1)
                {
                    MessageBox.Show("Заполните все поля!");
                    return;
                }
                string login = textBox1.Text;
                string password = textBox2.Text;
                string hash_password;

                using (var sh2 = SHA256.Create())
                {
                    var sh2byte = sh2.ComputeHash(Encoding.UTF8.GetBytes(password));
                    hash_password = BitConverter.ToString(sh2byte).Replace("-", "").ToLower();
                }
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT Password, Role, Surname, Name, Lastname " +
                        "FROM users WHERE login = @login", con);
                    cmd.Parameters.AddWithValue("@login", login);

                    MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sda.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Пользователь не найден!");
                        textBox1.Clear();
                        textBox2.Clear();
                        return;
                    }
                    string surname = dt.Rows[0]["Surname"].ToString();
                    string name = dt.Rows[0]["Name"].ToString();
                    string lastname = dt.Rows[0]["Lastname"].ToString();
                    string FIO = $"{surname} {name} {lastname}";
                    using (var sh2 = SHA256.Create())
                    {
                        var sh2byte = sh2.ComputeHash(Encoding.UTF8.GetBytes(password));
                        hash_password = BitConverter.ToString(sh2byte).Replace("-", "").ToLower();
                    }
                    string hashpassword = dt.Rows[0].ItemArray.GetValue(0).ToString();
                    if (hash_password == hashpassword)
                    {
                        if (dt.Rows[0].ItemArray.GetValue(1).ToString() == "1")
                        {
                            Menu admin = new Menu(FIO);
                            this.Hide();
                            admin.ShowDialog();
                            MySqlConnection con3 = new MySqlConnection(connectionString);
                            con.Close();
                            textBox1.Text = "";
                            textBox2.Text = "";
                            this.Close();
                        }
                        if (dt.Rows[0].ItemArray.GetValue(1).ToString() == "2")
                        {
                            Menu_registrator reg = new Menu_registrator(FIO);
                            this.Hide();
                            reg.ShowDialog();
                            this.Close();
                        }
                        if (dt.Rows[0].ItemArray.GetValue(1).ToString() == "3")
                        {
                            Form1 glav = new Form1(FIO);
                            this.Hide();
                            glav.ShowDialog();
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Неверный пароль!");
                        textBox2.Clear();
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            textBox1.Clear();
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
    }
}
