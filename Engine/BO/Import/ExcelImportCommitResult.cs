namespace Aelbry.BO.Import
{
    public class ExcelImportCommitResult
    {
        public int CreatedCount { get; set; }

        public List<string> Warnings { get; set; } = new List<string>();
    }
}
