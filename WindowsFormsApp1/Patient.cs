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
    public partial class Patient : Form
    {
        string connectionString;
        DataTable patientTable;
        public event Action<int, string> PatientSelected;
        public Patient()
        {
            InitializeComponent();
        }

        private void Patient_Load(object sender, EventArgs e)
        {
            int count = CountData.GetTableCount("patients");
            label9.Text = $"Количество записей: {count}";
            comboBox2.SelectedIndex = 0;
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                patientTable = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Patients;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(patientTable);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = patientTable;
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[1].HeaderText = "Фамилия";
                dataGridView1.Columns[2].HeaderText = "Имя";
                dataGridView1.Columns[3].HeaderText = "Отчество";
                dataGridView1.Columns[4].HeaderText = "Дата рождения";
                dataGridView1.Columns[5].HeaderText = "Номер телефона";
                dataGridView1.Columns[6].HeaderText = "Номер полиса";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EditPatient ed = new EditPatient();
            ed.ShowDialog();
            this.Show();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }
        private void ApplyFilterAndSort()
        {
            if (patientTable == null) return;

            string filterExpr = "";

            string searchText = textBox5.Text.Trim().Replace("'", "''");
            if (!string.IsNullOrEmpty(searchText))
            {
                if (int.TryParse(searchText, out int policyNumber))
                {
                    filterExpr = $"Number_policy = {policyNumber}";
                }
                else
                {
                    filterExpr = "";
                }
            }

            string sortExpr = "";
            if (comboBox2.SelectedIndex == 1)
                sortExpr = "Surname ASC";
            else if (comboBox2.SelectedIndex == 2)
                sortExpr = "Surname DESC";

            DataView dv = patientTable.DefaultView;
            dv.RowFilter = filterExpr;
            dv.Sort = sortExpr;

            dataGridView1.DataSource = dv;
            dataGridView1.Refresh();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian_Hyphen(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddPatient ad = new AddPatient();
            ad.ShowDialog();
            this.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пациента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dataGridView1.SelectedRows[0];

            int patientId = Convert.ToInt32(row.Cells["idPatients"].Value);
            string fullName = $"{row.Cells["Surname"].Value} {row.Cells["Name"].Value} {row.Cells["Lastname"].Value}";

            // Вызываем событие
            PatientSelected?.Invoke(patientId, fullName);

            this.Close();
        }
    }
}
