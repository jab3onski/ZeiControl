using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZeiControl
{
    public partial class DatabaseBrowserWindow : Window
    {
        public static ComboBox SelectedTableBox { get; set; }
        public static ListView TableItemsList { get; set; }
        public static Button DeleteSelectedTable { get; set; }

        private bool displayNeeded;

        public DatabaseBrowserWindow()
        {
            InitializeComponent();

            SelectedTableBox = SelectTableComboBox;
            TableItemsList = DatabaseItemsListView;
            DeleteSelectedTable = DeleteSelButton;

            displayNeeded = true;

            SQLiteConnection connection;
            connection = Core.DatabaseHandling.CreateConnection();

            try
            {
                Core.DatabaseHandling.ListAllAvailableTables(connection);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception encountered: ");
                Trace.WriteLine(ex.Message);
            }
            connection.Close();
        }

        //Move window override
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectTableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (displayNeeded)
            {
                string tableName = SelectedTableBox.SelectedItem.ToString();

                SQLiteConnection connection;
                connection = Core.DatabaseHandling.CreateConnection();

                try
                {
                    Core.DatabaseHandling.DisplayTableContents(connection, tableName);
                    DeleteSelectedTable.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception encountered: ");
                    Trace.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }

        private void DeleteSelButton_Click(object sender, RoutedEventArgs e)
        {
            string tableName = SelectedTableBox.SelectedItem.ToString();
            displayNeeded = false;

            SQLiteConnection connection;
            connection = Core.DatabaseHandling.CreateConnection();

            try
            {
                Core.DatabaseHandling.DropTableFromDatabase(connection, tableName);
                SelectedTableBox.Items.Remove(SelectedTableBox.SelectedItem);
                SelectedTableBox.SelectedIndex = -1;
                TableItemsList.Items.Clear();
                DeleteSelectedTable.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception encountered: ");
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                displayNeeded = true;
            }
            connection.Close();
        }
    }
}
