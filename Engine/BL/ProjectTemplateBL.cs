using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class ProjectTemplateBL
    {
        public List<ProjectTemplate> GetByCompany(int companyId)
        {
            using (var dal = ProjectTemplateDAL.Instance)
            {
                return dal.GetByCompany(companyId);
            }
        }

        public ProjectTemplate GetById(int projectTemplateId)
        {
            using (var dal = ProjectTemplateDAL.Instance)
            {
                return dal.GetById(projectTemplateId);
            }
        }

        public int Create(ProjectTemplate template)
        {
            using (var dal = ProjectTemplateDAL.Instance)
            {
                return dal.Create(template);
            }
        }

        public void Update(ProjectTemplate template)
        {
            using (var dal = ProjectTemplateDAL.Instance)
            {
                dal.Update(template);
            }
        }

        public void Delete(int projectTemplateId, int modifiedBy)
        {
            using (var dal = ProjectTemplateDAL.Instance)
            {
                dal.Delete(projectTemplateId, modifiedBy);
            }
        }

        public List<ProjectTemplateActivity> GetActivities(int projectTemplateId)
        {
            using (var dal = ProjectTemplateDAL.Instance)
            {
                return dal.GetActivities(projectTemplateId);
            }
        }

        public int AddActivity(int projectTemplateId, string name, string description, decimal estimatedHours, int sequence, int createdBy)
        {
            using (var dal = ProjectTemplateDAL.Instance)
            {
                return dal.AddActivity(projectTemplateId, name, description, estimatedHours, sequence, createdBy);
            }
        }

        public void RemoveActivity(int projectTemplateActivityId)
        {
            using (var dal = ProjectTemplateDAL.Instance)
            {
                dal.RemoveActivity(projectTemplateActivityId);
            }
        }
    }
}
