namespace Aelbry.BO.Documents
{
    /// <summary>
    /// Version inmutable del contenido de un documento. Cada guardado crea una fila nueva.
    /// </summary>
    public class DocumentVersion
    {
        public int DocumentVersionId { get; set; }

        public int DocumentId { get; set; }

        public int VersionNumber { get; set; }

        public string ContentMarkdown { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
