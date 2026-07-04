using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    public class ActivityBL
    {
        public List<Activity> GetTreeByProject(int projectId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                return dal.GetTreeByProject(projectId);
            }
        }

        public Activity GetById(int activityId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                return dal.GetById(activityId);
            }
        }

        /// <summary>
        /// Genera el codigo WBS (ej. "PROJ-1", subactividad "PROJ-1.1") a partir del proyecto
        /// o de la actividad padre, y dispara el recalculo en cascada del avance ponderado.
        /// </summary>
        public int Create(Activity activity)
        {
            using (var dal = ActivityDAL.Instance)
            using (var projectDal = ProjectDAL.Instance)
            {
                var project = projectDal.GetById(activity.ProjectId)
                    ?? throw new InvalidOperationException("El proyecto no existe.");

                var siblings = dal.GetFlatByProject(activity.ProjectId)
                    .Where(a => a.ParentActivityId == activity.ParentActivityId)
                    .ToList();

                if (activity.ParentActivityId.HasValue)
                {
                    var parent = dal.GetById(activity.ParentActivityId.Value)
                        ?? throw new InvalidOperationException("La actividad padre no existe.");
                    activity.Code = $"{parent.Code}.{siblings.Count + 1}";
                }
                else
                {
                    activity.Code = $"{project.Code}-{siblings.Count + 1}";
                }

                int newId = dal.Create(activity);
                RecalculateProgress(newId, activity.CreatedBy);

                return newId;
            }
        }

        public void Update(Activity activity)
        {
            using (var dal = ActivityDAL.Instance)
            {
                dal.Update(activity);
            }

            RecalculateProgress(activity.ActivityId, activity.ModifiedBy ?? activity.CreatedBy);
        }

        public void Delete(int activityId, int modifiedBy)
        {
            int? parentActivityId;
            int projectId;

            using (var dal = ActivityDAL.Instance)
            {
                var activity = dal.GetById(activityId)
                    ?? throw new InvalidOperationException("La actividad no existe.");

                parentActivityId = activity.ParentActivityId;
                projectId = activity.ProjectId;

                dal.Delete(activityId, modifiedBy);
            }

            if (parentActivityId.HasValue)
            {
                RecalculateProgress(parentActivityId.Value, modifiedBy);
            }
            else
            {
                RecalculateProjectProgress(projectId, modifiedBy);
            }
        }

        public List<ActivityParticipant> GetParticipants(int activityId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                return dal.GetParticipants(activityId);
            }
        }

        public void AddParticipant(int activityId, int userId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                dal.AddParticipant(activityId, userId);
            }
        }

        public void RemoveParticipant(int activityId, int userId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                dal.RemoveParticipant(activityId, userId);
            }
        }

        public List<Tag> GetTags(int activityId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                return dal.GetTags(activityId);
            }
        }

        public void AddTag(int activityId, int tagId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                dal.AddTag(activityId, tagId);
            }
        }

        public void RemoveTag(int activityId, int tagId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                dal.RemoveTag(activityId, tagId);
            }
        }

        public List<ChecklistItem> GetChecklistItems(int activityId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                return dal.GetChecklistItems(activityId);
            }
        }

        public int AddChecklistItem(int activityId, string text, int sequence, int createdBy)
        {
            int newId;

            using (var dal = ActivityDAL.Instance)
            {
                newId = dal.AddChecklistItem(activityId, text, sequence, createdBy);
            }

            RecalculateProgress(activityId, createdBy);
            return newId;
        }

        public void ToggleChecklistItem(int checklistItemId, int activityId, bool isChecked, int modifiedBy)
        {
            using (var dal = ActivityDAL.Instance)
            {
                dal.ToggleChecklistItem(checklistItemId, isChecked, modifiedBy);
            }

            RecalculateProgress(activityId, modifiedBy);
        }

        public void DeleteChecklistItem(int checklistItemId, int activityId, int modifiedBy)
        {
            using (var dal = ActivityDAL.Instance)
            {
                dal.DeleteChecklistItem(checklistItemId);
            }

            RecalculateProgress(activityId, modifiedBy);
        }

        public List<ActivityDependency> GetDependencies(int activityId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                return dal.GetDependencies(activityId);
            }
        }

        public int AddDependency(int activityId, int dependsOnActivityId, DependencyType dependencyType, int createdBy)
        {
            if (activityId == dependsOnActivityId)
            {
                throw new InvalidOperationException("Una actividad no puede depender de si misma.");
            }

            using (var dal = ActivityDAL.Instance)
            {
                if (CreatesCycle(dal, dependsOnActivityId, activityId))
                {
                    throw new InvalidOperationException("Esta dependencia generaria un ciclo entre actividades.");
                }

                return dal.AddDependency(activityId, dependsOnActivityId, dependencyType, createdBy);
            }
        }

        public void RemoveDependency(int activityDependencyId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                dal.RemoveDependency(activityDependencyId);
            }
        }

        /// <summary>
        /// Avance ponderado promedio de las actividades donde el usuario es responsable o participante.
        /// </summary>
        public decimal GetUserProgress(int userId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                var activities = dal.GetByUser(userId);
                return ActivityProgressCalculator.CalculateWeightedProgress(activities.Select(a => (a.Weight, a.Progress)));
            }
        }

        /// <summary>
        /// Avance ponderado promedio de las actividades asignadas a los miembros de un equipo.
        /// </summary>
        public decimal GetTeamProgress(int teamId)
        {
            using (var dal = ActivityDAL.Instance)
            {
                var activities = dal.GetByTeam(teamId);
                return ActivityProgressCalculator.CalculateWeightedProgress(activities.Select(a => (a.Weight, a.Progress)));
            }
        }

        /// <summary>
        /// Recalcula el avance de la actividad (hoja: checklist/horas, o con hijos: promedio ponderado),
        /// y propaga el recalculo hacia arriba: actividad padre -> ... -> proyecto.
        /// </summary>
        public void RecalculateProgress(int activityId, int modifiedBy)
        {
            using (var dal = ActivityDAL.Instance)
            {
                RecalculateActivityAndAncestors(dal, activityId, modifiedBy);
            }
        }

        private void RecalculateActivityAndAncestors(ActivityDAL dal, int activityId, int modifiedBy)
        {
            var activity = dal.GetById(activityId);
            if (activity == null)
            {
                return;
            }

            var children = dal.GetChildrenWeightProgress(activityId);

            decimal progress = children.Count > 0
                ? ActivityProgressCalculator.CalculateWeightedProgress(children.Select(c => (c.Weight, c.Progress)))
                : ActivityProgressCalculator.CalculateLeafProgress(
                    activity.ChecklistItems.Count(i => i.IsChecked),
                    activity.ChecklistItems.Count,
                    activity.WorkedHours,
                    activity.EstimatedHours);

            dal.UpdateProgress(activityId, progress, modifiedBy);

            if (activity.ParentActivityId.HasValue)
            {
                RecalculateActivityAndAncestors(dal, activity.ParentActivityId.Value, modifiedBy);
            }
            else
            {
                RecalculateProjectProgress(activity.ProjectId, modifiedBy);
            }
        }

        private void RecalculateProjectProgress(int projectId, int modifiedBy)
        {
            using (var dal = ActivityDAL.Instance)
            using (var projectDal = ProjectDAL.Instance)
            {
                var roots = dal.GetRootWeightProgressByProject(projectId);
                decimal progress = ActivityProgressCalculator.CalculateWeightedProgress(roots.Select(r => (r.Weight, r.Progress)));

                projectDal.UpdateProgress(projectId, progress, modifiedBy);
            }
        }

        private bool CreatesCycle(ActivityDAL dal, int currentActivityId, int targetActivityId)
        {
            if (currentActivityId == targetActivityId)
            {
                return true;
            }

            foreach (var dependency in dal.GetDependencies(currentActivityId))
            {
                if (CreatesCycle(dal, dependency.DependsOnActivityId, targetActivityId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
