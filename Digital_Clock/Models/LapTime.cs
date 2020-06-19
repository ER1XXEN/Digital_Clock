using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Digital_Clock.Models
{
    class LapTime
    {
        public int Number { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan TimeSinceLast { get; set; }

        public LapTime(int Number, TimeSpan CurrentTime, TimeSpan LastTime)
        {
            this.Number = Number;
            this.TotalTime = CurrentTime;
            this.TimeSinceLast = CurrentTime.Subtract(LastTime);
        }
    }
}
