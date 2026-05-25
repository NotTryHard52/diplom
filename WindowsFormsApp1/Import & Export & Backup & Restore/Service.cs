using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Import___Export___Backup___Restore;

namespace WindowsFormsApp1.Import___Export___Backup
{
    public partial class Service : Form
    {
        public Service()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Import import = new Import();
            import.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Export export = new Export();
            export.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Backup backup = new Backup();
            backup.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Restore restore = new Restore();
            restore.ShowDialog();
        }
    }
}
