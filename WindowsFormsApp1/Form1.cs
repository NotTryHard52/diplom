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
    public partial class Form1 : Form
    {
        private int currentUserId;
        private string currentRole;
        private Form activeForm = null;
        
        public Form1(string FIO, int userId, string role)
        {
            InitializeComponent();
            label_fio.Text = FIO;
            currentUserId = userId;
            currentRole = role;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            this.Hide();
            login.ShowDialog();
            this.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            OpenChildForm(new UchetTalona_GL());
            panel1.Width = 60;
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

        private void button6_Click(object sender, EventArgs e)
        {
            OpenChildForm(new UchetTalona_GL());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole));
        }
    }
}
