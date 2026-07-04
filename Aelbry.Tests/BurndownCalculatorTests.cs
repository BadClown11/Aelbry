using Aelbry.BL;
using Xunit;

namespace Aelbry.Tests
{
    public class BurndownCalculatorTests
    {
        [Fact]
        public void Calculate_IdealLine_DecreasesLinearlyToZero()
        {
            var activities = new List<BurndownCalculator.ActivitySnapshot>
            {
                new() { EstimatedHours = 100m, IsCompleted = false },
            };

            var points = BurndownCalculator.Calculate(activities, new DateTime(2026, 1, 1), new DateTime(2026, 1, 11));

            Assert.Equal(100m, points[0].IdealRemainingHours);
            Assert.Equal(50m, points[5].IdealRemainingHours);
            Assert.Equal(0m, points[^1].IdealRemainingHours);
        }

        [Fact]
        public void Calculate_ActualLine_StaysConstant_WhenNothingIsCompleted()
        {
            var activities = new List<BurndownCalculator.ActivitySnapshot>
            {
                new() { EstimatedHours = 40m, IsCompleted = false },
                new() { EstimatedHours = 60m, IsCompleted = false },
            };

            var points = BurndownCalculator.Calculate(activities, new DateTime(2026, 1, 1), new DateTime(2026, 1, 5));

            Assert.All(points, p => Assert.Equal(100m, p.ActualRemainingHours));
        }

        [Fact]
        public void Calculate_ActualLine_DropsOnTheDayAnActivityIsCompleted()
        {
            var activities = new List<BurndownCalculator.ActivitySnapshot>
            {
                new() { EstimatedHours = 40m, IsCompleted = true, ActualEndDate = new DateTime(2026, 1, 3) },
                new() { EstimatedHours = 60m, IsCompleted = false },
            };

            var points = BurndownCalculator.Calculate(activities, new DateTime(2026, 1, 1), new DateTime(2026, 1, 5));

            Assert.Equal(100m, points[0].ActualRemainingHours); // dia 1: nada completado aun
            Assert.Equal(100m, points[1].ActualRemainingHours); // dia 2: aun no
            Assert.Equal(60m, points[2].ActualRemainingHours);  // dia 3: se completo la de 40h
            Assert.Equal(60m, points[3].ActualRemainingHours);  // se mantiene
        }

        [Fact]
        public void Calculate_ReturnsOnePointPerDayInRange()
        {
            var points = BurndownCalculator.Calculate(new List<BurndownCalculator.ActivitySnapshot>(), new DateTime(2026, 1, 1), new DateTime(2026, 1, 8));

            Assert.Equal(8, points.Count);
        }
    }
}
