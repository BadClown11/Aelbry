using Aelbry.BL;
using Xunit;

namespace Aelbry.Tests
{
    public class ActivityProgressCalculatorTests
    {
        [Fact]
        public void CalculateLeafProgress_UsesChecklistRatio_WhenChecklistExists()
        {
            decimal progress = ActivityProgressCalculator.CalculateLeafProgress(3, 4, workedHours: 0, estimatedHours: 10);

            Assert.Equal(75m, progress);
        }

        [Fact]
        public void CalculateLeafProgress_FallsBackToHoursRatio_WhenNoChecklist()
        {
            decimal progress = ActivityProgressCalculator.CalculateLeafProgress(0, 0, workedHours: 5, estimatedHours: 10);

            Assert.Equal(50m, progress);
        }

        [Fact]
        public void CalculateLeafProgress_CapsAt100Percent_WhenWorkedExceedsEstimated()
        {
            decimal progress = ActivityProgressCalculator.CalculateLeafProgress(0, 0, workedHours: 15, estimatedHours: 10);

            Assert.Equal(100m, progress);
        }

        [Fact]
        public void CalculateLeafProgress_ReturnsZero_WhenNoChecklistAndNoEstimatedHours()
        {
            decimal progress = ActivityProgressCalculator.CalculateLeafProgress(0, 0, workedHours: 0, estimatedHours: 0);

            Assert.Equal(0m, progress);
        }

        [Fact]
        public void CalculateWeightedProgress_AveragesChildrenByWeight()
        {
            var children = new (decimal Weight, decimal Progress)[]
            {
                (Weight: 1m, Progress: 100m),
                (Weight: 3m, Progress: 0m),
            };

            decimal progress = ActivityProgressCalculator.CalculateWeightedProgress(children);

            Assert.Equal(25m, progress);
        }

        [Fact]
        public void CalculateWeightedProgress_ReturnsZero_WhenTotalWeightIsZero()
        {
            var children = new (decimal Weight, decimal Progress)[]
            {
                (Weight: 0m, Progress: 100m),
                (Weight: 0m, Progress: 50m),
            };

            decimal progress = ActivityProgressCalculator.CalculateWeightedProgress(children);

            Assert.Equal(0m, progress);
        }

        [Fact]
        public void CalculateWeightedProgress_ReturnsZero_WhenNoChildren()
        {
            decimal progress = ActivityProgressCalculator.CalculateWeightedProgress(Array.Empty<(decimal, decimal)>());

            Assert.Equal(0m, progress);
        }
    }
}
