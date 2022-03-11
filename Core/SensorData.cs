using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeiControl.Core
{
    class SensorData
    {
        public int Id { get; set; }

        public string SensorType { get; set; }

        public double SensorValue { get; set; }

        public string DateTimeValue { get; set; }
    }
}
