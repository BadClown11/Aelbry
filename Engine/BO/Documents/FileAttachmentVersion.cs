namespace Aelbry.BO.Documents
{
    /// <summary>
    /// Version inmutable de una subida de archivo. StoredFileName es el nombre fisico en
    /// disco (GUID + extension); OriginalFileName es el nombre que el usuario ve/descarga.
    /// </summary>
    public class FileAttachmentVersion
    {
        public int FileAttachmentVersionId { get; set; }

        public int FileAttachmentId { get; set; }

        public int VersionNumber { get; set; }

        public string StoredFileName { get; set; }

        public string OriginalFileName { get; set; }

        public string ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        public int UploadedBy { get; set; }

        public DateTime UploadedDate { get; set; }
    }
}
