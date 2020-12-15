using System;

namespace DiabNet.Nightscout
{
    public static class DateExtension
    {
        public static DateTimeOffset ToStartOfDay(this DateTimeOffset date)
        {
            return new(date.Date);
        }
        public static DateTimeOffset ToEndDay(this DateTimeOffset date)
        {
            return new(date.Year, date.Month, date.Day, 23, 59,59, date.Offset);
        }
    }
}
