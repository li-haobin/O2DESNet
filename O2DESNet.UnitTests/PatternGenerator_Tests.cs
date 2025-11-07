using Microsoft.Extensions.Logging;

using NUnit.Framework;

using O2DESNet.Standard;

using Serilog;
using Serilog.Events;

using System;
using System.Collections.Generic;

namespace O2DESNet.UnitTests;

/// <summary>
/// Test suite for validating the behavior of the Standard.PatternGenerator under a variety of
/// seasonality configurations and lifecycle scenarios. The tests focus on two core aspects:
/// (1) statistical consistency of the long‑run average event rate and (2) robustness of the
/// start/stop lifecycle.
/// 
/// Overview
/// - The PatternGenerator emits events whose inter‑arrival process is governed by a baseline
///   MeanHourlyRate and optionally modulated by seasonal factors at different calendar granularities
///   (hour of day, day of week, day of month, month of year, year) and by custom user‑defined
///   periodicities.
/// - Each test constructs a PatternGenerator.Statics configuration, starts the generator, and runs
///   until a certain number of events have been generated (except the on/off test which exercises
///   lifecycle without asserting rate accuracy).
/// - The helper Test method compares the expected elapsed time, derived from the configured
///   baseline rate and number of generated events, against the observed simulation clock time. The
///   relative difference must stay within a small tolerance, accounting for stochastic variance.
/// 
/// What is validated
/// - NoSeasonality: Verifies that, with only MeanHourlyRate set, the observed time to produce N
///   events is consistent with the reciprocal rate (1 / MeanHourlyRate) times the event count.
/// - HoursInDay, DaysInWeek, DaysInMonth, MonthsInYear, Years: Ensures that enabling a single
///   seasonal dimension (with lists sized 24, 7, 31, 12, or a finite year list) still yields the
///   correct long‑run average rate when integrated across the seasonal cycle.
/// - Combined_HoursInDay_DaysInWeek: Ensures multiple seasonal dimensions can be enabled together
///   without breaking the average‑rate property.
/// - Customized: Uses CustomizedSeasonalFactors to validate arbitrary user‑defined periods and
///   factor sequences behave consistently with the baseline rate in the long run.
/// - Customized_On_and_Off: Exercises Start/End transitions mid‑simulation to ensure the generator
///   can be paused and resumed safely, and that advancing the clock while stopped does not cause
///   errors. This test is primarily about robustness (no assertion on average rate).
/// 
/// How the checks work
/// - After generating N events (by repeatedly calling Run(1)), the test computes:
///     expected = (1 / MeanHourlyRate) * Count
///     observed = ClockTime.TotalHours
///     diff = expected / observed - 1
///   The assertion gates on |diff| being smaller than a tolerance (typically 0.05, 0.04 for the
///   most volatile case). Tolerances vary slightly to accommodate variance introduced by strong
///   seasonal modulation and finite sample sizes.
/// 
/// Reproducibility and logging
/// - A fixed seed is used to keep tests reproducible.
/// - Serilog is configured in SetUp to log to console and a file, and bridged into
///   Microsoft.Extensions.Logging so test output can be correlated with simulation steps when
///   diagnosing failures.
/// 
/// Scope
/// - These tests target average‑rate and lifecycle behavior. They do not attempt to verify the
///   exact distributional form of inter‑arrival times beyond the long‑run rate implied by the input
///   configuration, nor do they validate a particular combination rule across seasonal dimensions
///   beyond its effect on long‑run averages.
/// </summary>
[TestFixture]
public class PatternGenerator_Tests
{
    private Microsoft.Extensions.Logging.ILogger _logger;
    private LogEventLevel _minLevel = LogEventLevel.Information;

    [SetUp]
    public void Init()
    {
        // Comment out the following code to switch log file
        //ConfigureSerilog("Logs\\log-PatternGenerator_Tests.txt");
    }

    private void ConfigureSerilog(string filePath)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(_minLevel)
            .WriteTo.Console()      // Output to console
            .WriteTo.File(filePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                restrictedToMinimumLevel: _minLevel,
                shared: true)       // Output to file
            .Enrich.FromLogContext()
            .CreateLogger();

        _logger = new LoggerFactory()
            .AddSerilog(Log.Logger)
            .CreateLogger<PatternGenerator_Tests>();
    }

    [Test]
    public void EventRate_NoSeasonality_MatchesExpectedAverage()
    {
        TestContext.Out.WriteLine("Seasonality - None");
        var diff = Test(new PatternGenerator.Statics
        {
            MeanHourlyRate = 1,
        }, 1000);
        Assert.That(Math.Abs(diff), Is.LessThanOrEqualTo(0.05),
            $"Absolute relative difference {Math.Abs(diff):P2} exceeded tolerance 5%.");
    }

    [Test]
    public void EventRate_WithHourlySeasonality_MatchesExpectedAverage()
    {
        TestContext.Out.WriteLine("Seasonality - Hours in Day");
        var statics = new PatternGenerator.Statics
        {
            MeanHourlyRate = 15,
        };
        statics.SeasonalFactors_HoursOfDay.AddRange(
        [
            1, 2, 3, 3, 3, 3, 3, 10,
            10, 10, 10, 9, 9, 8, 8, 8,
            7, 6, 5, 4, 3, 2, 1, 0,
        ]);
        var diff = Test(statics, 1000);
        Assert.That(Math.Abs(diff), Is.LessThanOrEqualTo(0.04),
            $"Absolute relative difference {Math.Abs(diff):P2} exceeded tolerance 4%.");
    }

    [Test]
    public void EventRate_WithWeeklySeasonality_MatchesExpectedAverage()
    {
        TestContext.Out.WriteLine("Seasonality - Days in Week");
        var statics = new PatternGenerator.Statics
        {
            MeanHourlyRate = 1,
        };
        statics.SeasonalFactors_DaysOfWeek.AddRange(
        [
            1, 2, 3, 3, 1, 0, 0,
        ]);
        var diff = Test(statics, 1000);
        Assert.That(Math.Abs(diff), Is.LessThanOrEqualTo(0.05),
            $"Absolute relative difference {Math.Abs(diff):P2} exceeded tolerance 5%.");
    }

    [Test]
    public void EventRate_WithHourlyAndWeeklySeasonality_MatchesExpectedAverage()
    {
        TestContext.Out.WriteLine("Seasonality - Combined_HoursInDay_DaysInWeek");
        var statics = new PatternGenerator.Statics
        {
            MeanHourlyRate = 1,
        };
        statics.SeasonalFactors_HoursOfDay.AddRange(
        [
            1, 2, 3, 3, 3, 3, 3, 10,
            10, 10, 10, 9, 9, 8, 8, 8,
            7, 6, 5, 4, 3, 2, 1, 0,
        ]);
        statics.SeasonalFactors_DaysOfWeek.AddRange(
        [
            1, 2, 3, 3, 1, 0, 0,
        ]);
        var diff = Test(statics, 1000);
        Assert.That(Math.Abs(diff), Is.LessThanOrEqualTo(0.05),
            $"Absolute relative difference {Math.Abs(diff):P2} exceeded tolerance 5%.");
    }

    [Test]
    public void EventRate_WithDayOfMonthSeasonality_MatchesExpectedAverage()
    {
        TestContext.Out.WriteLine("Seasonality - DaysInMonth");
        var statics = new PatternGenerator.Statics
        {
            MeanHourlyRate = 0.5,
        };
        statics.SeasonalFactors_DaysOfMonth.AddRange(
        [
            1, 1, 1, 1, 1, 1, 1,
            2, 2, 2, 2, 2, 2, 2,
            4, 4, 4, 4, 4, 4, 4,
            3, 3, 3, 3, 3, 3, 3,
        ]);
        var diff = Test(statics, 2000);
        Assert.That(Math.Abs(diff), Is.LessThanOrEqualTo(0.05),
            $"Absolute relative difference {Math.Abs(diff):P2} exceeded tolerance 5%.");
    }

    [Test]
    public void EventRate_WithMonthOfYearSeasonality_MatchesExpectedAverage()
    {
        TestContext.Out.WriteLine("Seasonality - MonthsInYear");
        var statics = new PatternGenerator.Statics
        {
            MeanHourlyRate = 0.05,
        };
        statics.SeasonalFactors_MonthsOfYear.AddRange(
        [
            1, 1, 1, 1, 1, 1,
            2, 2, 2, 2, 2, 2,
        ]);
        var diff = Test(statics, 4000);
        Assert.That(Math.Abs(diff), Is.LessThanOrEqualTo(0.05),
            $"Absolute relative difference {Math.Abs(diff):P2} exceeded tolerance 5%.");
    }

    // Fix for CS0200: Use collection initializer to add elements to the read-only property instead of assignment.
    [Test]
    public void EventRate_WithYearSeasonality_MatchesExpectedAverage()
    {
        TestContext.Out.WriteLine("Seasonality - Years");
        var statics = new PatternGenerator.Statics
        {
            MeanHourlyRate = 0.05,
        };
        statics.SeasonalFactors_Years.AddRange([1.0, 2.0, 3.0]);
        var diff = Test(statics, 5000);
        Assert.That(Math.Abs(diff), Is.LessThanOrEqualTo(0.05),
            $"Absolute relative difference {Math.Abs(diff):P2} exceeded tolerance 5%.");
    }

    [Test]
    public void EventRate_WithCustomizedSeasonalFactors_MatchesExpectedAverage()
    {
        TestContext.Out.WriteLine("Seasonality - Customized");
        var statics = new PatternGenerator.Statics
        {
            MeanHourlyRate = 0.5,
        };
        statics.CustomizedSeasonalFactors.Add((TimeSpan.FromHours(100), new List<double> { 1, 3, 9 }));
        statics.CustomizedSeasonalFactors.Add((TimeSpan.FromHours(1000), new List<double> { 1, 3, 9 }));
        var diff = Test(statics, 3000);
        Assert.That(Math.Abs(diff), Is.LessThanOrEqualTo(0.05),
            $"Absolute relative difference {Math.Abs(diff):P2} exceeded tolerance 5%.");
    }

    [Test]
    public void Lifecycle_CustomizedSeasonality_StartStop_ResumesAndGeneratesExpectedEvents()
    {
        const int nEvents = 100;
        _logger?.LogInformation("Seasonality - Customized_On_and_Off");
        var statics = new PatternGenerator.Statics
        {
            MeanHourlyRate = 0.5,
        };
        statics.CustomizedSeasonalFactors.Add((TimeSpan.FromHours(100), new List<double> { 1, 3, 9 }));
        statics.CustomizedSeasonalFactors.Add((TimeSpan.FromHours(1000), new List<double> { 1, 3, 9 }));
        Test_On_and_Off(statics, nEvents);
    }

    private double Test(PatternGenerator.Statics assets, int nEvents)
    {
        var gen = new PatternGenerator(_logger, assets, id: nameof(PatternGenerator), seed: 0);
        gen.Start();
        for (int i = 0; i < nEvents; i++)
            gen.Run(1);

        var expected = 1 / assets.MeanHourlyRate * gen.Count;
        var observed = gen.ClockTime.TotalHours;
        var diff = expected / observed - 1;
        return diff;
    }

    private void Test_On_and_Off(PatternGenerator.Statics config, int nEvents)
    {
        var gen = new PatternGenerator(_logger, config, id: nameof(PatternGenerator), seed: 0);
        gen.Start();
        Assert.That(gen.IsOn, Is.True, "Generator should be ON after Start().");
        gen.Run(nEvents / 2);
        Assert.That(gen.Count, Is.EqualTo(nEvents / 2), "Generator should produce the requested number of events before pausing.");
        gen.End();
        Assert.That(gen.IsOn, Is.False, "Generator should be OFF after End().");
        Assert.That(gen.ClockTime > TimeSpan.Zero, Is.True, "ClockTime should have advanced after producing events.");
        gen.Run(TimeSpan.FromDays(3));
        gen.Start();
        Assert.That(gen.IsOn, Is.True, "Generator should be ON after resuming Start().");
        gen.Run(nEvents / 2);
        Assert.That(gen.Count, Is.EqualTo(nEvents), "Generator should produce the total expected number of events after resume.");
        gen.End();
    }
}
