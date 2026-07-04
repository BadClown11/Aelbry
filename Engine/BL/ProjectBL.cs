using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class ProjectBL
    {
        public List<Project> GetByCompany(int companyId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                return dal.GetByCompany(companyId);
            }
        }

        public Project GetById(int projectId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                return dal.GetById(projectId);
            }
        }

        public int Create(Project project)
        {
            using (var dal = ProjectDAL.Instance)
            {
                return dal.Create(project);
            }
        }

        public void Update(Project project)
        {
            using (var dal = ProjectDAL.Instance)
            {
                dal.Update(project);
            }
        }

        public void Delete(int projectId, int modifiedBy)
        {
            using (var dal = ProjectDAL.Instance)
            {
                dal.Delete(projectId, modifiedBy);
            }
        }

        public List<ProjectMember> GetMembers(int projectId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                return dal.GetMembers(projectId);
            }
        }

        public void AddMember(int projectId, int userId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                dal.AddMember(projectId, userId);
            }
        }

        public void RemoveMember(int projectId, int userId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                dal.RemoveMember(projectId, userId);
            }
        }

        public List<Tag> GetTags(int projectId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                return dal.GetTags(projectId);
            }
        }

        public void AddTag(int projectId, int tagId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                dal.AddTag(projectId, tagId);
            }
        }

        public void RemoveTag(int projectId, int tagId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                dal.RemoveTag(projectId, tagId);
            }
        }
    }
}
