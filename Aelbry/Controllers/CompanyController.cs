using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class CompanyController : ApiControllerBase
    {
        private readonly CompanyBL _companyBL;

        public CompanyController(CompanyBL companyBL)
        {
            _companyBL = companyBL;
        }

        // La vista no lleva datos; el JWT via header solo puede validarse en las llamadas
        // fetch de la SPA embebida (GetAll, Create, etc.), no en la navegacion de pagina completa.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:COMPANY_MANAGE")]
        public JsonResult GetAll()
        {
            return Exec(() => _companyBL.GetAll());
        }

        [HttpGet]
        [Authorize(Policy = "Permission:COMPANY_MANAGE")]
        public JsonResult GetById(int companyId)
        {
            return Exec(() => _companyBL.GetById(companyId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:COMPANY_MANAGE")]
        public JsonResult Create([FromBody] Company company)
        {
            var result = Exec(() =>
            {
                company.CreatedBy = CurrentUserId;
                return _companyBL.Create(company);
            });

            if (WasSuccessful(result))
            {
                Audit("COMPANY", "CREATE", company.CompanyId, dataBefore: null, dataAfter: company);
            }

            return result;
        }

        [HttpPost]
        [Authorize(Policy = "Permission:COMPANY_MANAGE")]
        public JsonResult Update([FromBody] Company company)
        {
            var before = _companyBL.GetById(company.CompanyId);

            var result = Exec(() =>
            {
                company.ModifiedBy = CurrentUserId;
                _companyBL.Update(company);
            });

            if (WasSuccessful(result))
            {
                Audit("COMPANY", "UPDATE", company.CompanyId, before, company);
            }

            return result;
        }

        [HttpPost]
        [Authorize(Policy = "Permission:COMPANY_MANAGE")]
        public JsonResult Delete(int companyId)
        {
            var before = _companyBL.GetById(companyId);

            var result = Exec(() => _companyBL.Delete(companyId, CurrentUserId));

            if (WasSuccessful(result))
            {
                Audit("COMPANY", "DELETE", companyId, before, dataAfter: null);
            }

            return result;
        }
    }
}
