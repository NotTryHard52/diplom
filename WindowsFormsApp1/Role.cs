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
    public partial class Role : Form
    {
        string connectionString;
        int selectedRoleId = -1;
        public Role()
        {
            InitializeComponent();
        }

        private void Role_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            LoadRoles();
            var hoverEffect = new HoverDataGridView(dataGridView1);
        }
        private void LoadRoles()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                DataTable t = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Roles;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);

                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[1].HeaderText = "Наименование";

                label2.Text = $"Количество записей: {t.Rows.Count}";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string roleName = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(roleName))
            {
                MessageBox.Show("Поле не должно быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Roles WHERE RoleName = @name";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@name", roleName);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string insertQuery = "INSERT INTO Roles (RoleName) VALUES (@name)";
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@name", roleName);
                insertCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            LoadRoles();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.Russian(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedRoleId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newName = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Поле не должно быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            if (newName == currentName)
            {
                MessageBox.Show("Вы не внесли изменений!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Roles WHERE RoleName = @name AND idRoles != @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@name", newName);
                checkCmd.Parameters.AddWithValue("@id", selectedRoleId);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("Такая запись уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string updateQuery = "UPDATE Roles SET RoleName = @name WHERE idRoles = @id";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, con);
                updateCmd.Parameters.AddWithValue("@name", newName);
                updateCmd.Parameters.AddWithValue("@id", selectedRoleId);
                updateCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            selectedRoleId = -1;
            LoadRoles();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedRoleId = Convert.ToInt32(row.Cells[0].Value);
                textBox1.Text = row.Cells[1].Value.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedRoleId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string roleName = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Role = @roleId";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@roleId", selectedRoleId);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("Нельзя удалить эту роль, так как она используется в других записях!","Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить запись: \"{roleName}\"?","Подтверждение удаления",MessageBoxButtons.YesNo,MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;

                string deleteQuery = "DELETE FROM Roles WHERE idRoles = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con);
                deleteCmd.Parameters.AddWithValue("@id", selectedRoleId);
                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            textBox1.Clear();
            selectedRoleId = -1;
            LoadRoles();
        }
    }
}
