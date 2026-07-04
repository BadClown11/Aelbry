using Aelbry.BO.Documents;
using Aelbry.DAL;

namespace Aelbry.BL.Documents
{
    /// <summary>
    /// Documentos estilo Notion: cada guardado crea una DocumentVersion inmutable nueva;
    /// nunca se sobreescribe el contenido de una version existente.
    /// </summary>
    public class DocumentBL
    {
        public List<Document> GetByProject(int projectId)
        {
            using (var dal = DocumentDAL.Instance)
            {
                return dal.GetByProject(projectId);
            }
        }

        public Document GetById(int documentId)
        {
            using (var dal = DocumentDAL.Instance)
            {
                return dal.GetById(documentId);
            }
        }

        public List<DocumentVersion> GetVersions(int documentId)
        {
            using (var dal = DocumentDAL.Instance)
            {
                return dal.GetVersionsByDocument(documentId);
            }
        }

        public DocumentVersion GetLatestVersion(int documentId)
        {
            using (var dal = DocumentDAL.Instance)
            {
                return dal.GetLatestVersion(documentId);
            }
        }

        public Document Create(int projectId, string title, string contentMarkdown, int createdBy)
        {
            using (var dal = DocumentDAL.Instance)
            {
                int id = dal.Create(projectId, title, contentMarkdown, createdBy);
                return dal.GetById(id);
            }
        }

        public void UpdateTitle(int documentId, string title, int modifiedBy)
        {
            using (var dal = DocumentDAL.Instance)
            {
                dal.UpdateTitle(documentId, title, modifiedBy);
            }
        }

        public DocumentVersion SaveNewVersion(int documentId, string contentMarkdown, int createdBy)
        {
            using (var dal = DocumentDAL.Instance)
            {
                dal.CreateVersion(documentId, contentMarkdown, createdBy);
                return dal.GetLatestVersion(documentId);
            }
        }

        public void Delete(int documentId, int modifiedBy)
        {
            using (var dal = DocumentDAL.Instance)
            {
                dal.Delete(documentId, modifiedBy);
            }
        }
    }
}
