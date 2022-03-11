using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class SensorSettingsWindow : Window
    {
        private static readonly Regex allowOnlyNumbersRegex = new("[^0-9]+");

        public TextBox TextBoxTempInterval { get; set; }
        public TextBox TextBoxProximityInterval { get; set; }
        public Label LabSuccessful { get; set; }
        public Label LabFailed { get; set; }

        public SensorSettingsWindow()
        {
            InitializeComponent();

            TextBoxTempInterval = TemperatureIntervalBox;
            TextBoxProximityInterval = ProximityIntervalBox;
            LabSuccessful = LabelSuccessful;
            LabFailed = LabelFailed;
        }

        private static bool IsCharacterLegal(string text)
        {
            return !allowOnlyNumbersRegex.IsMatch(text);
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

        private void ProximityIntervalBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsCharacterLegal(e.Text);
        }

        private void TemperatureIntervalBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsCharacterLegal(e.Text);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxTempInterval.Text.Length > 0 || TextBoxProximityInterval.Text.Length > 0)
            {
                if (ushort.TryParse(TextBoxTempInterval.Text, out ushort tempInterval) &&
                    TextBoxTempInterval.Text.Length > 0 && tempInterval >= 500)
                {
                    MessagingProtocol.SendMessageTempInterval(tempInterval);
                    LabSuccessful.Visibility = Visibility.Visible;
                    LabFailed.Visibility = Visibility.Hidden;
                }

                if (ushort.TryParse(TextBoxProximityInterval.Text, out ushort proximityInterval) &&
                    TextBoxProximityInterval.Text.Length > 0 && proximityInterval >= 500)
                {
                    MessagingProtocol.SendMessageProximityInterval(proximityInterval);
                    LabSuccessful.Visibility = Visibility.Visible;
                    LabFailed.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                LabSuccessful.Visibility = Visibility.Hidden;
                LabFailed.Visibility = Visibility.Visible;
            }
        }
    }
}
