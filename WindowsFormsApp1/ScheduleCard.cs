using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp1.Schedule;

namespace WindowsFormsApp1
{
    public partial class ScheduleCard : UserControl
    {
        public ScheduleItem Item { get; private set; }
        public event Action<ScheduleItem> CardClicked;
        public ScheduleCard(ScheduleItem item)
        {
            InitializeComponent();
            Item = item;

            label1.Text = item.DoctorName;
            label2.Text = item.Time;
            label3.Text = item.Status;

            BackColor = item.Status == "Свободно"
                ? Color.FromArgb(220, 255, 220)
                : Color.FromArgb(255, 220, 220);

            label3.ForeColor = item.Status == "Свободно" ? Color.Green : Color.Red;

            this.Click += OnClick;
            foreach (Control c in Controls)
                c.Click += OnClick;
        }

        private void OnClick(object sender, EventArgs e)
        {
            CardClicked?.Invoke(Item);
        }
    }
}
