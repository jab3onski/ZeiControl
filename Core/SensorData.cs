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

        public int SensorValue { get; set; }

        public DateTime DateTimeValue { get; set; }
    }
}
