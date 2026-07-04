namespace Aelbry.BO.Reports
{
    public class BurndownPoint
    {
        public DateTime Date { get; set; }

        public decimal IdealRemainingHours { get; set; }

        public decimal ActualRemainingHours { get; set; }
    }
}
