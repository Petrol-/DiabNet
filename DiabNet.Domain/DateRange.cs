using System;
using System.Collections.Generic;
using System.Linq;

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

        public DateTimeOffset From { get; }
        public DateTimeOffset To { get; }

        private TimeSpan TotalTime => To - From;

        public IEnumerable<DateRange> SplitByDay(int days)
        {
            if (TotalTime.TotalDays < days)
            {
                return new[] {this};
            }

            var totalSlices = (int) Math.Floor(TotalTime.TotalDays / days);
            var ranges = Enumerable.Range(0, totalSlices).Select(slice =>
            {
                var from = From.AddDays(days * slice);
                var to = from.AddDays(days);
                return new DateRange(from, to);
            }).ToList();

            var lastRange = ranges.Last();
            if (lastRange.To < To)
            {
                ranges.Add(new DateRange(lastRange.To, To));
            }
            return ranges;
        }
    }
}
