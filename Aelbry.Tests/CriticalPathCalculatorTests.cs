using Aelbry.BL;
using Xunit;

namespace Aelbry.Tests
{
    public class CriticalPathCalculatorTests
    {
        [Fact]
        public void Calculate_SingleActivityWithoutDependencies_HasZeroSlack()
        {
            var nodes = new List<CriticalPathCalculator.ActivityNode>
            {
                new() { ActivityId = 1, Duration = 5m },
            };

            var result = CriticalPathCalculator.Calculate(nodes);

            Assert.Single(result);
            Assert.Equal(0m, result[0].EarlyStart);
            Assert.Equal(5m, result[0].EarlyFinish);
            Assert.Equal(0m, result[0].Slack);
            Assert.True(result[0].IsCritical);
        }

        [Fact]
        public void Calculate_LinearChain_AllActivitiesAreCritical()
        {
            // 1 (3d) -> 2 (2d) -> 3 (4d)
            var nodes = new List<CriticalPathCalculator.ActivityNode>
            {
                new() { ActivityId = 1, Duration = 3m },
                new() { ActivityId = 2, Duration = 2m, PredecessorIds = new List<int> { 1 } },
                new() { ActivityId = 3, Duration = 4m, PredecessorIds = new List<int> { 2 } },
            };

            var result = CriticalPathCalculator.Calculate(nodes).ToDictionary(r => r.ActivityId);

            Assert.Equal(0m, result[1].EarlyStart);
            Assert.Equal(3m, result[2].EarlyStart);
            Assert.Equal(5m, result[3].EarlyStart);
            Assert.Equal(9m, result[3].EarlyFinish);

            Assert.All(result.Values, r => Assert.True(r.IsCritical));
        }

        [Fact]
        public void Calculate_ParallelBranches_ShorterBranchHasPositiveSlack()
        {
            // 1 (5d) -> 3 (2d);  2 (1d) -> 3 (2d).  La rama 1->3 es la critica.
            var nodes = new List<CriticalPathCalculator.ActivityNode>
            {
                new() { ActivityId = 1, Duration = 5m },
                new() { ActivityId = 2, Duration = 1m },
                new() { ActivityId = 3, Duration = 2m, PredecessorIds = new List<int> { 1, 2 } },
            };

            var result = CriticalPathCalculator.Calculate(nodes).ToDictionary(r => r.ActivityId);

            Assert.True(result[1].IsCritical);
            Assert.True(result[3].IsCritical);
            Assert.False(result[2].IsCritical);
            Assert.Equal(4m, result[2].Slack);
        }

        [Fact]
        public void Calculate_CyclicDependency_Throws()
        {
            var nodes = new List<CriticalPathCalculator.ActivityNode>
            {
                new() { ActivityId = 1, Duration = 1m, PredecessorIds = new List<int> { 2 } },
                new() { ActivityId = 2, Duration = 1m, PredecessorIds = new List<int> { 1 } },
            };

            Assert.Throws<InvalidOperationException>(() => CriticalPathCalculator.Calculate(nodes));
        }

        [Fact]
        public void Calculate_EmptyList_ReturnsEmptyResult()
        {
            var result = CriticalPathCalculator.Calculate(new List<CriticalPathCalculator.ActivityNode>());

            Assert.Empty(result);
        }
    }
}
