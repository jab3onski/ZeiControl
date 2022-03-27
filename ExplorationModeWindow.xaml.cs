using System;
using System.Collections.Generic;
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
using ZeiControl.Core;

namespace ZeiControl
{
    public partial class ExplorationModeWindow : Window
    {
        public ExplorationModeWindow()
        {
            InitializeComponent();
        }

        //Move window override
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedInterval = CaptureIntervalBox.SelectedIndex;
            int selectedQuality = PictureQualityBox.SelectedIndex;
            int selectedTurnSharpness = TurnPresetBox.SelectedIndex;

            Trace.WriteLine(selectedInterval);
            Trace.WriteLine(selectedQuality);
            Trace.WriteLine(selectedTurnSharpness);

            MessagingProtocol.ProcessOutgoingData(
                HelperMethods.TransformToBytePacket16Bit(selectedInterval, 0x46, 0x00, 0x00));
            MessagingProtocol.ProcessOutgoingData(
                HelperMethods.TransformToBytePacket16Bit(selectedQuality, 0x46, 0x00, 0xFF));
            MessagingProtocol.ProcessOutgoingData(
                HelperMethods.TransformToBytePacket16Bit(selectedTurnSharpness, 0x46, 0xFF, 0x00));

            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.autonomousDrivingEnablePacket);
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
