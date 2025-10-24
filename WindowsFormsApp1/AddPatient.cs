using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AddPatient : Form
    {
        string connectionString;
        public AddPatient()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian_Hyphen(sender, e);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.OnlyRussian(sender, e);
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.OnlyRussian(sender, e);
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);
        }

        private void AddPatient_Load(object sender, EventArgs e)
        {
            InputLimit.Date(dateTimePicker1);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || 
                string.IsNullOrWhiteSpace(textBox2.Text) ||     
                !maskedTextBox2.MaskFull ||                  
                string.IsNullOrWhiteSpace(textBox4.Text))  
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string surname = textBox1.Text.Trim();
            string name = textBox2.Text.Trim();
            string lastname = textBox3.Text.Trim();
            string phone_number = maskedTextBox2.Text.Trim();
            string policy = textBox4.Text.Trim();
            DateTime birthDate = dateTimePicker1.Value.Date;

            if (policy.Length != 10)
            {
                MessageBox.Show("Полис должен содержать ровно 10 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Connect connect = new Connect();
            connectionString = connect.ConnectDB();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Patients WHERE Number_policy = @policy";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@policy", policy);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Пациент с таким полисом уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string insertQuery = @"
            INSERT INTO Patients (Surname, Name, Lastname, `Date of birth`, Phone_number, Number_policy)
            VALUES (@surname, @name, @lastname, @birthDate, @phone_number, @policy)";

                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@surname", surname);
                    insertCmd.Parameters.AddWithValue("@name", name);
                    insertCmd.Parameters.AddWithValue("@lastname", lastname);
                    insertCmd.Parameters.AddWithValue("@birthDate", birthDate.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@phone_number", phone_number);
                    insertCmd.Parameters.AddWithValue("@policy", policy);

                    insertCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            maskedTextBox2.Clear();
            textBox4.Clear();
        }
    }
}
