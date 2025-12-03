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
    public partial class Doctor : Form
    {
        string connectionString;
        DataTable doctorsTable;
        int selectedId = -1;

        public Doctor()
        {
            InitializeComponent();
        }

        private void Doctor_Load(object sender, EventArgs e)
        {
            FillSpecialties();
            comboBox2.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            textBox5.TextChanged += textBox5_TextChanged;
            LoadDoctor();
            var hoverEffect = new HoverDataGridView(dataGridView1);
        }
        private void LoadDoctor()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                doctorsTable = new DataTable();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT d.idDoctors, d.Surname, d.Name, d.Lastname, d.Phone_number, d.Photo, s.SpecialityName " +
                    "FROM Doctors d JOIN Speciality s ON d.Speciality = s.idSpeciality;", con);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(doctorsTable);

            }
            LoadDoctorPhotos(doctorsTable);
            dataGridView1.DataSource = doctorsTable;

            dataGridView1.Columns["idDoctors"].Visible = false;
            dataGridView1.Columns["Surname"].HeaderText = "Фамилия";
            dataGridView1.Columns["Name"].HeaderText = "Имя";
            dataGridView1.Columns["Lastname"].HeaderText = "Отчество";
            dataGridView1.Columns["Phone_number"].HeaderText = "Телефон";
            dataGridView1.Columns["SpecialityName"].HeaderText = "Специальность";
            dataGridView1.Columns["Photo"].Visible = false;
            label9.Text = $"Количество записей: {doctorsTable.Rows.Count}";

            DataGridViewImageColumn imgCol = (DataGridViewImageColumn)dataGridView1.Columns["Фото"];
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;

            dataGridView1.RowTemplate.Height = 100;
            dataGridView1.Refresh();
        }

        private void LoadDoctorPhotos(DataTable table)
        {
            if (table == null) return;

            if (!table.Columns.Contains("Фото"))
                table.Columns.Add("Фото", typeof(Image));

            string photoFolder = Path.Combine(Application.StartupPath, "photo");

            foreach (DataRow row in table.Rows)
            {
                string fileName = "not-image.png";

                if (row["Photo"] != DBNull.Value && !string.IsNullOrEmpty(row["Photo"].ToString()))
                    fileName = row["Photo"].ToString();

                string photoPath = Path.Combine(photoFolder, fileName);
                if (!File.Exists(photoPath))
                    photoPath = Path.Combine(photoFolder, "not-image.png");

                try
                {
                    using (FileStream fs = new FileStream(photoPath, FileMode.Open, FileAccess.Read))
                    {
                        using (var tempImg = Image.FromStream(fs))
                        {
                            row["Фото"] = new Bitmap(tempImg);
                        }
                    }
                }
                catch
                {
                    row["Фото"] = new Bitmap(100, 100);
                }
            }
        }
        private void FillSpecialties()
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT DISTINCT SpecialityName FROM speciality;";
                MySqlCommand cmd = new MySqlCommand(query, con);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("Все");
                    while (reader.Read())
                    {
                        string specialty = reader["SpecialityName"].ToString();
                        comboBox1.Items.Add(specialty);
                    }
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем ID врача
            int doctorId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["idDoctors"].Value);

            // Получаем фото из таблицы (колонка "Фото")
            Image photo = null;
            if (dataGridView1.SelectedRows[0].Cells["Фото"].Value is Image img)
            {
                photo = new Bitmap(img); // копия, чтобы не блокировать ресурс
            }

            // Открываем форму редактирования и передаем фото
            EditDoctor editForm = new EditDoctor(doctorId, photo);
            editForm.ShowDialog();

            // После закрытия формы — обновляем таблицу
            LoadDoctor();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }
        private void ApplyFilterAndSort()
        {
            if (doctorsTable == null) return;

            string filterExpr = "";

            string selectedSpecialty = comboBox1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedSpecialty) && selectedSpecialty != "Все")
            {
                filterExpr = $"SpecialityName = '{selectedSpecialty.Replace("'", "''")}'";
            }

            string searchText = textBox5.Text.Trim().Replace("'", "''");
            if (!string.IsNullOrEmpty(searchText))
            {
                if (!string.IsNullOrEmpty(filterExpr))
                {
                    filterExpr += " AND ";
                }
                filterExpr += $"Surname LIKE '%{searchText}%'";
            }

            string sortExpr = "";
            if (comboBox2.SelectedIndex == 1)
                sortExpr = "Surname ASC";
            else if (comboBox2.SelectedIndex == 2)
                sortExpr = "Surname DESC";

            DataView dv = doctorsTable.DefaultView;
            dv.RowFilter = filterExpr;
            dv.Sort = sortExpr;

            dataGridView1.DataSource = dv;
            dataGridView1.Refresh();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddDoctor ad = new AddDoctor();
            ad.ShowDialog();

            LoadDoctor();
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian_Hyphen(sender, e);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            textBox5.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Schedule WHERE idDoctor = @doctorId";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@doctorId", selectedId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Нельзя удалить этого врача, так как он используется в расписании!",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                DialogResult result = MessageBox.Show(
                    "Вы уверены, что хотите удалить запись?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;

                string deleteQuery = "DELETE FROM Doctors WHERE idDoctors = @id";
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@id", selectedId);
                    deleteCmd.ExecuteNonQuery();
                    MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            selectedId = -1;
            LoadDoctor();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedId = Convert.ToInt32(row.Cells["idDoctors"].Value);
            }
        }
    }
}
