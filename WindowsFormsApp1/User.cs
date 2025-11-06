using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class User : Form
    {
        string connectionString;
        int selectedId = -1;
        private int currentUserId;
        public User(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
        }

        private void User_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            LoadUser();
            dataGridView1.CellClick += dataGridView1_CellClick;
        }
        private void LoadUser()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string userQuery = @"
            SELECT 
                Users.idUsers,
                Users.Surname,
                Users.Name,
                Users.Lastname,
                Users.Login,
                Users.Password,
                Roles.RoleName AS Role
            FROM Users
            JOIN Roles ON Users.Role = Roles.idRoles;
        ";

                DataTable userTable = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(userQuery, con);
                da.Fill(userTable);

                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = userTable;

                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[1].HeaderText = "Фамилия";
                dataGridView1.Columns[2].HeaderText = "Имя";
                dataGridView1.Columns[3].HeaderText = "Отчество";
                dataGridView1.Columns[4].HeaderText = "Логин";
                dataGridView1.Columns[5].HeaderText = "Пароль";
                dataGridView1.Columns[6].HeaderText = "Роль";
                label2.Text = $"Количество записей: {userTable.Rows.Count}";

                string roleQuery = "SELECT idRoles, RoleName FROM Roles;";
                MySqlCommand roleCmd = new MySqlCommand(roleQuery, con);
                MySqlDataAdapter roleDa = new MySqlDataAdapter(roleCmd);
                DataTable roleTable = new DataTable();
                roleDa.Fill(roleTable);

                comboBox1.DisplayMember = "RoleName";
                comboBox1.ValueMember = "idRoles";
                comboBox1.DataSource = roleTable;
            }
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

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.English_Symbol(sender, e);
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputLimit.English_Symbol(sender, e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int num = rnd.Next(1000, 9999);
            textBox6.Text = "login_" + num;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string password = GeneratePassword(10);
            textBox5.Text = password;
        }
        private string GeneratePassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; i++)
                sb.Append(chars[rnd.Next(chars.Length)]);

            return sb.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || 
        string.IsNullOrWhiteSpace(textBox2.Text) ||
        string.IsNullOrWhiteSpace(textBox6.Text) ||
        string.IsNullOrWhiteSpace(textBox5.Text) || 
        comboBox1.SelectedValue == null)             
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string surname = textBox1.Text.Trim();
            string name = textBox2.Text.Trim();
            string lastname = textBox3.Text.Trim();
            string login = textBox6.Text.Trim();
            string password = textBox5.Text.Trim();
            int roleId = Convert.ToInt32(comboBox1.SelectedValue);

            string hash_password;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                hash_password = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@login", login);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string insertQuery = @"
            INSERT INTO Users (Surname, Name, Lastname, Login, Password, Role)
            VALUES (@surname, @name, @lastname, @login, @password, @role)";

                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@surname", surname);
                    insertCmd.Parameters.AddWithValue("@name", name);
                    insertCmd.Parameters.AddWithValue("@lastname", lastname);
                    insertCmd.Parameters.AddWithValue("@login", login);
                    insertCmd.Parameters.AddWithValue("@password", hash_password);
                    insertCmd.Parameters.AddWithValue("@role", roleId);

                    insertCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUser();
            }
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox5.Clear();
            textBox6.Clear();
            comboBox1.SelectedIndex = -1;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                selectedId = Convert.ToInt32(row.Cells["idUsers"].Value);
                textBox1.Text = row.Cells["Surname"].Value.ToString();
                textBox2.Text = row.Cells["Name"].Value.ToString();
                textBox3.Text = row.Cells["Lastname"].Value.ToString();
                textBox6.Text = row.Cells["Login"].Value.ToString();
                textBox5.Text = "";
                string roleName = row.Cells["Role"].Value.ToString();
                comboBox1.SelectedIndex = comboBox1.FindStringExact(roleName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string surname = textBox1.Text.Trim();
            string name = textBox2.Text.Trim();
            string lastname = textBox3.Text.Trim();
            string login = textBox6.Text.Trim();
            string password = textBox5.Text.Trim();
            int roleId = Convert.ToInt32(comboBox1.SelectedValue);

            if (string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(login) || comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Поля не должны быть пустыми!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string currentSurname = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            string currentName = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            string currentLastname = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
            string currentLogin = dataGridView1.SelectedRows[0].Cells[4].Value.ToString();
            string currentRoleName = dataGridView1.SelectedRows[0].Cells[6].Value.ToString();
            string selectedRoleName = comboBox1.Text;

            bool changed = surname != currentSurname ||
               name != currentName ||
               lastname != currentLastname ||
               login != currentLogin ||
               selectedRoleName != currentRoleName ||
               !string.IsNullOrWhiteSpace(password);

            if (!changed)
            {
                MessageBox.Show("Вы не внесли никаких изменений!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Login = @login AND idUsers != @id";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@login", login);
                    checkCmd.Parameters.AddWithValue("@id", selectedId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string updateQuery;
                if (!string.IsNullOrWhiteSpace(password))
                {
                    string hash_password;
                    using (var sha = SHA256.Create())
                    {
                        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                        hash_password = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }

                    updateQuery = @"
                        UPDATE Users
                        SET Surname=@surname, Name=@name, Lastname=@lastname,
                            Login=@login, Password=@password, Role=@role
                        WHERE idUsers=@id";
                }
                else
                {
                    updateQuery = @"
                        UPDATE Users
                        SET Surname=@surname, Name=@name, Lastname=@lastname,
                            Login=@login, Role=@role
                        WHERE idUsers=@id";
                }

                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, con))
                {
                    updateCmd.Parameters.AddWithValue("@surname", surname);
                    updateCmd.Parameters.AddWithValue("@name", name);
                    updateCmd.Parameters.AddWithValue("@lastname", lastname);
                    updateCmd.Parameters.AddWithValue("@login", login);
                    updateCmd.Parameters.AddWithValue("@role", roleId);
                    updateCmd.Parameters.AddWithValue("@id", selectedId);

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        string hash_password;
                        using (var sha = SHA256.Create())
                        {
                            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                            hash_password = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                        }
                        updateCmd.Parameters.AddWithValue("@password", hash_password);
                    }

                    updateCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            selectedId = -1;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox5.Clear();
            textBox6.Clear();
            comboBox1.SelectedIndex = -1;
            LoadUser();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedId == currentUserId)
            {
                MessageBox.Show("Нельзя удалить пользователя, под которым выполнен вход!", "Отказано", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string userLogin = dataGridView1.SelectedRows[0].Cells["Login"].Value.ToString();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM `Order` WHERE User = @userId";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@userId", selectedId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Нельзя удалить этого пользователя, так как он используется в других записях!",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить пользователя \"{userLogin}\"?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;

                string deleteQuery = "DELETE FROM Users WHERE idUsers = @id";
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@id", selectedId);

                    try
                    {
                        deleteCmd.ExecuteNonQuery();
                        MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            selectedId = -1;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox5.Clear();
            textBox6.Clear();
            comboBox1.SelectedIndex = -1;

            LoadUser();
        }
    }
}
