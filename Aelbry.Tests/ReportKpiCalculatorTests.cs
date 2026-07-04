using Aelbry.BL;
using Xunit;

namespace Aelbry.Tests
{
    public class ReportKpiCalculatorTests
    {
        [Fact]
        public void CalculateProductivityPercent_ReturnsZero_WhenNothingWasDue()
        {
            Assert.Equal(0m, ReportKpiCalculator.CalculateProductivityPercent(completed: 5, due: 0));
        }

        [Fact]
        public void CalculateProductivityPercent_Returns100_WhenCompletedEqualsDue()
        {
            Assert.Equal(100m, ReportKpiCalculator.CalculateProductivityPercent(completed: 10, due: 10));
        }

        [Fact]
        public void CalculateProductivityPercent_ReturnsPartialPercentage()
        {
            Assert.Equal(50m, ReportKpiCalculator.CalculateProductivityPercent(completed: 5, due: 10));
        }

        [Fact]
        public void CalculateProductivityPercent_CanExceed100_WhenMoreCompletedThanScheduled()
        {
            // Se completo trabajo que estaba programado para otra semana; el KPI no se acota a 100.
            Assert.Equal(150m, ReportKpiCalculator.CalculateProductivityPercent(completed: 15, due: 10));
        }
    }
}
