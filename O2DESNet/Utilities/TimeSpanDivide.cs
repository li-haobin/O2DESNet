using System;

namespace Utilities
{
    public static partial class TimeSpanExtension
    {
        /// <summary>
        /// Divides a timespan by an integer value
        /// </summary>
        public static TimeSpan Divide(this TimeSpan dividend, int divisor)
        {
            return TimeSpan.FromTicks(dividend.Ticks / divisor);
        }

        /// <summary>
        /// Divides a timespan by a double value
        /// </summary>
        public static TimeSpan Divide(this TimeSpan dividend, double divisor)
        {
            return TimeSpan.FromTicks((long)(dividend.Ticks / divisor));
        }
    }
}
