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
        public int DoctorId { get; private set; }
        public DoctorCard()
        {
            InitializeComponent();
        }
        public void SetData(int id, string fio, string phone, string spec, Image photo)
        {
            DoctorId = id;
            dfio.Text = fio;
            phonenumber.Text = phone;
            dspec.Text = spec;
            pictureBox1.Image = photo;
        }
    }
}
