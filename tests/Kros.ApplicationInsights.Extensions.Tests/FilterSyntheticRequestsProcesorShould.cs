using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterSyntheticRequestsProcesorShould
    {
        [Fact]
        public void PassRequestIfItIsNotSynthetic()
        {
            using TestActivity activity = new("SomeRequest");

            new FilterSyntheticRequestsProcessor().OnEnd(activity.Activity);

            Assert.False(activity.IsDropped);
        }

        [Fact]
        public void FilterOutRequestIfItIsSynthetic()
        {
            using TestActivity activity = new("SomeRequest");
            activity.WithTag(ActivityTags.SyntheticSource, "source");

            new FilterSyntheticRequestsProcessor().OnEnd(activity.Activity);

            Assert.True(activity.IsDropped);
        }
    }
}
