using System;
using System.Linq;
using NUnit.Framework;

namespace DiabNet.Domain.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SplitByDay_should_return_same_range_when_TotalTime_less_than_days()
        {
            var date = DateTimeOffset.Now;
            var range = new DateRange(date, date.AddDays(2));
            var split = range.SplitByDay(5);

            Assert.AreSame(range, split.First());
        }

        [Test]
        public void SplitByDay_should_return_multiple_ranges_from_Start_to_end()
        {
            var date = DateTimeOffset.Now;
            var range = new DateRange(date, date.AddDays(2).AddHours(1));
            var split = range.SplitByDay(1);

            Assert.AreEqual(3, split.Count());
        }
    }
}
