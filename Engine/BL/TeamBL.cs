using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class TeamBL
    {
        public List<Team> GetByDepartment(int departmentId)
        {
            using (var dal = TeamDAL.Instance)
            {
                return dal.GetByDepartment(departmentId);
            }
        }

        public Team GetById(int teamId)
        {
            using (var dal = TeamDAL.Instance)
            {
                return dal.GetById(teamId);
            }
        }

        public int Create(Team team)
        {
            using (var dal = TeamDAL.Instance)
            {
                return dal.Create(team);
            }
        }

        public void Update(Team team)
        {
            using (var dal = TeamDAL.Instance)
            {
                dal.Update(team);
            }
        }

        public void Delete(int teamId, int modifiedBy)
        {
            using (var dal = TeamDAL.Instance)
            {
                dal.Delete(teamId, modifiedBy);
            }
        }
    }
}
