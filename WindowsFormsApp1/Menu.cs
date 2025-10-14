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
        public Menu(string FIO)
        {
            InitializeComponent();
            label_fio.Text = FIO;
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
            OpenChildForm(new User());
            panel1.Width = 60; // свернуть меню после открытия 193
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
    }
}
