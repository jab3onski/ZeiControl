﻿using System;
using System.Collections.Generic;
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

        public static Image StreamSourceFrame { get; set; }
        public static ListView DbListView { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            XsliderDragStarted = false;
            YsliderDragStarted = false;

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
            Close();
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
                HelperMethods.TransformAsBytePacket(
                    HelperMethods.RescaleToAnalogValue(((Slider)sender).Value), 0x58));

            XsliderDragStarted = false;
        }

        private void SliderXAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!XsliderDragStarted)
            {
                MessagingProtocol.ProcessOutgoingData(
                HelperMethods.TransformAsBytePacket(
                    HelperMethods.RescaleToAnalogValue(((Slider)sender).Value), 0x58));
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
                HelperMethods.TransformAsBytePacket(
                    HelperMethods.RescaleToAnalogValue(((Slider)sender).Value), 0x59));

            YsliderDragStarted = false;
        }

        private void SliderYAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!YsliderDragStarted)
            {
                MessagingProtocol.ProcessOutgoingData(
                    HelperMethods.TransformAsBytePacket(
                        HelperMethods.RescaleToAnalogValue(((Slider)sender).Value), 0x59));
            }
        }
    }
}
