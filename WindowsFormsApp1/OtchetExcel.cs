using MySql.Data.MySqlClient;
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
    public partial class OtchetExcel : Form
    {
        string connectionString;
        DataTable orderTable;
        public OtchetExcel()
        {
            InitializeComponent();
        }

        private void OtchetExcel_Load(object sender, EventArgs e)
        {
            dateFrom.Value = DateTime.Now.AddMonths(-1);
            dateTo.Value = DateTime.Now;
            dateFrom.MaxDate = DateTime.Now; 
            dateTo.MaxDate = DateTime.Now.AddDays(1);
            Connect connect = new Connect();
            connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                orderTable = new DataTable();
                string query = @"
                        SELECT 
                            o.idOrder AS 'Номер талона',
                            o.sum AS 'Сумма',
                            CONCAT(d.surname, ' ', d.name, ' ', d.lastname) AS 'Врач',
                            DATE_FORMAT(sc.date, '%d.%m.%Y') AS 'Дата',
                            sc.time AS 'Время',
                            CONCAT(r.surname, ' ', r.name, ' ', r.lastname) AS 'Регистратор',
                            CONCAT(p.surname, ' ', p.name, ' ', p.lastname) AS 'Пациент',
                            st.name AS 'Статус'
                        FROM `Order` o
                        JOIN Schedule sc ON o.Schedule = sc.idSchedule
                        JOIN Doctors d ON sc.idDoctor = d.idDoctors
                        JOIN `Users` r ON o.User = r.idUsers
                        JOIN Patients p ON o.Patients_idPatients = p.idPatients
                        JOIN StatusesPriem st ON o.Status = st.idStatusesPriem
                    ";
                MySqlCommand cmd = new MySqlCommand(query, con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(orderTable);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.DataSource = orderTable;
                if (orderTable.Rows.Count > 0)
                {
                    var dates = orderTable.AsEnumerable()
                                          .Where(r => r["Дата"] != DBNull.Value)
                                          .Select(r => Convert.ToDateTime(r["Дата"]))
                                          .ToList();

                    if (dates.Count > 0)
                    {
                        DateTime minDate = dates.Min();
                        DateTime maxDate = dates.Max();

                        dateFrom.MinDate = minDate;
                        dateFrom.MaxDate = maxDate;
                        dateFrom.Value = minDate;

                        dateTo.MinDate = minDate;
                        dateTo.MaxDate = maxDate;
                        dateTo.Value = maxDate;
                    }
                }
            }
            ApplyFilterAndSort();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (dateFrom.Value > dateTo.Value)
            {
                dateTo.Value = dateFrom.Value;
            }
            ApplyFilterAndSort();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (dateTo.Value < dateFrom.Value)
            {
                dateFrom.Value = dateTo.Value;
            }
            ApplyFilterAndSort();
        }
        private void ApplyFilterAndSort()
        {
            if (orderTable == null || orderTable.Rows.Count == 0)
                return;

            DateTime start = dateFrom.Value.Date;
            DateTime end = dateTo.Value.Date.AddDays(1).AddTicks(-1); 

            string filter = $"Дата >= #{start:MM/dd/yyyy}# AND Дата <= #{end:MM/dd/yyyy}#";

            DataView dv = orderTable.DefaultView;
            dv.RowFilter = filter;

            dataGridView1.DataSource = dv;
        }
    }
}
