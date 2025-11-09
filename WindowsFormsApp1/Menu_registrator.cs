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
    public partial class Menu_registrator : Form
    {
        private int currentUserId;
        public Menu_registrator(string FIO, int userId)
        {
            InitializeComponent();
            label_fio.Text = FIO;
            currentUserId = userId;
        }
        private void OpenChildForm(Form childForm)
        {
            panel2.Controls.Clear();

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel2.Controls.Add(childForm);
            childForm.Show();
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Services());
            panel1.Width = 60;
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Doctor());
            panel1.Width = 60;
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Patient());
            panel1.Width = 60;
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Schedule());
            panel1.Width = 60;
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            this.Hide();
            login.ShowDialog();
            this.Show();
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Talon(label_fio.Text, currentUserId));
            panel1.Width = 60;
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            OpenChildForm(new UchetTalona());
            panel1.Width = 60;
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            panel1.Width = 193;
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void Menu_registrator_Load(object sender, EventArgs e)
        {
            
        }
    }
}
