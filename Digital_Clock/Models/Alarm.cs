﻿using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

namespace Digital_Clock.Models

{
    internal class Alarm

    {
        public string Content { get; set; } = "Your alarm wants your attention";
        public bool Activated { get; set; } = true;
        public bool Snooze { get; set; } = false;
        public int SnoozeAmount { get; set; } = 0;
        public TimeSpan AlarmTime { get; set; } = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
        public List<WeekDays> DaysToRepeat { get; set; } = new List<WeekDays>();
    }

    public enum WeekDays
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