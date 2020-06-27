using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital_Clock.Models
{
    class AlarmWithMethods : Alarm
    {
        public string GetTimeString()
        {
            return this.AlarmTime.ToString("hh':'mm");
        }

        public void SnoozeAlarm(int SnoozeTime = 10)
        {
            SnoozeAmount = SnoozeAmount + 1;
            AlarmTime.Add(new TimeSpan(0, SnoozeTime, 0));
            Snooze = true;
        }
        public void ResetAlarm(int SnoozeTime = 10)
        {
            AlarmTime.Subtract(new TimeSpan(0, SnoozeTime*SnoozeAmount, 0));
            SnoozeAmount = SnoozeAmount + 1;
            Snooze = false;
        }
    }
}
