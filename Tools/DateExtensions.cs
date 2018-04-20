using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public static class DateExtensions
    {
        public static long GetDiffSeconds(this DateTime dateTime, DateTime dateTime2)
        {
            TimeSpan ts1 = new TimeSpan(dateTime.Ticks);
            TimeSpan ts2 = new TimeSpan(dateTime2.Ticks);
            //return ts1.Seconds - ts2.Seconds;
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            //dateDiff = ts.Days.ToString() + "天"
            //        + ts.Hours.ToString() + "小时"
            //        + ts.Minutes.ToString() + "分钟"
            //        + ts.Seconds.ToString() + "秒";

            return ts.Days * 24 * 60 * 60 + ts.Hours * 60 * 60 + ts.Minutes * 60 + ts.Seconds;
        }
    }
}
