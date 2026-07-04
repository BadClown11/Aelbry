using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class ProjectBL
    {
        private readonly ActivityBL _activityBL;

        public ProjectBL(ActivityBL activityBL)
        {
            _activityBL = activityBL;
        }

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

        /// <summary>
        /// Duplicacion profunda (Modulo 4): clona el proyecto, sus etiquetas, y el arbol
        /// completo de actividades (subactividades, checklists, etiquetas de cada una).
        /// </summary>
        public int Duplicate(int projectId, string newCode, string newName, int currentUserId)
        {
            using (var dal = ProjectDAL.Instance)
            {
                var source = dal.GetById(projectId)
                    ?? throw new InvalidOperationException("El proyecto no existe.");

                var clone = new Project
                {
                    CompanyId = source.CompanyId,
                    Code = newCode,
                    Name = newName,
                    ColorHex = source.ColorHex,
                    ClientName = source.ClientName,
                    ProjectStatusId = source.ProjectStatusId,
                    Priority = source.Priority,
                    RiskLevel = source.RiskLevel,
                    EstimatedHours = source.EstimatedHours,
                    ProjectManagerId = source.ProjectManagerId,
                    CreatedBy = currentUserId,
                };

                int newProjectId = dal.Create(clone);

                foreach (var tag in dal.GetTags(projectId))
                {
                    dal.AddTag(newProjectId, tag.TagId);
                }

                _activityBL.DuplicateProjectActivities(projectId, newProjectId, currentUserId);

                return newProjectId;
            }
        }

        /// <summary>
        /// Aplica una plantilla corporativa (Modulo 4): crea el proyecto con los valores por
        /// defecto de la plantilla y clona su esqueleto de actividades (lista plana, nivel raiz).
        /// </summary>
        public int CreateFromTemplate(int projectTemplateId, string code, string name, int companyId,
            int projectStatusId, int projectManagerId, int currentUserId)
        {
            using (var templateDal = ProjectTemplateDAL.Instance)
            using (var projectDal = ProjectDAL.Instance)
            {
                var template = templateDal.GetById(projectTemplateId)
                    ?? throw new InvalidOperationException("La plantilla no existe.");

                var project = new Project
                {
                    CompanyId = companyId,
                    Code = code,
                    Name = name,
                    ColorHex = "#4C6EF5",
                    ProjectStatusId = projectStatusId,
                    Priority = template.DefaultPriority,
                    RiskLevel = ProjectRiskLevel.Low,
                    EstimatedHours = template.DefaultEstimatedHours,
                    ProjectManagerId = projectManagerId,
                    CreatedBy = currentUserId,
                };

                int newProjectId = projectDal.Create(project);

                foreach (var templateActivity in templateDal.GetActivities(projectTemplateId))
                {
                    var activity = new Activity
                    {
                        ProjectId = newProjectId,
                        Name = templateActivity.Name,
                        Description = templateActivity.Description,
                        ColorHex = "#4C6EF5",
                        Status = ActivityStatus.Pending,
                        Priority = ProjectPriority.Medium,
                        ResponsibleUserId = projectManagerId,
                        Weight = 1,
                        EstimatedHours = templateActivity.EstimatedHours,
                        CreatedBy = currentUserId,
                    };

                    _activityBL.Create(activity);
                }

                return newProjectId;
            }
        }
    }
}
