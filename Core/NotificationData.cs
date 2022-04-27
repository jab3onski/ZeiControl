using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeiControl.Core
{
    internal class NotificationData
    {
        public string Severity { get; set; }

        public string Message { get; set; }

        //List of predefined notifications

        public static NotificationData LoginAttemptFailed = new() // IMPLEMENTED
        {
            Severity = "Info",
            Message = "Application failed to connect"
        };

        public static NotificationData LoginAttemptSuccessfull = new() // IMPLEMENTED
        {
            Severity = "Info",
            Message = "Connection ready"
        };

        public static NotificationData BatteryBelow50 = new() // IMPLEMENTED
        {
            Severity = "Low",
            Message = "Battery level is below 50%"
        };

        public static NotificationData BatteryBelow25 = new() // IMPLEMENTED
        {
            Severity = "Medium",
            Message = "Battery level is below 25%"
        };

        public static NotificationData BatteryBelow10 = new() // IMPLEMENTED
        {
            Severity = "High",
            Message = "Battery level is below 10%, shut down the device to prevent battery degradation"
        };

        public static NotificationData WiFiDisconnected = new() // IMPLEMENTED
        {
            Severity = "Info",
            Message = "Connection has been closed"
        };

        public static NotificationData WiFiInterrupted = new() // IMPLEMENTED
        {
            Severity = "High",
            Message = "Connection has been interrupted"
        };

        public static NotificationData SensorsActive = new()
        {
            Severity = "Info",
            Message = "At least one sensor is sending data"
        };

        public static NotificationData SensorsInactive = new()
        {
            Severity = "Info",
            Message = "All sensor data transfer is disabled"
        };

        public static NotificationData RSSILow = new() // IMPLEMENTED
        {
            Severity = "Medium",
            Message = "The signal strength is getting low, network performance has been lowered"
        };

        public static NotificationData RSSIVeryLow = new() // IMPLEMENTED
        {
            Severity = "High",
            Message = "The signal strength is very low, interruptions may occur"
        };

        public static NotificationData UptimeNearLimit = new() // IMPLEMENTED
        {
            Severity = "High",
            Message = "Internal timer of device MCU almost reached it's maximum value, device restart is advised"
        };

        public static NotificationData AutonomousDrivingActive = new() // IMPLEMENTED
        {
            Severity = "Info",
            Message = "Robot has entered autonomous exploration mode, camera is disabled"
        };

        public static NotificationData AutonomousDrivingInactive = new() // IMPLEMENTED
        {
            Severity = "Info",
            Message = "User has regained control of the robot, camera now may be enabled"
        };
    }
}
