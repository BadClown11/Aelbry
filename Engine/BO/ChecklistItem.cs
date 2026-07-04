namespace Aelbry.BO
{
    public class ChecklistItem
    {
        public int ChecklistItemId { get; set; }

        public int ActivityId { get; set; }

        public string Text { get; set; }

        public bool IsChecked { get; set; }

        public int Sequence { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? CompletedBy { get; set; }

        public DateTime? CompletedDate { get; set; }
    }
}
