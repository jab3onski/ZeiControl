using System;
using System.Collections.Generic;
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
        NetworkHandling nwHandler = new();
        MessagingProtocol messagingProtocol = new();

        //Protocol message to stop driving in current direction
        byte[] stopPacket = { 0x23, 0x4D, 0x5F, 0xFF, 0xFF, 0xFF, 0xFF, 0x23 };

        public MainWindow()
        {
            InitializeComponent();
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
            Close();
        }

        //Drive left mouse button handling
        private void MouseOnLeftButtonPressed(object sender, MouseButtonEventArgs e)
        {
            byte[] bytePacket = { 0x23, 0x4D, 0x5F, 0x00, 0xFF, 0x00, 0x00, 0x23 };
            messagingProtocol.ProcessOutgoingData(bytePacket);
        }

        private void MouseOnLeftButtonReleased(object sender, MouseButtonEventArgs e)
        {
            messagingProtocol.ProcessOutgoingData(stopPacket);
        }

        //Drive reverse mouse button handling
        private void MouseOnReverseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            byte[] bytePacket = { 0x23, 0x4D, 0x5F, 0x00, 0x00, 0x00, 0xFF, 0x23 };
            messagingProtocol.ProcessOutgoingData(bytePacket);
        }

        private void MouseOnReverseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            messagingProtocol.ProcessOutgoingData(stopPacket);
        }

        //Drive right mouse button handling
        private void MouseOnRightButtonPressed(object sender, MouseButtonEventArgs e)
        {
            byte[] bytePacket = { 0x23, 0x4D, 0x5F, 0x00, 0x00, 0xFF, 0x00, 0x23 };
            messagingProtocol.ProcessOutgoingData(bytePacket);
        }

        private void MouseOnRightButtonReleased(object sender, MouseButtonEventArgs e)
        {
            messagingProtocol.ProcessOutgoingData(stopPacket);
        }

        //Drive forward mouse button handling
        private void MouseOnForwardButtonPressed(object sender, MouseButtonEventArgs e)
        {
            byte[] bytePacket = { 0x23, 0x4D, 0x5F, 0xFF, 0x00, 0x00, 0x00, 0x23 };
            messagingProtocol.ProcessOutgoingData(bytePacket);
        }

        private void MouseOnForwardButtonReleased(object sender, MouseButtonEventArgs e)
        {
            messagingProtocol.ProcessOutgoingData(stopPacket);
        }

        //Drive full stop (relays closed)
        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            byte[] bytePacket = { 0x23, 0x4D, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x23 };
            messagingProtocol.ProcessOutgoingData(bytePacket);
        }
    }
}
