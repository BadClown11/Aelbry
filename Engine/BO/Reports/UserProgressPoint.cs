namespace Aelbry.BO.Reports
{
    public class UserProgressPoint
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public int ActivityCount { get; set; }

        public decimal AverageProgress { get; set; }
    }
}
