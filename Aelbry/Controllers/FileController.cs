using Aelbry.BL.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class FileController : ApiControllerBase
    {
        private readonly FileBL _fileBL;

        public FileController(FileBL fileBL)
        {
            _fileBL = fileBL;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:FILES_VIEW")]
        public JsonResult GetFolders(int projectId)
        {
            return Exec(() => _fileBL.GetFolders(projectId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:FILES_MANAGE")]
        public JsonResult CreateFolder(int projectId, int? parentFolderId, string name)
        {
            return Exec(() => _fileBL.CreateFolder(projectId, parentFolderId, name, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:FILES_MANAGE")]
        public JsonResult DeleteFolder(int fileFolderId)
        {
            return Exec(() => _fileBL.DeleteFolder(fileFolderId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:FILES_VIEW")]
        public JsonResult GetAttachments(int projectId, int? fileFolderId)
        {
            return Exec(() => _fileBL.GetAttachments(projectId, fileFolderId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:FILES_VIEW")]
        public JsonResult GetVersions(int fileAttachmentId)
        {
            return Exec(() => _fileBL.GetVersions(fileAttachmentId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:FILES_MANAGE")]
        public async Task<JsonResult> Upload(int projectId, int? fileFolderId, IFormFile file)
        {
            return await ExecAsync(async () =>
            {
                using var stream = file.OpenReadStream();
                return await _fileBL.UploadNewFileAsync(projectId, fileFolderId, file.FileName, file.ContentType, file.Length, stream, CurrentUserId);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:FILES_MANAGE")]
        public async Task<JsonResult> UploadNewVersion(int fileAttachmentId, IFormFile file)
        {
            return await ExecAsync(async () =>
            {
                using var stream = file.OpenReadStream();
                return await _fileBL.UploadNewVersionAsync(fileAttachmentId, file.FileName, file.ContentType, file.Length, stream, CurrentUserId);
            });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:FILES_MANAGE")]
        public JsonResult DeleteAttachment(int fileAttachmentId)
        {
            return Exec(() => _fileBL.DeleteAttachment(fileAttachmentId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:FILES_VIEW")]
        public IActionResult Download(int fileAttachmentVersionId)
        {
            var (content, version) = _fileBL.DownloadVersion(fileAttachmentVersionId);
            string contentType = string.IsNullOrWhiteSpace(version.ContentType) ? "application/octet-stream" : version.ContentType;
            return File(content, contentType, version.OriginalFileName);
        }

        [HttpGet]
        [Authorize(Policy = "Permission:FILES_VIEW")]
        public IActionResult DownloadLatest(int fileAttachmentId)
        {
            var version = _fileBL.DownloadLatest(fileAttachmentId, out var content);
            string contentType = string.IsNullOrWhiteSpace(version.ContentType) ? "application/octet-stream" : version.ContentType;
            return File(content, contentType, version.OriginalFileName);
        }
    }
}
