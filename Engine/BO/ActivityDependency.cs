namespace Aelbry.BO
{
    public class ActivityDependency
    {
        public int ActivityDependencyId { get; set; }

        public int ActivityId { get; set; }

        public int DependsOnActivityId { get; set; }

        public string DependsOnActivityName { get; set; }

        public DependencyType DependencyType { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
