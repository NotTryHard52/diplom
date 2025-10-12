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
    public partial class Talon : Form
    {
        string connectionString;
        public Talon()
        {
            InitializeComponent();
        }

        private void Talon_Load(object sender, EventArgs e)
        {
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                DataTable t = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Services;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[1].HeaderText = "Наименование";
                dataGridView1.Columns[2].HeaderText = "Цена";
                dataGridView1.Columns[3].HeaderText = "Категория";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Schedule sh = new Schedule();
            sh.ShowDialog();
            this.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Patient pat = new Patient();
            pat.ShowDialog();
            this.Show();
        }
    }
}
