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

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

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

        private void ForwardButtonClicked(object sender, RoutedEventArgs e)
        {
            byte[] bytePacket = { 0x23, 0x4D, 0x5F, 0xFF, 0x00, 0x00, 0x00, 0x23 };
            messagingProtocol.ProcessOutgoingData(bytePacket);
        }

        private void LeftButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void ReverseButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void RightButtonClicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
