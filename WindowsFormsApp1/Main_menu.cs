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
    public partial class Main_menu : Form
    {
        public Main_menu(string FIO, string role)
        {
            InitializeComponent();
            label_fio.Text = FIO;
            if (role == "1") { label_role.Text = "Администратор"; }
            else if (role == "2") { label_role.Text = "Регистратор"; }
            else if (role == "3") { label_role.Text = "Главный врач"; }
        }
    }
}
