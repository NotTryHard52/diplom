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
    public partial class Doctor : Form
    {
        string connectionString;
        DataTable doctorsTable;

        public Doctor()
        {
            InitializeComponent();
        }

        private void Doctor_Load(object sender, EventArgs e)
        {
            FillSpecialties();
            int count = CountData.GetTableCount("doctors");
            label9.Text = $"Количество записей: {count}";
            comboBox2.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            textBox5.TextChanged += textBox5_TextChanged;
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                doctorsTable = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT d.idDoctors, d.Surname, d.Name, d.Lastname, d.Phone_number, d.Photo, s.SpecialityName FROM Doctors d JOIN Speciality s ON d.Speciality = s.idSpeciality;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(doctorsTable);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = doctorsTable;
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[1].HeaderText = "Фамилия";
                dataGridView1.Columns[2].HeaderText = "Имя";
                dataGridView1.Columns[3].HeaderText = "Отчество";
                dataGridView1.Columns[4].HeaderText = "Номер телефона";
                dataGridView1.Columns[5].HeaderText = "Фото";
                dataGridView1.Columns[6].HeaderText = "Специальность";
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

            int doctorId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["idDoctors"].Value);
            EditDoctor editForm = new EditDoctor(doctorId);
            editForm.ShowDialog();
            Doctor_Load(sender, e);
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
            this.Show();
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

        }
    }
}
