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
    public partial class Status : Form
    {
        string connectionString;
        public Status()
        {
            InitializeComponent();
        }

        private void Status_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                DataTable t = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Statuses;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;
                dataGridView1.Columns[0].HeaderText = "ID";
                dataGridView1.Columns[1].HeaderText = "Наименование";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

    }
}
