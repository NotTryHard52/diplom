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
    public partial class AddDoctor : Form
    {
        string connectionString;
        public AddDoctor()
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

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(maskedTextBox1.Text) || comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string surname = textBox1.Text.Trim();
            string name = textBox2.Text.Trim();
            string lastname = textBox3.Text.Trim();
            string phone_number = maskedTextBox1.Text.Trim();
            int specId = Convert.ToInt32(comboBox2.SelectedValue);

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Doctors WHERE Phone_number = @phone_number";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@phone_number", phone_number);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string insertQuery = @"
            INSERT INTO Doctors (Surname, Name, Lastname, Phone_number, Speciality)
            VALUES (@surname, @name, @lastname, @Phone_number, @Speciality)";

                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@surname", surname);
                    insertCmd.Parameters.AddWithValue("@name", name);
                    insertCmd.Parameters.AddWithValue("@lastname", lastname);
                    insertCmd.Parameters.AddWithValue("@Phone_number", phone_number);
                    insertCmd.Parameters.AddWithValue("@Speciality", specId);

                    insertCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            maskedTextBox1.Clear();
            comboBox2.SelectedIndex = -1;
        }

        private void AddDoctor_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                string specQuery = "SELECT idSpeciality, SpecialityName FROM Speciality;";
                MySqlCommand specCmd = new MySqlCommand(specQuery, con);
                MySqlDataAdapter specDa = new MySqlDataAdapter(specCmd);
                DataTable specTable = new DataTable();
                specDa.Fill(specTable);

                comboBox2.DisplayMember = "SpecialityName";
                comboBox2.ValueMember = "idSpeciality";
                comboBox2.DataSource = specTable;
            }
        }
    }
}
