using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        public static void AddSensorEntryToTemp(SQLiteConnection connection, string type, double value)
        {
            SQLiteCommand command;
            command = connection.CreateCommand();
            command.CommandText =
                $"INSERT INTO temp (sensorType, value) VALUES ('{type}', '{value}');";
            _ = command.ExecuteNonQuery();
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
                    $"CREATE TABLE \"{tablename}\" (\"id\" INTEGER NOT NULL UNIQUE, \"sensorType\" TEXT NOT NULL, \"value\" REAL NOT NULL, \"sqlTimestamp\" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(\"id\" AUTOINCREMENT));";
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
                string valueString = sqlite_datareader.GetString(2).Replace(",", ".");
                double value = Convert.ToDouble(valueString, CultureInfo.InvariantCulture);
                string dateTime = sqlite_datareader.GetString(3);

                command = connection.CreateCommand();

                command.CommandText =
                    $"INSERT INTO \"{tablename}\" (sensorType, value, sqlTimestamp) VALUES ('{type}', '{value}', '{dateTime}');";
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

            if (tablename is "temp" or "sqlite_sequence")
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
                string valueString = sqlite_datareader.GetString(2).Replace(",", ".");
                double value = Convert.ToDouble(valueString, CultureInfo.InvariantCulture);
                string dateTime = sqlite_datareader.GetString(3);

                command = connection.CreateCommand();

                command.CommandText =
                    $"INSERT INTO \"{tablename}\" (sensorType, value, sqlTimestamp) VALUES ('{type}', '{value}', '{dateTime}');";
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

        public static void ListAllAvailableTables(SQLiteConnection connection)
        {
            SQLiteCommand command;
            SQLiteDataReader sqlite_datareader;

            command = connection.CreateCommand();
            command.CommandText = $"SELECT name FROM \"sqlite_sequence\" WHERE name NOT LIKE 'temp' AND" +
                $" name NOT LIKE 'sqlite_sequence';";
            sqlite_datareader = command.ExecuteReader();

            while (sqlite_datareader.Read())
            {
                string tableName = sqlite_datareader.GetString(0);
                DatabaseBrowserWindow.SelectedTableBox.Items.Add(tableName);
            }
        }

        public static void DisplayTableContents(SQLiteConnection connection, string tableName)
        {
            SQLiteCommand command;
            SQLiteDataReader sqlite_datareader;

            DatabaseBrowserWindow.TableItemsList.Items.Clear();

            command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM \"{tableName}\";";
            sqlite_datareader = command.ExecuteReader();

            while (sqlite_datareader.Read())
            {
                int id = sqlite_datareader.GetInt32(0);
                string sensorType = sqlite_datareader.GetString(1);
                string valueString = sqlite_datareader.GetString(2).Replace(",", ".");
                double sensorValue = Convert.ToDouble(valueString, CultureInfo.InvariantCulture);
                string dateTimeValue = sqlite_datareader.GetString(3);

                _ = DatabaseBrowserWindow.TableItemsList.Items.Add(
                            new SensorData { Id = id, SensorType = sensorType, SensorValue = sensorValue, DateTimeValue = dateTimeValue });
            }
        }

        public static void DropTableFromDatabase(SQLiteConnection connection, string tableName)
        {
            SQLiteCommand command;
            command = connection.CreateCommand();
            command.CommandText =
                $"DROP TABLE IF EXISTS \"{tableName}\";";
            command.ExecuteNonQuery();
        }

        public static void CreateCSVFile(SQLiteConnection connection, string tableName)
        {
            SQLiteCommand command;
            SQLiteDataReader sqlite_datareader;

            command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM \"{tableName}\";";
            sqlite_datareader = command.ExecuteReader();


            List<SensorDataCSV> sensorDataList = new();

            while (sqlite_datareader.Read())
            {
                int id = sqlite_datareader.GetInt32(0);
                string sensorType = sqlite_datareader.GetString(1);
                string valueString = sqlite_datareader.GetString(2);
                string dateTimeValue = sqlite_datareader.GetString(3);

                sensorDataList.Add(new SensorDataCSV
                { Id = id, SensorType = sensorType, SensorValue = valueString, DateTimeValue = dateTimeValue });
            }

            DateTimeOffset currentDateTime = DateTime.UtcNow;
            string currentTimeStamp = currentDateTime.ToUnixTimeSeconds().ToString();

            string path = "./CSV/" + $"{tableName}" + "_" + currentTimeStamp + ".csv";

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };

            using StreamWriter streamWriter = new(path);
            using CsvWriter csvWriter = new(streamWriter, config);
            csvWriter.WriteRecords(sensorDataList);
            streamWriter.Flush();
        }
    }
}
