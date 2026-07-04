using Aelbry.BL;
using Aelbry.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize(Policy = "Permission:TEAMS_MANAGE")]
    public class TeamController : ApiControllerBase
    {
        private readonly TeamBL _teamBL;

        public TeamController(TeamBL teamBL)
        {
            _teamBL = teamBL;
        }

        [HttpGet]
        public JsonResult GetByDepartment(int departmentId)
        {
            return Exec(() => _teamBL.GetByDepartment(departmentId));
        }

        [HttpGet]
        public JsonResult GetById(int teamId)
        {
            return Exec(() => _teamBL.GetById(teamId));
        }

        [HttpPost]
        public JsonResult Create([FromBody] Team team)
        {
            return Exec(() =>
            {
                team.CreatedBy = CurrentUserId;
                return _teamBL.Create(team);
            });
        }

        [HttpPost]
        public JsonResult Update([FromBody] Team team)
        {
            return Exec(() =>
            {
                team.ModifiedBy = CurrentUserId;
                _teamBL.Update(team);
            });
        }

        [HttpPost]
        public JsonResult Delete(int teamId)
        {
            return Exec(() => _teamBL.Delete(teamId, CurrentUserId));
        }
    }
}
