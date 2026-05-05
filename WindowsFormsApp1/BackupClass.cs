using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    static class BackupClass
    {
        private static void CreateDatabaseDump(string fullPath)
        {
            Connect connect = new Connect();
            using (MySqlConnection conn = new MySqlConnection(connect.ConnectDB()))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(fullPath);
                        conn.Close();
                    }
                }
            }
        }

        public static string CreateBackupWithDialog(string selectedFolder)
        {
            if (!System.IO.Directory.Exists(selectedFolder))
            {
                throw new DirectoryNotFoundException($"Папка не найдена: {selectedFolder}");
            }

            string folderName = $"dump_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}";
            string backupDirectory = Path.Combine(selectedFolder, folderName);

            System.IO.Directory.CreateDirectory(backupDirectory);

            string fileName = $"dump_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.sql";
            string fullPath = Path.Combine(backupDirectory, fileName);

            CreateDatabaseDump(fullPath);

            return fullPath;
        }
    }
}
