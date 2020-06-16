using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital_Clock.Models
{
    internal class Alarm
    {
        public string _AlarmTime { get { return string.Format("{0:HH:mm}", AlarmTime); } set { } }
        public DateTime AlarmTime { get; set; } = DateTime.Now;
        public List<WeekDays> DaysToRepeat { get; set; } = new List<WeekDays>();
        public string Content { get; set; } = "Your alarm wants your attention";
        public bool Triggerd { get; set; } = false;
        public bool Activated { get; set; } = true;
    }

    public enum WeekDays : int
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }
}