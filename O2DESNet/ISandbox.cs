using System;
using System.Collections.Generic;

namespace O2DESNet
{
    public interface ISandbox : IDisposable
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the pointer.
        /// </summary>
        Pointer Pointer { get; }

        /// <summary>
        /// Gets the seed.
        /// </summary>
        int Seed { get; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        ISandbox Parent { get; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        IReadOnlyList<ISandbox> Children { get; }

        /// <summary>
        /// Gets the clock time.
        /// </summary>
        DateTime ClockTime { get; }

        /// <summary>
        /// Gets the head event time.
        /// </summary>
        DateTime? HeadEventTime { get; }

        /// <summary>
        /// Gets or sets the log file.
        /// </summary>
        string LogFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [debug mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [debug mode]; otherwise, <c>false</c>.
        /// </value>
        bool DebugMode { get; set; }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        bool Run();

        /// <summary>
        /// Runs the specified event count.
        /// </summary>
        /// <param name="eventCount">The event count.</param>
        bool Run(int eventCount);

        /// <summary>
        /// Runs the specified terminate.
        /// </summary>
        /// <param name="terminate">The terminate.</param>
        bool Run(DateTime terminate);

        /// <summary>
        /// Runs the specified duration.
        /// </summary>
        /// <param name="duration">The duration.</param>
        bool Run(TimeSpan duration);

        /// <summary>
        /// Runs the specified speed.
        /// </summary>
        /// <param name="speed">The speed.</param>
        bool Run(double speed);

        /// <summary>
        /// Warms up.
        /// </summary>
        /// <param name="till">The till.</param>
        bool WarmUp(DateTime till);

        /// <summary>
        /// Warms up.
        /// </summary>
        /// <param name="period">The period.</param>
        bool WarmUp(TimeSpan period);   
        
    }
}