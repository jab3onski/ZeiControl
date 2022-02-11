using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static void SaveToNewTable(SQLiteConnection connection, string tablename)
        {
            if (Regex.IsMatch(tablename, @"^\d+") || tablename.Length < 4)
            {
                SaveSensorDataWindow.DefaultTableLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.TableExistsLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.ErrorLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.SuccessLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.IllegalCharLabel.Visibility = System.Windows.Visibility.Visible;
                SaveSensorDataWindow.TableNotExists.Visibility = System.Windows.Visibility.Hidden;
                return;
            }

            SQLiteCommand command;

            try
            {
                command = connection.CreateCommand();
                command.CommandText =
                    $"CREATE TABLE \"{tablename}\" (\"id\" INTEGER NOT NULL UNIQUE, \"sensorType\" TEXT NOT NULL, \"value\" INTEGER NOT NULL, \"sqlTimestamp\" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(\"id\" AUTOINCREMENT));";
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                SaveSensorDataWindow.DefaultTableLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.TableExistsLabel.Visibility = System.Windows.Visibility.Visible;
                SaveSensorDataWindow.ErrorLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.SuccessLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.IllegalCharLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.TableNotExists.Visibility = System.Windows.Visibility.Hidden;
                return;
            }

            SQLiteDataReader sqlite_datareader;
            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM temp;";

            sqlite_datareader = command.ExecuteReader();

            while(sqlite_datareader.Read())
            {
                string type = sqlite_datareader.GetString(1);
                int value = sqlite_datareader.GetInt32(2);
                DateTime dateTime = sqlite_datareader.GetDateTime(3);

                command = connection.CreateCommand();

                command.CommandText =
                    $"INSERT INTO \"{tablename}\" (sensorType, value, sqlTimestamp) VALUES ('{type}', {value}, '{dateTime}');";
                command.ExecuteNonQuery();
            }

            SaveSensorDataWindow.DefaultTableLabel.Visibility = System.Windows.Visibility.Hidden;
            SaveSensorDataWindow.TableExistsLabel.Visibility = System.Windows.Visibility.Hidden;
            SaveSensorDataWindow.ErrorLabel.Visibility = System.Windows.Visibility.Hidden;
            SaveSensorDataWindow.SuccessLabel.Visibility = System.Windows.Visibility.Visible;
            SaveSensorDataWindow.IllegalCharLabel.Visibility = System.Windows.Visibility.Hidden;
            SaveSensorDataWindow.TableNotExists.Visibility = System.Windows.Visibility.Hidden;
        }

        public static void AppendToExistingTable(SQLiteConnection connection, string tablename)
        {
            SQLiteCommand command;
            SQLiteDataReader sqlite_datareader;

            command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM sqlite_sequence WHERE name = '{tablename}';";
            sqlite_datareader = command.ExecuteReader();

            if(!sqlite_datareader.HasRows)
            {
                SaveSensorDataWindow.DefaultTableLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.TableExistsLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.ErrorLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.SuccessLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.IllegalCharLabel.Visibility = System.Windows.Visibility.Hidden;
                SaveSensorDataWindow.TableNotExists.Visibility = System.Windows.Visibility.Visible;
                return;
            }

            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM temp;";

            sqlite_datareader = command.ExecuteReader();

            while (sqlite_datareader.Read())
            {
                string type = sqlite_datareader.GetString(1);
                int value = sqlite_datareader.GetInt32(2);
                DateTime dateTime = sqlite_datareader.GetDateTime(3);

                command = connection.CreateCommand();

                command.CommandText =
                    $"INSERT INTO \"{tablename}\" (sensorType, value, sqlTimestamp) VALUES ('{type}', {value}, '{dateTime}');";
                command.ExecuteNonQuery();
            }

            SaveSensorDataWindow.DefaultTableLabel.Visibility = System.Windows.Visibility.Hidden;
            SaveSensorDataWindow.TableExistsLabel.Visibility = System.Windows.Visibility.Hidden;
            SaveSensorDataWindow.ErrorLabel.Visibility = System.Windows.Visibility.Hidden;
            SaveSensorDataWindow.SuccessLabel.Visibility = System.Windows.Visibility.Visible;
            SaveSensorDataWindow.IllegalCharLabel.Visibility = System.Windows.Visibility.Hidden;
            SaveSensorDataWindow.TableNotExists.Visibility = System.Windows.Visibility.Hidden;
        }

        public static void ClearSessionDataFromTemp(SQLiteConnection connection)
        {
            SQLiteCommand command;
            command = connection.CreateCommand();
            command.CommandText =
                $"DELETE FROM \"temp\";";
            command.ExecuteNonQuery();

            command = connection.CreateCommand();
            command.CommandText =
                $"UPDATE \"sqlite_sequence\" SET seq = 1 WHERE name = 'temp';";
            command.ExecuteNonQuery();
        }
    }
}
