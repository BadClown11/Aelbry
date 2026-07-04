using Aelbry.BO.Documents;
using Aelbry.DAL;

namespace Aelbry.BL.Documents
{
    /// <summary>
    /// Archivos adjuntos con versionado: cada re-subida del mismo FileAttachment crea una
    /// FileAttachmentVersion nueva y guarda un binario nuevo en disco (el anterior se conserva).
    /// </summary>
    public class FileBL
    {
        private readonly FileStorageService _storage;

        public FileBL(FileStorageService storage)
        {
            _storage = storage;
        }

        public List<FileFolder> GetFolders(int projectId)
        {
            using (var dal = FileDAL.Instance)
            {
                return dal.GetFoldersByProject(projectId);
            }
        }

        public FileFolder CreateFolder(int projectId, int? parentFolderId, string name, int createdBy)
        {
            using (var dal = FileDAL.Instance)
            {
                int id = dal.CreateFolder(projectId, parentFolderId, name, createdBy);
                return dal.GetFoldersByProject(projectId).Find(f => f.FileFolderId == id);
            }
        }

        public void DeleteFolder(int fileFolderId)
        {
            using (var dal = FileDAL.Instance)
            {
                dal.DeleteFolder(fileFolderId);
            }
        }

        public List<FileAttachment> GetAttachments(int projectId, int? fileFolderId)
        {
            using (var dal = FileDAL.Instance)
            {
                return dal.GetAttachmentsByProject(projectId, fileFolderId);
            }
        }

        public async Task<FileAttachment> UploadNewFileAsync(int projectId, int? fileFolderId, string originalFileName, string contentType, long fileSizeBytes, Stream content, int uploadedBy)
        {
            using (var dal = FileDAL.Instance)
            {
                int attachmentId = dal.CreateAttachment(projectId, fileFolderId, originalFileName, uploadedBy);

                string storedFileName = _storage.GenerateStoredFileName(originalFileName);
                await _storage.SaveAsync(projectId, storedFileName, content);

                dal.CreateVersion(attachmentId, storedFileName, originalFileName, contentType, fileSizeBytes, uploadedBy);

                return dal.GetAttachmentById(attachmentId);
            }
        }

        public async Task<FileAttachmentVersion> UploadNewVersionAsync(int fileAttachmentId, string originalFileName, string contentType, long fileSizeBytes, Stream content, int uploadedBy)
        {
            using (var dal = FileDAL.Instance)
            {
                var attachment = dal.GetAttachmentById(fileAttachmentId);

                string storedFileName = _storage.GenerateStoredFileName(originalFileName);
                await _storage.SaveAsync(attachment.ProjectId, storedFileName, content);

                dal.CreateVersion(fileAttachmentId, storedFileName, originalFileName, contentType, fileSizeBytes, uploadedBy);

                return dal.GetLatestVersion(fileAttachmentId);
            }
        }

        public List<FileAttachmentVersion> GetVersions(int fileAttachmentId)
        {
            using (var dal = FileDAL.Instance)
            {
                return dal.GetVersionsByAttachment(fileAttachmentId);
            }
        }

        public (Stream Content, FileAttachmentVersion Version) DownloadVersion(int fileAttachmentVersionId)
        {
            using (var dal = FileDAL.Instance)
            {
                var version = dal.GetVersionById(fileAttachmentVersionId);
                var attachment = dal.GetAttachmentById(version.FileAttachmentId);

                var stream = _storage.OpenRead(attachment.ProjectId, version.StoredFileName);
                return (stream, version);
            }
        }

        public FileAttachmentVersion DownloadLatest(int fileAttachmentId, out Stream content)
        {
            using (var dal = FileDAL.Instance)
            {
                var attachment = dal.GetAttachmentById(fileAttachmentId);
                var version = dal.GetLatestVersion(fileAttachmentId);

                content = _storage.OpenRead(attachment.ProjectId, version.StoredFileName);
                return version;
            }
        }

        public void DeleteAttachment(int fileAttachmentId)
        {
            using (var dal = FileDAL.Instance)
            {
                dal.DeleteAttachment(fileAttachmentId);
            }
        }
    }
}
