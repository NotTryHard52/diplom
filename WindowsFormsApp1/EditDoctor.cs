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
    public partial class EditDoctor : Form
    {
        string connectionString;
        int selectedDoctorId;
        string oldSurname;
        string oldName;
        string oldLastname;
        string oldPhone;
        int oldSpecialityId;
        private Image doctorPhoto;
        private string oldPhotoFileName;
        string selectedPhotoFileName;
        string photoFolder;
        string selectedPhotoFullPath;
        public EditDoctor(int doctorId, Image photo)
        {
            InitializeComponent();
            selectedDoctorId = doctorId;
            doctorPhoto = photo;
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
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                !maskedTextBox1.MaskFull ||
                comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newSurname = textBox1.Text.Trim();
            string newName = textBox2.Text.Trim();
            string newLastname = textBox3.Text.Trim();
            string newPhone = maskedTextBox1.Text.Trim();
            int newSpecialityId = Convert.ToInt32(comboBox2.SelectedValue);

            bool changed = newSurname != oldSurname ||
                           newName != oldName ||
                           newLastname != oldLastname ||
                           newPhone != oldPhone ||
                           newSpecialityId != oldSpecialityId || !string.IsNullOrEmpty(selectedPhotoFileName);

            if (!changed)
            {
                MessageBox.Show("Вы не внесли никаких изменений!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string photoFileToSave = oldPhotoFileName;

            if (!string.IsNullOrEmpty(selectedPhotoFileName) && !string.IsNullOrEmpty(selectedPhotoFullPath))
            {
                try
                {
                    if (!System.IO.Directory.Exists(photoFolder)) System.IO.Directory.CreateDirectory(photoFolder);

                    string destPath = Path.Combine(photoFolder, selectedPhotoFileName);

                    if (File.Exists(destPath))
                    {
                        string name = Path.GetFileNameWithoutExtension(selectedPhotoFileName);
                        string ext = Path.GetExtension(selectedPhotoFileName);
                        string uniqueName = $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                        destPath = Path.Combine(photoFolder, uniqueName);
                        selectedPhotoFileName = uniqueName;
                    }

                    File.Copy(selectedPhotoFullPath, destPath, true);
                    photoFileToSave = selectedPhotoFileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении фото: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = @"
                    SELECT COUNT(*) FROM Doctors 
                    WHERE Surname = @surname 
                      AND Name = @name 
                      AND Lastname = @lastname 
                      AND Phone_number = @phone
                      AND idDoctors <> @id;";

                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@surname", newSurname);
                    checkCmd.Parameters.AddWithValue("@name", newName);
                    checkCmd.Parameters.AddWithValue("@lastname", newLastname);
                    checkCmd.Parameters.AddWithValue("@phone", newPhone);
                    checkCmd.Parameters.AddWithValue("@id", selectedDoctorId);

                    int duplicateCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (duplicateCount > 0)
                    {
                        MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string query = @"
                        UPDATE Doctors
                        SET Surname = @surname,
                            Name = @name,
                            Lastname = @lastname,
                            Phone_number = @phone,
                            Speciality = @speciality,
                            Photo = @photo
                        WHERE idDoctors = @id;";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@surname", newSurname);
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@lastname", newLastname);
                    cmd.Parameters.AddWithValue("@phone", newPhone);
                    cmd.Parameters.AddWithValue("@speciality", newSpecialityId);
                    cmd.Parameters.AddWithValue("@id", selectedDoctorId);
                    cmd.Parameters.AddWithValue("@photo", string.IsNullOrEmpty(photoFileToSave) ? (object)DBNull.Value : photoFileToSave);

                    cmd.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void EditDoctor_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            LoadSpecialities();
            LoadDoctorData();

            photoFolder = System.IO.Path.Combine(Application.StartupPath, "photo");
            if (doctorPhoto != null)
            {
                pictureBox1.Image = new Bitmap(doctorPhoto);
            }
            label6.Text = $"Фото: {(string.IsNullOrEmpty(oldPhotoFileName) ? "нет" : oldPhotoFileName)}";
        }
        private void LoadSpecialities()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT idSpeciality, SpecialityName FROM Speciality;";
                MySqlDataAdapter da = new MySqlDataAdapter(query, con);
                DataTable t = new DataTable();
                da.Fill(t);

                comboBox2.DisplayMember = "SpecialityName";
                comboBox2.ValueMember = "idSpeciality";
                comboBox2.DataSource = t;
                comboBox2.SelectedIndex = -1;
            }
        }
        private void LoadDoctorData()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT d.Surname, d.Name, d.Lastname, d.Phone_number, s.idSpeciality, d.Photo
                    FROM Doctors d
                    JOIN Speciality s ON d.Speciality = s.idSpeciality
                    WHERE d.idDoctors = @id;";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", selectedDoctorId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        oldSurname = reader["Surname"].ToString();
                        oldName = reader["Name"].ToString();
                        oldLastname = reader["Lastname"].ToString();
                        oldPhone = reader["Phone_number"].ToString();
                        oldSpecialityId = Convert.ToInt32(reader["idSpeciality"]);
                        oldPhotoFileName = reader["Photo"] == DBNull.Value ? "" : reader["Photo"].ToString();

                        textBox1.Text = oldSurname;
                        textBox2.Text = oldName;
                        textBox3.Text = oldLastname;
                        maskedTextBox1.Text = oldPhone;
                        comboBox2.SelectedValue = oldSpecialityId;
                    }
                }
            }
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
                        FileInfo fileInfo = new FileInfo(ofd.FileName);

                        if (fileInfo.Length > 2 * 1024 * 1024)
                        {
                            MessageBox.Show("Размер изображения не должен превышать 2 МБ!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Image newImage = Image.FromFile(ofd.FileName);
                        pictureBox1.Image = new Bitmap(newImage);

                        selectedPhotoFullPath = ofd.FileName;
                        selectedPhotoFileName = Path.GetFileName(ofd.FileName);
                        label6.Text = $"Фото: {selectedPhotoFileName}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}","Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    } 
}
