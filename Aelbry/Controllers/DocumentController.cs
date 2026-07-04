using Aelbry.BL.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class DocumentController : ApiControllerBase
    {
        private readonly DocumentBL _documentBL;

        public DocumentController(DocumentBL documentBL)
        {
            _documentBL = documentBL;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:DOCUMENTS_VIEW")]
        public JsonResult GetByProject(int projectId)
        {
            return Exec(() => _documentBL.GetByProject(projectId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:DOCUMENTS_VIEW")]
        public JsonResult GetById(int documentId)
        {
            return Exec(() => _documentBL.GetById(documentId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:DOCUMENTS_VIEW")]
        public JsonResult GetLatestVersion(int documentId)
        {
            return Exec(() => _documentBL.GetLatestVersion(documentId));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:DOCUMENTS_VIEW")]
        public JsonResult GetVersions(int documentId)
        {
            return Exec(() => _documentBL.GetVersions(documentId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:DOCUMENTS_MANAGE")]
        public JsonResult Create(int projectId, string title, string contentMarkdown)
        {
            return Exec(() => _documentBL.Create(projectId, title, contentMarkdown, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:DOCUMENTS_MANAGE")]
        public JsonResult UpdateTitle(int documentId, string title)
        {
            return Exec(() => _documentBL.UpdateTitle(documentId, title, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:DOCUMENTS_MANAGE")]
        public JsonResult SaveNewVersion(int documentId, string contentMarkdown)
        {
            return Exec(() => _documentBL.SaveNewVersion(documentId, contentMarkdown, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:DOCUMENTS_MANAGE")]
        public JsonResult Delete(int documentId)
        {
            return Exec(() => _documentBL.Delete(documentId, CurrentUserId));
        }
    }
}
