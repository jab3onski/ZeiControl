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

    public partial class SaveSensorDataWindow : Window
    {
        public static TextBox InputTableNameTextBox { get; set; }
        public static Label DefaultTableLabel { get; set; }
        public static Label TableExistsLabel { get; set; }
        public static Label ErrorLabel { get; set; }
        public static Label SuccessLabel { get; set; }
        public static Label IllegalCharLabel { get; set; }
        public static Label TableNotExists { get; set; }

        public SaveSensorDataWindow()
        {
            InitializeComponent();

            InputTableNameTextBox = TableNameBox;
            DefaultTableLabel = DefaultNameLabel;
            TableExistsLabel = TableNameExistsLabel;
            ErrorLabel = ErrorNameLabel;
            SuccessLabel = TableNameSuccessLabel;
            IllegalCharLabel = IllegalCharacterLabel;
            TableNotExists = TableNotExistsLabel;
        }

        //Move window override
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void CloseSaveDataWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveTableButton_Click(object sender, RoutedEventArgs e)
        {
            string tableName = InputTableNameTextBox.Text;

            SQLiteConnection connection;
            connection = Core.DatabaseHandling.CreateConnection();

            try
            {
                Core.DatabaseHandling.SaveToNewTable(connection, tableName);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception encountered: ");
                Trace.WriteLine(ex.Message);

                DefaultTableLabel.Visibility = Visibility.Hidden;
                TableExistsLabel.Visibility = Visibility.Hidden;
                ErrorLabel.Visibility = Visibility.Visible;
                SuccessLabel.Visibility = Visibility.Hidden;
                IllegalCharLabel.Visibility = Visibility.Hidden;
                TableNotExists.Visibility = Visibility.Hidden;
            }
            connection.Close();
        }

        private void AppendTableButton_Click(object sender, RoutedEventArgs e)
        {
            string tableName = InputTableNameTextBox.Text;

            SQLiteConnection connection;
            connection = Core.DatabaseHandling.CreateConnection();

            try
            {
                Core.DatabaseHandling.AppendToExistingTable(connection, tableName);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception encountered: ");
                Trace.WriteLine(ex.Message);

                DefaultTableLabel.Visibility = Visibility.Hidden;
                TableExistsLabel.Visibility = Visibility.Hidden;
                ErrorLabel.Visibility = Visibility.Visible;
                SuccessLabel.Visibility = Visibility.Hidden;
                IllegalCharLabel.Visibility = Visibility.Hidden;
                TableNotExists.Visibility = Visibility.Hidden;
            }
            connection.Close();
        }
    }
}
