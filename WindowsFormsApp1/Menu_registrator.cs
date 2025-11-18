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
        private string currentRole;
        private Form activeForm = null;

        public Menu_registrator(string FIO, int userId, string role)
        {
            InitializeComponent();
            label_fio.Text = FIO;
            currentUserId = userId;
            currentRole = role;
        }
        private void OpenChildForm(Form childForm)
        {
            if (activeForm != null && activeForm.GetType() == childForm.GetType())
            {
                return;
            }

            panel2.Controls.Clear();

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel2.Controls.Add(childForm);
            childForm.Show();

            activeForm = childForm;
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

        private void Menu_registrator_Load(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Services(false));
            //panel1.Width = 60;
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Doctor());
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Patient());
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Schedule());
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Talon(label_fio.Text, currentUserId));
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenChildForm(new UchetTalona());
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole));
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            this.Hide();
            login.ShowDialog();
            this.Show();
        }
    }
}
