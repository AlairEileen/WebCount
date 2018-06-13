using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Date
{
    public static class NumberToDateConvert
    {
        public static DateTime ToUnixDateTime(this long num)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1),TimeZoneInfo.Local);
            long lTime = long.Parse(num + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow).ToLocalTime();
        }
    }
}
