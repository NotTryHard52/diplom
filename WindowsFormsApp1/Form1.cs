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
        public Form1(string FIO, int userId)
        {
            InitializeComponent();
            label_fio.Text = FIO;
            currentUserId = userId;
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
            // Закрыть предыдущую форму, если нужно
            panel2.Controls.Clear();

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel2.Controls.Add(childForm);
            childForm.Show();
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            panel1.Width = 193;
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}
