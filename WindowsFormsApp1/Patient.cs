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
        int selectedId = -1;
        private bool openedFromTalon = false;
        public Patient(bool fromTalon = false)
        {
            InitializeComponent();

            openedFromTalon = fromTalon;

            button4.Visible = openedFromTalon;
        }

        private void Patient_Load(object sender, EventArgs e)
        {
            int count = CountData.GetTableCount("patients");
            label9.Text = $"Количество записей: {count}";
            comboBox2.SelectedIndex = 0;
            LoadPatient();
            var hoverEffect = new HoverDataGridView(dataGridView1);
        }
        private void LoadPatient()
        {
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
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["idPatients"].Value);

            EditPatient ed = new EditPatient(id);
            ed.ShowDialog();

            LoadPatient();
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
            InputLimit.Numbers(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddPatient ad = new AddPatient();
            ad.ShowDialog();

            LoadPatient();
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

            PatientSelected?.Invoke(patientId, fullName);

            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
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

                string checkQuery = "SELECT COUNT(*) FROM `Order` WHERE Patients_idPatients = @patientId";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@patientId", selectedId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Нельзя удалить этого пациента, так как он используется в приемах!",
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

                string deleteQuery = "DELETE FROM Patients WHERE idPatients = @id";
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@id", selectedId);
                    deleteCmd.ExecuteNonQuery();
                    MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            selectedId = -1;
            LoadPatient();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedId = Convert.ToInt32(row.Cells["idPatients"].Value);
            }
        }
    }
}
