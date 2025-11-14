using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AddDoctor : Form
    {
        string connectionString;
        string selectedPhotoFileName;
        string selectedPhotoFullPath;
        string photoFolder;
        string placeholderPath = Path.Combine(Application.StartupPath, "photo", "not-image.png");
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

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
        string.IsNullOrWhiteSpace(textBox2.Text) ||
        !maskedTextBox1.MaskFull ||
        comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string surname = textBox1.Text.Trim();
            string name = textBox2.Text.Trim();
            string lastname = textBox3.Text.Trim();
            string phone = maskedTextBox1.Text.Trim();
            int specId = Convert.ToInt32(comboBox2.SelectedValue);

            string finalPhotoFileName = null;

            if (!string.IsNullOrEmpty(selectedPhotoFullPath))
            {
                try
                {
                    string destPath = Path.Combine(photoFolder, selectedPhotoFileName);

                    if (File.Exists(destPath))
                    {
                        string nameOnly = Path.GetFileNameWithoutExtension(selectedPhotoFileName);
                        string ext = Path.GetExtension(selectedPhotoFileName);
                        string uniqueName = $"{nameOnly}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";

                        selectedPhotoFileName = uniqueName;
                        destPath = Path.Combine(photoFolder, uniqueName);
                    }

                    File.Copy(selectedPhotoFullPath, destPath, true);
                    finalPhotoFileName = selectedPhotoFileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка копирования фото: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                finalPhotoFileName = "not-image.png";
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Doctors WHERE Phone_number = @phone";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@phone", phone);
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (exists > 0)
                    {
                        MessageBox.Show("Такой номер уже существует!", "Дубликат",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string insertQuery = @"
            INSERT INTO Doctors (Surname, Name, Lastname, Phone_number, Speciality, Photo)
            VALUES (@surname, @name, @lastname, @phone, @spec, @photo);";

                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@surname", surname);
                    insertCmd.Parameters.AddWithValue("@name", name);
                    insertCmd.Parameters.AddWithValue("@lastname", lastname);
                    insertCmd.Parameters.AddWithValue("@phone", phone);
                    insertCmd.Parameters.AddWithValue("@spec", specId);
                    insertCmd.Parameters.AddWithValue("@photo", finalPhotoFileName);

                    insertCmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            maskedTextBox1.Clear();
            comboBox2.SelectedIndex = -1;
            pictureBox1.Image = Image.FromFile(placeholderPath);
            label6.Text = "Фото: нет";

            selectedPhotoFileName = null;
            selectedPhotoFullPath = null;
        }

        private void AddDoctor_Load(object sender, EventArgs e)
        {

            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            photoFolder = Path.Combine(Application.StartupPath, "photo");
            if (!System.IO.Directory.Exists(photoFolder))
                System.IO.Directory.CreateDirectory(photoFolder);
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
                comboBox2.SelectedIndex = -1;
            }
            pictureBox1.Image = Image.FromFile(placeholderPath);
            label6.Text = "Фото: нет";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                ofd.Title = "Выберите новое фото";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Image newImage = Image.FromFile(ofd.FileName);
                        pictureBox1.Image = new Bitmap(newImage);

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
    }
}
