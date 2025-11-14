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


        private void button1_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new Role());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new Status());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new Category());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new Speciality());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DictionarySelected?.Invoke(new StatusPriem());
        }

        internal static object GetParent(string startupPath)
        {
            throw new NotImplementedException();
        }
    }
}
