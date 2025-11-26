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
    public partial class Menu : Form
    {
        private int currentUserId;
        private string currentRole;
        private Form activeForm = null;
        public Menu(string FIO, int userId, string role)
        {
            InitializeComponent();
            label_fio.Text = FIO;
            currentUserId = userId;
            currentRole = role;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            this.Hide();
            login.ShowDialog();
            this.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Directory direct = new Directory();
            this.Hide();
            direct.ShowDialog();
            this.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Form1 user = new Form1();
            //this.Hide();
            //user.ShowDialog();
            //this.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Doctor doc = new Doctor();
            this.Hide();
            doc.ShowDialog();
            this.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            panel1.Width = 60;

            var directoryForm = new Directory();

            // подписка на событие
            directoryForm.DictionarySelected += (childForm) =>
            {
                OpenChildForm(childForm);
            };

            OpenChildForm(directoryForm);

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            OpenChildForm(new User(currentUserId));
            panel1.Width = 60; // свернуть меню после открытия 193
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

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Doctor());
            panel1.Width = 60; // свернуть меню после открытия 193
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            this.Hide();
            login.ShowDialog();
            this.Show();
        }
        public void ExpandPanel()
        {
            this.Width = 1120;
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            panel1.Width = 193;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole));
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            OpenChildForm(new User(currentUserId));
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            this.Hide();
            login.ShowDialog();
            this.Show();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var directoryForm = new Directory();

            directoryForm.DictionarySelected += (childForm) =>
            {
                OpenChildForm(childForm);
            };

            OpenChildForm(directoryForm);
        }
    }
}
