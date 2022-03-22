using System;
using System.Collections.Generic;
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
            MessagingProtocol.ProcessOutgoingData(MessagingProtocol.autonomousDrivingEnablePacket);
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
