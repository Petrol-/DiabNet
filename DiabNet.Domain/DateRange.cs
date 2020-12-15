using System;

namespace DiabNet.Domain
{
    public class DateRange
    {
        public DateRange(DateTimeOffset from, DateTimeOffset to)
        {
            if (from > to) throw new ArgumentException($"{nameof(from)} cannot be after {nameof(to)}");

            From = from;
            To = to;
        }
        public  DateTimeOffset From { get; }
        public  DateTimeOffset To { get; }
    }
}
