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
using System.Windows.Controls.Primitives;
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
        public static bool AutonomousDrivingEnabled { get; set; }

        public static Image StreamSourceFrame { get; set; }             //Image object for stream display
        public static ListView DbListView { get; set; }                 //Sensor Updates ListView
        public static ListView NotificationsListView { get; set; }      //Notifications ListView
        public static TextBlock FrontSensorTextBlock { get; set; }
        public static TextBlock LeftSensorTextBlock { get; set; }
        public static TextBlock RightSensorTextBlock { get; set; }
        public static TextBlock RRSITextBlock { get; set; }
        public static TextBlock UptimeTextBlock { get; set; }
        public static TextBlock CellVoltageTextBlock { get; set; }

        public static Button SaveDataButton { get; set; }
        public static Button ClearDataButton { get; set; }
        public static Button SettingsButton { get; set; }

        public static Button MoveForwardButton { get; set; }
        public static Button MoveLeftButton { get; set; }
        public static Button MoveRightButton { get; set; }
        public static Button MoveReverseButton { get; set; }
        public static Button MoveReverseLeftButton { get; set; }
        public static Button MoveReverseRightButton { get; set; }
        public static ToggleButton BuzzerEnableButton { get; set; }
        public static ToggleButton PLEDEnableButton { get; set; }
        public static ToggleButton StopEnableButton { get; set; }

        public static Slider Xslider { get; set; }
        public static Slider Yslider { get; set; }
        public static ToggleButton EnableCameraButton { get; set; }
        public static Button ZeroOutAxisButton { get; set; }

        public static Button WiFiConnectButton { get; set; }
        public static Button CloseConnectionButton { get; set; }
        public static Button AutonomousDrivingButton { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            XsliderDragStarted = false;
            YsliderDragStarted = false;

            WiFiConnectButton = ConnectButton;
            AutonomousDrivingButton = ExplorationModeButton;

            Xslider = SliderXAxis;
            Yslider = SliderYAxis;
            SensorUpdatesCounter = 0;
            AutonomousDrivingEnabled = false;

            CloseConnectionButton = DisconnectButton;

            StreamSourceFrame = CameraFrameImage;
            DbListView = DatabaseItemsList;
            NotificationsListView = NotificationItemsList;
            FrontSensorTextBlock = FrontProxTBlock;
            LeftSensorTextBlock = LeftProxTBlock;
            RightSensorTextBlock = RightProxTBlock;
            RRSITextBlock = RSSITBlock;
            UptimeTextBlock = UptimeTBlock;
            CellVoltageTextBlock = CVoltageTBlock;

            SaveDataButton = SaveSensorDataButton;
            ClearDataButton = ClearSessionDataButton;
            SettingsButton = SensorSettingsButton;

            MoveForwardButton = ForwardButton;
            MoveLeftButton = LeftButton;
            MoveRightButton = RightButton;
            MoveReverseButton = ReverseButton;
            MoveReverseLeftButton = ReverseLeftButton;
            MoveReverseRightButton = ReverseRightButton;
            BuzzerEnableButton = BuzzerButton;
            PLEDEnableButton = PowerLEDButton;
            StopEnableButton = FullStopButton;

            EnableCameraButton = CameraButton;
            ZeroOutAxisButton = ZeroOutAxis;

            HelperMethods.ChangePropertyVariableWiFi(false);


            SQLiteConnection connection;
            connection = DatabaseHandling.CreateConnection();
            try
            {
                DatabaseHandling.ClearSessionDataFromTemp(connection);
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
            StreamSourceFrame.Source = new BitmapImage(new Uri("pack://application:,,,/Images/NoImage.png"));
        }


        //Handle X-axis slider drag event
        private void SliderXAxis_DragStarted(object sender, DragStartedEventArgs e)
        {
            XsliderDragStarted = true;
        }

        private void SliderXAxis_DragCompleted(object sender, DragCompletedEventArgs e)
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
        private void SliderYAxis_DragStarted(object sender, DragStartedEventArgs e)
        {
            YsliderDragStarted = true;
        }

        private void SliderYAxis_DragCompleted(object sender, DragCompletedEventArgs e)
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

        private void ExplorationModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AutonomousDrivingEnabled)
            {
                ExplorationModeWindow explorationModeWindow = new();
                explorationModeWindow.Owner = this;
                explorationModeWindow.Show();
            }
            else
            {
                MessagingProtocol.ProcessOutgoingData(MessagingProtocol.autonomousDrivingDisablePacket);
            }
        }
    }
}
