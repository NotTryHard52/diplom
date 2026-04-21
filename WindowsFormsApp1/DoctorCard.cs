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
    public partial class DoctorCard : UserControl
    {
        public event EventHandler<int> EditClicked;
        public event EventHandler<int> DeleteClicked;

        private int doctorId;
        public DoctorCard()
        {
            InitializeComponent();
            button2.Click += button2_Click;
            button3.Click += button3_Click;
        }
        public void SetData(int id, string fio, string phone, string spec, Image photo)
        {
            doctorId = id;
            dfio.Text = fio;
            phonenumber.Text = phone;
            dspec.Text = spec;
            pictureBox1.Image = photo;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EditClicked?.Invoke(this, doctorId);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteClicked?.Invoke(this, doctorId);
        }
    }
}
