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
    public partial class UchetTalona : Form
    {
        string connectionString;
        public UchetTalona()
        {
            InitializeComponent();
        }

        private void UchetTalona_Load(object sender, EventArgs e)
        {
            int count = CountData.GetTableCount("Order");
            label9.Text = $"Количество записей: {count}";
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                DataTable t = new DataTable();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `Order`;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(t);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = t;
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[1].HeaderText = "Услуги";
                dataGridView1.Columns[2].HeaderText = "Сумма";
                dataGridView1.Columns[3].HeaderText = "Количество";
                dataGridView1.Columns[4].HeaderText = "Расписание";
                dataGridView1.Columns[5].HeaderText = "Регистратор";
                dataGridView1.Columns[6].HeaderText = "Пациент";
            }
        }
    }
}
