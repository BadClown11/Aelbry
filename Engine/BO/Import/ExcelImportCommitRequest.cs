namespace Aelbry.BO.Import
{
    public class ExcelImportCommitRequest
    {
        public string Token { get; set; }

        public int ProjectId { get; set; }

        public int? ParentActivityId { get; set; }

        public ExcelColumnMapping Mapping { get; set; }
    }
}
