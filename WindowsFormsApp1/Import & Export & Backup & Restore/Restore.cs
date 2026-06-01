using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Import___Export___Backup___Restore
{
    public partial class Restore : Form
    {
        private string selectedBackupFile = "";
        public Restore()
        {
            InitializeComponent();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "SQL files (*.sql)|*.sql";
            ofd.Title = "Выберите резервную копию";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedBackupFile = ofd.FileName;

                label2.Text = "*файл выбран";
                label3.Text = selectedBackupFile;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedBackupFile))
            {
                MessageBox.Show("Сначала выберите backup файл","Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                BackupClass.RestoreBackup(selectedBackupFile);

                MessageBox.Show("База данных успешно восстановлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка восстановления:\n{ex.Message}","Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
