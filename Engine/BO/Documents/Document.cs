using Aelbry.BO.Common;

namespace Aelbry.BO.Documents
{
    /// <summary>
    /// Documento tipo Notion. El contenido "actual" vive en la ultima DocumentVersion
    /// (MAX(VersionNumber)); esta entidad solo guarda los metadatos.
    /// </summary>
    public class Document : AuditableEntity
    {
        public int DocumentId { get; set; }

        public int ProjectId { get; set; }

        public string Title { get; set; }

        public int LatestVersionNumber { get; set; }
    }
}
