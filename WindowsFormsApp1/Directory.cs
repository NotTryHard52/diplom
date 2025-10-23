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
    public partial class Directory : Form
    {
        public event Action<Form> DictionarySelected;
        public Directory()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new Role());
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new Status());
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new Category());
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new Speciality());
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new StatusPriem());
        }
    }
}
