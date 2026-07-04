namespace Aelbry.BO.Documents
{
    public class FileFolder
    {
        public int FileFolderId { get; set; }

        public int ProjectId { get; set; }

        public int? ParentFolderId { get; set; }

        public string Name { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
