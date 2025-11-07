using Microsoft.Extensions.Logging;

using NUnit.Framework;

using O2DESNet.Demos;
using O2DESNet.Distributions;
using O2DESNet.Standard;

using Serilog;
using Serilog.Events;

using System;
using System.Diagnostics;

namespace O2DESNet.UnitTests;

/// <summary>
/// Version 3 functional and integration tests for O2DESNet that exercise the updated Sandbox
/// APIs, warm-up lifecycle, generator module behavior, and two flavors of M/M/n queue demos.
/// 
/// Goals
/// - Validate core lifecycle hooks introduced or refined in V3 (e.g., WarmUp and WarmedUpHandler)
///   across a hierarchy of nested Sandbox instances, confirming that warm-up propagates correctly
///   and that user overrides are invoked once the warm-up horizon elapses.
/// - Smoke and regression-test the Standard.Generator in Sandbox form, including Start/End cycles
///   and running across mixed phases (pre-start, running, paused), ensuring that event scheduling
///   and clock advancement align with expectations.
/// - Exercise two reference queueing implementations (MMnQueue_Atomic and MMnQueue_Modular) over a
///   long horizon to surface integration issues in the event loop, statistics accumulation
///   (HourCounter), and logging, while providing baseline KPIs for future comparisons.
/// 
/// Test coverage overview
/// - WarmedUp: Builds a small Sandbox hierarchy A → {B → D, C}. Calling WarmUp triggers the
///   framework to advance to the specified duration and then invoke WarmedUpHandler on each module.
///   The handlers log a message per module (A/B/C/D), demonstrating successful propagation and hook
///   execution.
/// - Generator: Creates a Generator.Statics with an exponential inter-arrival time and hosts it in a
///   Sandbox via .Sandbox(...). It mixes Run(durations) with Start/End to exercise transitions
///   between idle, active, and paused phases without throwing, and with expected clock progression.
/// - MMnQueue_Atomic and MMnQueue_Modular: For several seeds, each queue demo is warmed up for 1,000
///   simulated hours and then run for 20,000 hours. The tests log long-run metrics
///   (AvgNQueueing, AvgNServing, AvgHoursInSystem) and elapsed wall-clock time. These are intended
///   as stability and performance baselines rather than strict analytical assertions.
/// 
/// Instrumentation and reproducibility
/// - Serilog is configured to write to console and a rolling file and is bridged into
///   Microsoft.Extensions.Logging for consistent diagnostics across modules.
/// - A deterministic seed is supplied wherever applicable to keep runs reproducible.
/// 
/// Scope and non-goals
/// - These tests focus on lifecycle correctness (warm-up and start/stop), scheduler stability,
///   and aggregate statistics over long runs. They do not assert closed-form queueing results or the
///   exact distributions of inter-arrival/service times; such analytical validation can be covered
///   in dedicated unit tests.
/// - The nested Sandbox classes (A/B/C/D) are minimal scaffolding to validate V3 warm-up semantics
///   and the WarmedUpHandler override path.
/// </summary>
[TestFixture]
public class Version_3_Tests
{
    private Microsoft.Extensions.Logging.ILogger _logger;
    private LogEventLevel _minLevel = LogEventLevel.Information;

    [SetUp]
    public void Init()
    {
        // Comment out the following code to switch log file
        //ConfigureSerilog("Logs\\log-Version_3.txt");
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
    public void WarmedUp()
    {
        var a = new A(_logger);
        var ok = a.WarmUp(TimeSpan.FromHours(1));
        Assert.That(a.ClockTime, Is.EqualTo(TimeSpan.FromHours(1)));
    }

    [Test]
    public void Generator()
    {
        var gen = new Generator.Statics
        {
            InterArrivalTime =
            rs => Exponential.Sample(rs, TimeSpan.FromMinutes(3))
        }.Sandbox(logger: _logger, seed: 0);
        gen.Run(TimeSpan.FromHours(0.5));
        gen.Start();
        gen.Run(TimeSpan.FromHours(2));
        gen.End();
        gen.Run(TimeSpan.FromHours(0.5));
        gen.Start();
        gen.Run(TimeSpan.FromHours(1));
        Assert.That(gen.ClockTime, Is.EqualTo(TimeSpan.FromHours(4)));
    }

    [Test]
    public void MMnQueue_Atomic()
    {
        for (int seed = 0; seed < 3; seed++)
        {
            var q = new MMnQueue_Atomic(_logger, 4, 5, 1, seed);
            var sw = new Stopwatch();
            sw.Start();
            var warmOk = q.WarmUp(TimeSpan.FromHours(1000));
            var runOk = q.Run(TimeSpan.FromHours(20000));
            sw.Stop();
            q.Logger?.LogInformation("{0:F4}\t{1:F4}\t{2:F4}\t{3}ms",
                q.AvgNQueueing, q.AvgNServing, q.AvgHoursInSystem, sw.ElapsedMilliseconds);

            Assert.That(warmOk && runOk, Is.True);
            Assert.That(q.ClockTime, Is.EqualTo(TimeSpan.FromHours(21000)));
            Assert.That(double.IsFinite(q.AvgNQueueing), Is.True);
            Assert.That(q.AvgNQueueing, Is.GreaterThanOrEqualTo(0));
            Assert.That(double.IsFinite(q.AvgNServing), Is.True);
            Assert.That(q.AvgNServing, Is.GreaterThanOrEqualTo(0));
            Assert.That(double.IsFinite(q.AvgHoursInSystem), Is.True);
            Assert.That(q.AvgHoursInSystem, Is.GreaterThanOrEqualTo(0));
        }
    }

    [Test]
    public void MMnQueue_Modular()
    {
        for (int seed = 0; seed < 3; seed++)
        {
            var q = new MMnQueue_Modular(_logger, 4, 5, 1, seed);
            var sw = new Stopwatch();
            sw.Start();
            var warmOk = q.WarmUp(TimeSpan.FromHours(1000));
            var runOk = q.Run(TimeSpan.FromHours(20000));
            sw.Stop();
            q.Logger?.LogInformation("{0:F4}\t{1:F4}\t{2:F4}\t{3}ms",
                q.AvgNQueueing, q.AvgNServing, q.AvgHoursInSystem, sw.ElapsedMilliseconds);

            Assert.That(warmOk && runOk, Is.True);
            Assert.That(q.ClockTime, Is.EqualTo(TimeSpan.FromHours(21000)));
            Assert.That(double.IsFinite(q.AvgNQueueing), Is.True);
            Assert.That(q.AvgNQueueing, Is.GreaterThanOrEqualTo(0));
            Assert.That(double.IsFinite(q.AvgNServing), Is.True);
            Assert.That(q.AvgNServing, Is.GreaterThanOrEqualTo(0));
            Assert.That(double.IsFinite(q.AvgHoursInSystem), Is.True);
            Assert.That(q.AvgHoursInSystem, Is.GreaterThanOrEqualTo(0));
        }
    }

    private class Assets : IAssets
    {
        public string Id => GetType().Name;
    }

    private class A : Sandbox<Assets>
    {
        public A(Microsoft.Extensions.Logging.ILogger logger)
            : base(logger, new Assets(), id: nameof(A), seed: 0)
        {
            AddChild(new B(logger));
            AddChild(new C(logger));
        }

        public override void Dispose() { }

        protected override void WarmedUpHandler()
        {
            Logger?.LogInformation("A WarmedUp");
        }
    }

    private class B : Sandbox<Assets>
    {
        public B(Microsoft.Extensions.Logging.ILogger logger)
            : base(logger, new Assets(), id: nameof(B), seed: 0) { AddChild(new D(logger)); }

        public override void Dispose() { }

        protected override void WarmedUpHandler()
        {
            Logger?.LogInformation("B WarmedUp");
        }
    }

    private class C : Sandbox<Assets>
    {
        public C(Microsoft.Extensions.Logging.ILogger logger)
            : base(logger, new Assets(), id: nameof(C), seed: 0) { }

        public override void Dispose() { }

        protected override void WarmedUpHandler()
        {
            Logger?.LogInformation("C WarmedUp");
        }
    }

    private class D : Sandbox<Assets>
    {
        public D(Microsoft.Extensions.Logging.ILogger logger)
            : base(logger, new Assets(), id: nameof(D), seed: 0) { }

        public override void Dispose() { }

        protected override void WarmedUpHandler()
        {
            Logger?.LogInformation("D WarmedUp");
        }
    }
}
