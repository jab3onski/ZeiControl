using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeiControl.Core
{
    class DatabaseHandling
    {
        public static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn =
                new("Data Source=./Core/database.db");
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception encountered: ");
                Trace.WriteLine(e.Message);
            }
            return sqlite_conn;
        }

        public static void AddSensorEntryToTemp(SQLiteConnection connection, string type, int value)
        {
            SQLiteCommand command;
            command = connection.CreateCommand();
            command.CommandText =
                $"INSERT INTO temp (sensorType, value) VALUES ('{type}', {value});";
            command.ExecuteNonQuery();
        } 
    }
}
