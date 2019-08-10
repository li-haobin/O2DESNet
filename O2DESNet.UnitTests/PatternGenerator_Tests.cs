using NUnit.Framework;
using O2DESNet.Standard;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace O2DESNet.UnitTests
{
    public class PatternGenerator_Tests
    {
        [Test]
        public void NoSeasonality()
        {
            Debug.WriteLine("Seasonality - None");
            var diff = Test(new PatternGenerator.Statics
            {
                MeanHourlyRate = 1,
            }, 1000);
            if (Math.Abs(diff) > 0.05) Assert.Fail();
        }

        [Test]
        public void HoursInDay()
        {
            Debug.WriteLine("Seasonality - Hours in Day");
            var diff = Test(new PatternGenerator.Statics
            {
                MeanHourlyRate = 15,
                SeasonalFactors_HoursOfDay = new List<double>
                {
                    1, 2, 3, 3, 3, 3, 3, 10,
                    10, 10, 10, 9, 9, 8, 8, 8,
                    7, 6, 5, 4, 3, 2, 1, 0,
                },
            }, 1000);
            if (Math.Abs(diff) > 0.04) Assert.Fail();
        }

        [Test]
        public void DaysInWeek()
        {
            Debug.WriteLine("Seasonality - Days in Week");
            var diff = Test(new PatternGenerator.Statics
            {
                MeanHourlyRate = 1,
                SeasonalFactors_DaysOfWeek = new List<double>
                {
                    1, 2, 3, 3, 1, 0, 0,
                },
            }, 1000);
            if (Math.Abs(diff) > 0.05) Assert.Fail();
        }

        [Test]
        public void Combined_HoursInDay_DaysInWeek()
        {
            Debug.WriteLine("Seasonality - Combined_HoursInDay_DaysInWeek");
            var diff = Test(new PatternGenerator.Statics
            {
                MeanHourlyRate = 1,
                SeasonalFactors_HoursOfDay = new List<double>
                {
                    1, 2, 3, 3, 3, 3, 3, 10,
                    10, 10, 10, 9, 9, 8, 8, 8,
                    7, 6, 5, 4, 3, 2, 1, 0,
                },
                SeasonalFactors_DaysOfWeek = new List<double>
                {
                    1, 2, 3, 3, 1, 0, 0,
                },
            }, 1000);
            if (Math.Abs(diff) > 0.05) Assert.Fail();
        }

        [Test]
        public void DaysInMonth()
        {
            Debug.WriteLine("Seasonality - DaysInMonth");
            var diff = Test(new PatternGenerator.Statics
            {
                MeanHourlyRate = 0.5,
                SeasonalFactors_DaysOfMonth = new List<double>
                {
                    1, 1, 1, 1, 1, 1, 1,
                    2, 2, 2, 2, 2, 2, 2,
                    4, 4, 4, 4, 4, 4, 4,
                    3, 3, 3, 3, 3, 3, 3,
                },
            }, 2000);
            if (Math.Abs(diff) > 0.05) Assert.Fail();
        }

        [Test]
        public void MonthsInYear()
        {
            Debug.WriteLine("Seasonality - MonthsInYear");
            var diff = Test(new PatternGenerator.Statics
            {
                MeanHourlyRate = 0.05,
                SeasonalFactors_MonthsOfYear = new List<double>
                {
                    1, 1, 1, 1, 1, 1,
                    2, 2, 2, 2, 2, 2,
                },
            }, 4000);
            if (Math.Abs(diff) > 0.05) Assert.Fail();
        }

        [Test]
        public void Years()
        {
            Debug.WriteLine("Seasonality - Years");
            var diff = Test(new PatternGenerator.Statics
            {
                MeanHourlyRate = 0.05,
                SeasonalFactors_Years = new List<double>
                {
                    1, 2, 3,
                },
            }, 5000);
            if (Math.Abs(diff) > 0.05) Assert.Fail();
        }

        [Test]
        public void Customized()
        {
            Debug.WriteLine("Seasonality - Customized");
            var diff = Test(new PatternGenerator.Statics
            {
                MeanHourlyRate = 0.5,
                CustomizedSeasonalFactors = new List<(TimeSpan, List<double>)>
                {
                    (TimeSpan.FromHours(100), new List<double> { 1, 3, 9 }),
                    (TimeSpan.FromHours(1000), new List<double> { 1, 3, 9 }),
                },
            }, 3000);
            if (Math.Abs(diff) > 0.05) Assert.Fail();
        }

        [Test]
        public void Customized_On_and_Off()
        {
            Debug.WriteLine("Seasonality - Customized_On_and_Off");
            Test_On_and_Off(new PatternGenerator.Statics
            {
                MeanHourlyRate = 0.5,
                CustomizedSeasonalFactors = new List<(TimeSpan, List<double>)>
                {
                    (TimeSpan.FromHours(100), new List<double> { 1, 3, 9 }),
                    (TimeSpan.FromHours(1000), new List<double> { 1, 3, 9 }),
                },
            }, 100);
        }

        private double Test(PatternGenerator.Statics assets, int nEvents)
        {
            var gen = new PatternGenerator(assets, 0);
            gen.Start();
            for(int i = 0; i < nEvents; i++) gen.Run(1);

            var expected = 1 / assets.MeanHourlyRate * gen.Count;
            var observed = (gen.ClockTime - DateTime.MinValue).TotalHours;
            var diff = expected / observed - 1;
            return diff;
        }

        private void Test_On_and_Off(PatternGenerator.Statics config, int nEvents)
        {
            var gen = new PatternGenerator(config, 0);
            gen.Start();
            gen.Run(nEvents / 2);
            gen.End();
            gen.Run(TimeSpan.FromDays(3));
            gen.Start();
            gen.Run(nEvents / 2);
        }
    }
}
