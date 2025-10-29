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
    public partial class EditPatient : Form
    {
        string connectionString;
        int selectedPatientId;

        string oldSurname;
        string oldName;
        string oldLastname;
        DateTime oldBirthday;
        string oldPhone;
        string oldPolicy;
        public EditPatient(int patientId)
        {
            InitializeComponent();
            selectedPatientId = patientId;
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

        private void EditPatient_Load(object sender, EventArgs e)
        {
            InputLimit.Date(dateTimePicker1);
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();

            InputLimit.Date(dateTimePicker1);
            LoadPatientData();
        }
        private void LoadPatientData()
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
                        oldSurname = reader["Surname"].ToString();
                        oldName = reader["Name"].ToString();
                        oldLastname = reader["Lastname"].ToString();
                        oldBirthday = Convert.ToDateTime(reader["Date_birth"]);
                        oldPhone = reader["Phone_number"].ToString();
                        oldPolicy = reader["Number_policy"].ToString();

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

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                !maskedTextBox2.MaskFull ||
                !maskedTextBox1.MaskFull)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newSurname = textBox1.Text.Trim();
            string newName = textBox2.Text.Trim();
            string newLastname = textBox3.Text.Trim();
            DateTime newBirthday = dateTimePicker1.Value.Date;
            string newPhone = maskedTextBox2.Text.Trim();
            string newPolicy = maskedTextBox1.Text.Trim();

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

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

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

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
