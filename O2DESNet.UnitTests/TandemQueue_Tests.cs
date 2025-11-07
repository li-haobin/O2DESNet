using Microsoft.Extensions.Logging;

using NUnit.Framework;

using O2DESNet.Demos;

using Serilog;
using Serilog.Events;

using System;
using System.Diagnostics;

namespace O2DESNet.UnitTests;

/// <summary>
/// Long-run, smoke, and regression tests for the demo tandem queue model (O2DESNet.Demos.TandemQueue).
/// 
/// Purpose
/// - Exercises a two-station tandem queue over an extended horizon to ensure the end-to-end event
///   scheduling, queueing, and statistics-accumulation logic operate without runtime errors across
///   multiple random seeds.
/// - Provides quick feedback on performance counters (e.g., average numbers in queue/serving and
///   average time in system) after a warm-up period. While no analytical assertions are made here,
///   the logged KPIs serve as baselines for spotting regressions after refactors or dependency
///   upgrades.
/// 
/// Test design
/// - For each of several seeds, the test constructs a TandemQueue instance with a fixed set of
///   model parameters (e.g., capacities and/or rate parameters as defined by the demo), performs a
///   warm-up of 1,000 simulated hours to let transient effects dissipate, then runs 20,000 hours to
///   collect stable long-run averages.
/// - A Stopwatch captures wall-clock execution time to give a rough indication of simulation
///   throughput on the current environment; this is purely informational.
/// - Key performance indicators are emitted via Serilog: AvgNQueueing1/2, AvgNServing1/2, and
///   AvgHoursInSystem. Consumers can compare these metrics across commits or environments to detect
///   unintentional behavioral shifts.
/// 
/// Logging and reproducibility
/// - Serilog is configured to log to console and a rolling file. Logs are bridged to
///   Microsoft.Extensions.Logging so downstream components share a consistent logging facade.
/// - Deterministic seeds are used to make runs reproducible. Differences in KPIs across runs with
///   the same seed may indicate subtle changes in scheduling semantics or random number generation.
/// 
/// Scope and non-goals
/// - This class is intentionally non-assertive about numerical targets; it is intended as a
///   stability/regression harness and performance smoke test rather than a strict correctness proof
///   against closed-form queueing results. Analytical validation should live in complementary tests
///   that assert specific expectations for known parameterizations.
/// - The test focuses on the demo model’s integration with the simulation engine (event loop,
///   statistics, timing) rather than micro-level unit behavior of individual components.
/// </summary>
public class TandemQueue_Tests
{
    private Microsoft.Extensions.Logging.ILogger? _logger = null;
    private LogEventLevel _minLevel = LogEventLevel.Information;

    [SetUp]
    public void Init()
    {
        // Comment out the following code to switch log file
        //ConfigureSerilog("Logs\\log-TandemQueue_Tests.txt");
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
            .CreateLogger<TandemQueue_Tests>();
    }

    [Test]
    public void TandemQueue_LongRun_MetricsWithinBounds()
    {
        for (int seed = 0; seed < 3; seed++)
        {
            var q = new TandemQueue(_logger, 3, 5, 5, 2, seed);
            var sw = new Stopwatch();
            sw.Start();
            q.WarmUp(TimeSpan.FromHours(1000));
            q.Run(TimeSpan.FromHours(20000));
            sw.Stop();

            // Log for regression visibility
            _logger?.LogInformation("q1:{0:F4}\tq2:{1:F4}\ts1:{2:F4}\ts2:{3:F4}\tT:{4:F2}hrs\t{5}ms",
                q.AvgNQueueing1, q.AvgNQueueing2, q.AvgNServing1, q.AvgNServing2,
                q.AvgHoursInSystem, sw.ElapsedMilliseconds);

            // Deterministic, physics-based invariants that must always hold
            Assert.Multiple(() =>
            {
                // Queue 2 has finite capacity; averages must respect capacity bounds
                Assert.That(q.BufferQueueSize, Is.EqualTo(2));
                Assert.That(q.AvgNQueueing2, Is.GreaterThanOrEqualTo(0));
                Assert.That(q.AvgNQueueing2, Is.LessThanOrEqualTo(q.BufferQueueSize));

                // Queue 1 is unbounded but averages must be non-negative
                Assert.That(q.AvgNQueueing1, Is.GreaterThanOrEqualTo(0));

                // Each server has capacity 1; average in service must be within [0, 1]
                Assert.That(q.AvgNServing1, Is.GreaterThanOrEqualTo(0));
                Assert.That(q.AvgNServing1, Is.LessThanOrEqualTo(1));
                Assert.That(q.AvgNServing2, Is.GreaterThanOrEqualTo(0));
                Assert.That(q.AvgNServing2, Is.LessThanOrEqualTo(1));

                // Time in system must be non-negative
                Assert.That(q.AvgHoursInSystem, Is.GreaterThanOrEqualTo(0));

                // Ensure no NaNs are produced in metrics
                Assert.That(double.IsNaN(q.AvgNQueueing1), Is.False);
                Assert.That(double.IsNaN(q.AvgNQueueing2), Is.False);
                Assert.That(double.IsNaN(q.AvgNServing1), Is.False);
                Assert.That(double.IsNaN(q.AvgNServing2), Is.False);
                Assert.That(double.IsNaN(q.AvgHoursInSystem), Is.False);
            });
        }
    }
}
