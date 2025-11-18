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
using Excel = Microsoft.Office.Interop.Excel;

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

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Отчёт не может быть сформирован, потому что нет данных в таблице за выбранный период.\n", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel (*.xlsx)|*.xlsx";
            sfd.FileName = "Отчет.xlsx";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                Excel.Application excel = new Excel.Application();
                Excel.Workbook workbook = excel.Workbooks.Add();
                Excel.Worksheet sheet = workbook.ActiveSheet;

                sheet.Name = "Отчёт";

                sheet.Cells[1, 1] = "Отчёт по талонам";
                sheet.Cells[1, 1].Font.Bold = true;
                sheet.Cells[1, 1].Font.Size = 16;

                sheet.Cells[3, 1] = "Период:";
                sheet.Cells[3, 2] = $"{dateFrom.Value:dd.MM.yyyy} — {dateTo.Value:dd.MM.yyyy}";
                sheet.Cells[4, 1] = "Дата выгрузки:";
                sheet.Cells[4, 2] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

                int startRow = 6;

                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    sheet.Cells[startRow, i + 1] = dataGridView1.Columns[i].HeaderText;
                }

                Excel.Range headerRange = sheet.Range[
                    sheet.Cells[startRow, 1],
                    sheet.Cells[startRow, dataGridView1.Columns.Count]
                ];

                headerRange.Font.Bold = true;
                headerRange.Interior.Color = Color.LightBlue;
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                for (int r = 0; r < dataGridView1.Rows.Count; r++)
                {
                    for (int c = 0; c < dataGridView1.Columns.Count; c++)
                    {
                        sheet.Cells[startRow + 1 + r, c + 1] =
                            dataGridView1.Rows[r].Cells[c].Value?.ToString();
                    }
                }
                int statusColumnIndex = -1;

                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    if (dataGridView1.Columns[i].HeaderText == "Статус")
                    {
                        statusColumnIndex = i + 1;
                        break;
                    }
                }

                if (statusColumnIndex != -1)
                {
                    for (int r = 0; r < dataGridView1.Rows.Count; r++)
                    {
                        string status = dataGridView1.Rows[r].Cells["Статус"].Value?.ToString();

                        Excel.Range rowRange = sheet.Range[
                            sheet.Cells[startRow + 1 + r, 1],
                            sheet.Cells[startRow + 1 + r, dataGridView1.Columns.Count]
                        ];

                        if (status != null)
                        {
                            status = status.Trim().ToLower();

                            if (status.Contains("заверш"))
                            {
                                rowRange.Interior.Color = Color.LightGreen;
                            }
                            else if (status.Contains("отмен"))
                            {
                                rowRange.Interior.Color = Color.LightCoral;
                            }
                            else if (status.Contains("создан"))
                            {
                                rowRange.Interior.Color = Color.LightYellow;
                            }
                        }
                    }
                }

                Excel.Range fullTable = sheet.Range[
                    sheet.Cells[startRow, 1],
                    sheet.Cells[startRow + dataGridView1.Rows.Count, dataGridView1.Columns.Count]
                ];

                fullTable.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                fullTable.Columns.AutoFit();

                int totalsRow = startRow + dataGridView1.Rows.Count + 2;

                decimal totalSum = 0;

                int countCompleted = 0;
                int countCreated = 0;
                int countCanceled = 0;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    string status = row.Cells["Статус"].Value?.ToString()?.ToLower();

                    if (status != null)
                    {
                        if (status.Contains("заверш"))
                            countCompleted++;

                        if (status.Contains("создан"))
                            countCreated++;

                        if (status.Contains("отмен"))
                            countCanceled++;
                    }

                    if (status != null &&
                        (status.Contains("отмен") || status.Contains("создан")))
                        continue;

                    if (row.Cells["Сумма"].Value != null)
                        totalSum += Convert.ToDecimal(row.Cells["Сумма"].Value);
                }

                sheet.Cells[totalsRow, 1] = "Всего записей:";
                sheet.Cells[totalsRow, 2] = dataGridView1.Rows.Count;
                sheet.Cells[totalsRow, 1].Font.Bold = true;

                sheet.Cells[totalsRow + 1, 1] = "Итоговая сумма (завершённые):";
                sheet.Cells[totalsRow + 1, 2] = totalSum;
                sheet.Cells[totalsRow + 1, 1].Font.Bold = true;

                sheet.Cells[totalsRow + 3, 1] = "Завершено:";
                sheet.Cells[totalsRow + 3, 2] = countCompleted;

                sheet.Cells[totalsRow + 4, 1] = "Создано:";
                sheet.Cells[totalsRow + 4, 2] = countCreated;

                sheet.Cells[totalsRow + 5, 1] = "Отменено:";
                sheet.Cells[totalsRow + 5, 2] = countCanceled;

                sheet.Range[
                    sheet.Cells[totalsRow + 3, 1],
                    sheet.Cells[totalsRow + 5, 2]
                ].Font.Bold = true;

                workbook.SaveAs(sfd.FileName);
                workbook.Close();
                excel.Quit();

                MessageBox.Show($"Отчёт успешно создан!\nФайл сохранён по адресу:\n{sfd.FileName}","Готово",MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells["Статус"].Value == null)
                return;

            string status = dataGridView1.Rows[e.RowIndex].Cells["Статус"].Value.ToString().ToLower();

            if (status.Contains("заверш"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
            }
            else if (status.Contains("отмен"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
            }
            else if (status.Contains("создан"))
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
            }
            else
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
        }
    }
}
