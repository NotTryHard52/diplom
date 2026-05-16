using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Backup : Form
    {
        string path;
        public Backup()
        {
            InitializeComponent();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите папку для сохранения резервной копии";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    path = folderDialog.SelectedPath;
                    label2.Text = "*путь выбран";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (label2.Text == "*путь не выбран")
            {
                MessageBox.Show("Сначала выберите папку для сохранения", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string backupPath = BackupClass.CreateBackupWithDialog(path);

                MessageBox.Show($"Резервная копия успешно создана!\n\nПуть: {backupPath}",
                              "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании резервной копии:\n{ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
