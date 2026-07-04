using Aelbry.BL;
using Xunit;

namespace Aelbry.Tests
{
    public class AutomationTriggerEvaluatorTests
    {
        [Fact]
        public void CrossesThresholdUpward_ReturnsTrue_WhenValueCrossesFromBelowToAtOrAboveThreshold()
        {
            Assert.True(AutomationTriggerEvaluator.CrossesThresholdUpward(previousValue: 90m, newValue: 100m, threshold: 100m));
        }

        [Fact]
        public void CrossesThresholdUpward_ReturnsFalse_WhenAlreadyAtOrAboveThresholdBefore()
        {
            Assert.False(AutomationTriggerEvaluator.CrossesThresholdUpward(previousValue: 100m, newValue: 100m, threshold: 100m));
        }

        [Fact]
        public void CrossesThresholdUpward_ReturnsFalse_WhenValueDecreases()
        {
            Assert.False(AutomationTriggerEvaluator.CrossesThresholdUpward(previousValue: 100m, newValue: 50m, threshold: 100m));
        }

        [Fact]
        public void CrossesThresholdUpward_ReturnsFalse_WhenStillBelowThreshold()
        {
            Assert.False(AutomationTriggerEvaluator.CrossesThresholdUpward(previousValue: 40m, newValue: 60m, threshold: 100m));
        }

        [Fact]
        public void CrossesThresholdUpward_ReturnsTrue_WhenCrossingAnIntermediateThreshold()
        {
            Assert.True(AutomationTriggerEvaluator.CrossesThresholdUpward(previousValue: 45m, newValue: 55m, threshold: 50m));
        }
    }
}
