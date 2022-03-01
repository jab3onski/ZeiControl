using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZeiControl.Core;

namespace ZeiControl
{
    public partial class MainWindow : Window
    {
        readonly NetworkHandling nwHandler = new();

        private bool XsliderDragStarted;
        private bool YsliderDragStarted;

        public static int SensorUpdatesCounter { get; set; }

        public static Image StreamSourceFrame { get; set; }
        public static ListView DbListView { get; set; }

        private Slider Xslider { get; set; }
        private Slider Yslider { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            XsliderDragStarted = false;
            YsliderDragStarted = false;

            Xslider = SliderXAxis;
            Yslider = SliderYAxis;

            SensorUpdatesCounter = 0;

            StreamSourceFrame = CameraFrameImage;
            DbListView = DatabaseItemsList;
        }

        //Move window override
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        //Connection buttons handling and exit
        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            nwHandler.StartConnectionStream();
        }

        private void DisconnectButtonClick(object sender, RoutedEventArgs e)
        {
            nwHandler.StopConnectionStream();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            nwHandler.StopConnectionStream();
            Application.Current.Shutdown();
        }


        //Camera enabled / disabled
        private void CameraEnableChecked(object sender, RoutedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.cameraEnablePacket);
        }

        private void CameraEnableUnchecked(object sender, RoutedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.cameraDisablePacket);
        }


        //Handle X-axis slider drag event
        private void SliderXAxis_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            XsliderDragStarted = true;
        }

        private void SliderXAxis_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(
                HelperMethods.TransformAsBytePacket32Bit(((Slider)sender).Value, 0x58));

            XsliderDragStarted = false;
        }

        private void SliderXAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!XsliderDragStarted)
            {
                MessagingProtocol.ProcessOutgoingData(
                    HelperMethods.TransformAsBytePacket32Bit(((Slider)sender).Value, 0x58));
            }
        }

        //Handle Y-axis slider drag event
        private void SliderYAxis_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            YsliderDragStarted = true;
        }

        private void SliderYAxis_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(
                HelperMethods.TransformAsBytePacket32Bit(((Slider)sender).Value, 0x59));

            YsliderDragStarted = false;
        }

        private void SliderYAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!YsliderDragStarted)
            {
                MessagingProtocol.ProcessOutgoingData(
                    HelperMethods.TransformAsBytePacket32Bit(((Slider)sender).Value, 0x59));
            }
        }

        private void LeftButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.leftPacket);
        }

        private void LeftButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.stopPacket);
        }

        private void ForwardButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.forwardPacket);
        }

        private void ForwardButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.stopPacket);
        }

        private void RightButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.rightPacket);
        }

        private void RightButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.stopPacket);
        }

        private void ReverseButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.reversePacket);
        }

        private void ReverseButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.stopPacket);
        }

        private void ReverseLeftButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.reverseLeftPacket);
        }

        private void ReverseLeftButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.stopPacket);
        }

        private void ReverseRightButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.reverseRightPacket);
        }

        private void ReverseRightButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.stopPacket);
        }

        private void PowerLEDButton_Checked(object sender, RoutedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.powerLEDonPacket);
        }

        private void PowerLEDButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.powerLEDoffPacket);
        }

        private void FullStopButton_Checked(object sender, RoutedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.emergencyStopPacket);
        }

        private void FullStopButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.emergencyOverPacket);
        }

        private void BuzzerButton_Checked(object sender, RoutedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.buzzerOnPacket);
        }

        private void BuzzerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.buzzerOffPacket);
        }

        private void DatabaseBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseBrowserWindow databaseBrowserWindow = new();
            databaseBrowserWindow.Owner = this;
            databaseBrowserWindow.Show();
        }

        private void SaveSensorDataButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSensorDataWindow saveSensorDataWindow = new();
            saveSensorDataWindow.Owner = this;
            _ = saveSensorDataWindow.ShowDialog();
        }

        private void ClearSessionDataButton_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection connection;
            connection = DatabaseHandling.CreateConnection();

            try
            {
                DatabaseHandling.ClearSessionDataFromTemp(connection);
                DbListView.Items.Clear();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception encountered: ");
                Trace.WriteLine(ex.Message);
            }
            connection.Close();
        }

        private void ZeroOutAxis_Click(object sender, RoutedEventArgs e)
        {
            Xslider.Value = 90;
            Yslider.Value = 90;
        }

        private void SensorSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SensorSettingsWindow sensorSettingsWindow = new();
            sensorSettingsWindow.Owner = this;
            sensorSettingsWindow.Show();
        }
    }
}
