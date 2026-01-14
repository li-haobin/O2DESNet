using Microsoft.Extensions.Logging;

using NUnit.Framework;

using O2DESNet.Standard;

using Serilog;
using Serilog.Events;

using System;

namespace O2DESNet.UnitTests;

/// <summary>
/// Minimal, infrastructure-focused unit tests for the core simulation engine to ensure that
/// advancing simulation time behaves correctly and that logging is set up consistently for
/// diagnostics.
/// 
/// Purpose
/// - Validates that calling Run with a TimeSpan advances the simulation clock by the exact
///   duration requested when no intervening events preempt that horizon. This is the most
///   fundamental contract of the Simulator: time progression is monotonic and precise.
/// - Uses a simple Standard.Server instance as a lightweight concrete simulator host so the test
///   exercises the shared scheduling and clock-keeping mechanics rather than domain logic.
/// 
/// What is covered
/// - ClockTime_Advance: Constructs a Server with deterministic seed and capacity 1, advances the
///   simulation by two hours using Run(TimeSpan.FromHours(2)), and asserts that ClockTime equals the
///   same two-hour span. This acts as a smoke test that the base engine increments time correctly
///   when asked to advance by a fixed interval.
/// 
/// Test scaffolding and logging
/// - SetUp configures Serilog to write to console and a rolling file, then bridges it into
///   Microsoft.Extensions.Logging so diagnostics remain consistent across components. While the
///   assertion is simple, logs are useful when investigating integration or environment issues.
/// - A fixed seed is supplied to the simulator to keep behaviors reproducible across runs, even
///   though this specific test does not rely on randomness.
/// 
/// Scope and non-goals
/// - This class intentionally avoids validating operational semantics of servers, queues, or event
///   processing policies. Those concerns are covered by dedicated tests elsewhere (for example,
///   queueing or pattern generation tests). Here, the focus is the simulator infrastructure: time
///   advancement and basic lifecycle via disposal.
/// - Future extensions may include verifying pause/resume semantics, advancing with pending events,
///   and ensuring that advancing by zero or negative durations is handled as specified by the
///   engine’s contract.
/// </summary>
[TestFixture]
public class Simulator_Tests
{
    private Microsoft.Extensions.Logging.ILogger? _logger;
    private LogEventLevel _minLevel = LogEventLevel.Information;

    [SetUp]
    public void Init()
    {
        // Comment out the following code to switch log file
        //ConfigureSerilog("Logs\\log-Simulator_Tests.txt");
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
            .CreateLogger<Simulator_Tests>();
    }

    [Test]
    public void Run_WithTimeSpan_AdvancesClockTimeByDuration()
    {
        var statics = new Server.Statics { Capacity = 1 };
        using var sim = new Server(_logger, statics, id: nameof(Server), seed: 0);

        // Assert initial clock time is zero
        Assert.That(sim.ClockTime, Is.EqualTo(TimeSpan.Zero));

        var advanceBy = TimeSpan.FromHours(2);
        sim.Run(advanceBy);

        // Assert clock time advanced exactly by the requested duration
        Assert.That(sim.ClockTime, Is.EqualTo(advanceBy));
    }
}
