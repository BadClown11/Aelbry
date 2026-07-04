using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class ProjectStatusBL
    {
        public List<ProjectStatus> GetByCompany(int companyId)
        {
            using (var dal = ProjectStatusDAL.Instance)
            {
                return dal.GetByCompany(companyId);
            }
        }

        public ProjectStatus GetById(int projectStatusId)
        {
            using (var dal = ProjectStatusDAL.Instance)
            {
                return dal.GetById(projectStatusId);
            }
        }

        public int Create(ProjectStatus status)
        {
            using (var dal = ProjectStatusDAL.Instance)
            {
                return dal.Create(status);
            }
        }

        public void Update(ProjectStatus status)
        {
            using (var dal = ProjectStatusDAL.Instance)
            {
                dal.Update(status);
            }
        }

        public void Delete(int projectStatusId, int modifiedBy)
        {
            using (var dal = ProjectStatusDAL.Instance)
            {
                dal.Delete(projectStatusId, modifiedBy);
            }
        }
    }
}
