using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Main_menu : Form
    {
        public Main_menu(string FIO, int role)
        {
            InitializeComponent();
            label_fio.Text = FIO;
            label_role.Text = GetRoleName(role);
        }

        private string GetRoleName(int roleId)
        {
            string roleName = "";
            Connect connect = new Connect();
            string connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {

                con.Open();

                string query = "SELECT RoleName FROM Roles WHERE idRoles = @id";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", roleId);

                    object result = cmd.ExecuteScalar();

                    if (result != null)
                        roleName = result.ToString();
                }
            }

            return roleName;
        }
    }
}
