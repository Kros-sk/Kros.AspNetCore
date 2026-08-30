using System;
using System.Diagnostics;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    /// <summary>
    /// Creates activities which behave like the ones produced by a real instrumentation library,
    /// so that processors can be tested in isolation.
    /// </summary>
    internal sealed class TestActivity : IDisposable
    {
        private static readonly ActivitySource _source = new(nameof(TestActivity));

        static TestActivity()
        {
            // The listener is registered once for the whole test run. Tests run in parallel, so a listener
            // owned by a single test instance could be disposed while another test is starting an activity,
            // which would make ActivitySource.StartActivity return null.
            ActivitySource.AddActivityListener(new ActivityListener
            {
                ShouldListenTo = source => source == _source,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
            });
        }

        public TestActivity(string displayName, ActivityKind kind = ActivityKind.Server)
        {
            Activity = _source.StartActivity(displayName, kind)
                ?? throw new InvalidOperationException("Activity was not created.");
        }

        public Activity Activity { get; }

        /// <summary>
        /// Returns <see langword="true"/> when the activity was dropped by a processor.
        /// </summary>
        public bool IsDropped => !Activity.Recorded && !Activity.IsAllDataRequested;

        public TestActivity WithTag(string name, string value)
        {
            Activity.SetTag(name, value);
            return this;
        }

        public void Dispose() => Activity.Dispose();
    }
}
