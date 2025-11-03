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
    public partial class Services : Form
    {
        string connectionString;
        int selectedId;
        public int SelectedServiceId { get; private set; }
        public string SelectedServiceName { get; private set; }
        public decimal SelectedServicePrice { get; private set; }
        public Services()
        {
            InitializeComponent();
        }

        private void Services_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            LoadServices();
        }
        private void LoadServices()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                DataTable t = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT s.idServices, s.Name, s.Price, c.Name AS Category FROM Services s JOIN Category c ON s.Category = c.idCategory;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[1].HeaderText = "Наименование";
                dataGridView1.Columns[2].HeaderText = "Цена";
                dataGridView1.Columns[3].HeaderText = "Категория";
                label2.Text = $"Количество записей: {t.Rows.Count}";

                string categoryQuery = "SELECT idCategory, Name FROM Category;";
                MySqlCommand categoryCmd = new MySqlCommand(categoryQuery, con);
                MySqlDataAdapter categoryDa = new MySqlDataAdapter(categoryCmd);
                DataTable categoryTable = new DataTable();
                categoryDa.Fill(categoryTable);

                comboBox1.DisplayMember = "Name";
                comboBox1.ValueMember = "idCategory";
                comboBox1.DataSource = categoryTable;
            }

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian(sender, e);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Numbers(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
        string.IsNullOrWhiteSpace(textBox2.Text) ||
        comboBox1.SelectedValue == null)       
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBox2.Text.Trim(), out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string serviceName = textBox1.Text.Trim();
            int categoryId = Convert.ToInt32(comboBox1.SelectedValue);

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Services WHERE Name = @name";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@name", serviceName);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string insertQuery = "INSERT INTO Services (Name, Price, Category) VALUES (@name, @price, @category)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                    {
                        insertCmd.Parameters.AddWithValue("@name", serviceName);
                        insertCmd.Parameters.AddWithValue("@price", price);
                        insertCmd.Parameters.AddWithValue("@category", categoryId);
                        insertCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadServices();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении услуги:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            textBox1.Clear();
            textBox2.Clear();
            comboBox1.SelectedIndex = -1;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                selectedId = Convert.ToInt32(row.Cells["idServices"].Value);
                textBox1.Text = row.Cells["Name"].Value.ToString();
                textBox2.Text = row.Cells["Price"].Value.ToString();
                string categoryName = row.Cells["Category"].Value.ToString();
                comboBox1.SelectedIndex = comboBox1.FindStringExact(categoryName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = textBox1.Text.Trim();
            string priceText = textBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priceText) || comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int categoryId = Convert.ToInt32(comboBox1.SelectedValue);

            DataGridViewRow currentRow = dataGridView1.SelectedRows.Count > 0 ? dataGridView1.SelectedRows[0] : null;
            if (currentRow != null)
            {
                string currentName = currentRow.Cells["Name"].Value.ToString();
                string currentPrice = currentRow.Cells["Price"].Value.ToString();
                string currentCategory = currentRow.Cells["Category"].Value.ToString();
                string newCategory = comboBox1.Text;

                bool changed = name != currentName || priceText != currentPrice || newCategory != currentCategory;

                if (!changed)
                {
                    MessageBox.Show("Вы не внесли никаких изменений!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Services WHERE Name = @name AND idServices <> @id";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@name", name);
                    checkCmd.Parameters.AddWithValue("@id", selectedId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string updateQuery = @"UPDATE Services 
                                           SET Name = @name, Price = @price, Category = @category 
                                           WHERE idServices = @id";
                using (MySqlCommand cmd = new MySqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@category", categoryId);
                    cmd.Parameters.AddWithValue("@id", selectedId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadServices();

                selectedId = -1;
                textBox1.Clear();
                textBox2.Clear();
                comboBox1.SelectedIndex = -1;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите услугу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dataGridView1.SelectedRows[0];
            SelectedServiceId = Convert.ToInt32(row.Cells["idServices"].Value);
            SelectedServiceName = row.Cells["Name"].Value.ToString();
            SelectedServicePrice = Convert.ToDecimal(row.Cells["Price"].Value);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ServicesName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить запись: \"{ServicesName}\"?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string deleteQuery = "DELETE FROM Services WHERE idServices = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con);
                deleteCmd.Parameters.AddWithValue("@id", selectedId);
                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            textBox2.Clear();
            comboBox1.SelectedIndex = -1;
            selectedId = -1;
            LoadServices();
        }
    }
}
