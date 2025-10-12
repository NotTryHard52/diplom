using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public static class CountData
    {
        public static int GetTableCount(string tableName)
        {
            Connect connect = new Connect();
            string connectionString = connect.ConnectDB();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = $"SELECT COUNT(*) FROM `{tableName}`";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}
