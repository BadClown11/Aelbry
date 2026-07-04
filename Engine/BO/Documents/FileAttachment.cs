namespace Aelbry.BO.Documents
{
    /// <summary>
    /// Archivo "logico" (nombre, carpeta). El binario y los metadatos de cada subida
    /// viven en FileAttachmentVersion; LatestVersionNumber/LatestFileSizeBytes son
    /// proyecciones de conveniencia calculadas por el SP de listado.
    /// </summary>
    public class FileAttachment
    {
        public int FileAttachmentId { get; set; }

        public int ProjectId { get; set; }

        public int? FileFolderId { get; set; }

        public string FileName { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int LatestVersionNumber { get; set; }

        public long LatestFileSizeBytes { get; set; }
    }
}
