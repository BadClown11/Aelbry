using Aelbry.BO;
using Aelbry.DAL;

namespace Aelbry.BL
{
    /// <summary>
    /// Cronometro en vivo (Start/Stop) y registro manual de horas (Modulo 6). Cada mutacion
    /// recalcula Activity.WorkedHours (suma de TimeEntry) y dispara la cascada de avance.
    /// </summary>
    public class TimeEntryBL
    {
        private readonly ActivityBL _activityBL;

        public TimeEntryBL(ActivityBL activityBL)
        {
            _activityBL = activityBL;
        }

        public TimeEntry GetRunningByUser(int userId)
        {
            using (var dal = TimeEntryDAL.Instance)
            {
                return dal.GetRunningByUser(userId);
            }
        }

        public List<TimeEntry> GetByActivity(int activityId)
        {
            using (var dal = TimeEntryDAL.Instance)
            {
                return dal.GetByActivity(activityId);
            }
        }

        public List<TimeEntry> GetByUser(int userId, DateTime? startDate, DateTime? endDate)
        {
            using (var dal = TimeEntryDAL.Instance)
            {
                return dal.GetByUser(userId, startDate, endDate);
            }
        }

        /// <summary>
        /// Inicia el cronometro para el usuario en una actividad. Si ya tenia uno corriendo
        /// (en cualquier actividad), lo detiene primero (Start funciona tambien como "cambiar de tarea").
        /// </summary>
        public int Start(int activityId, int userId)
        {
            using (var dal = TimeEntryDAL.Instance)
            {
                var running = dal.GetRunningByUser(userId);
                if (running != null)
                {
                    StopAndRecalculate(dal, running.TimeEntryId, running.ActivityId, userId);
                }

                return dal.Start(activityId, userId, userId);
            }
        }

        /// <summary>
        /// Detiene el cronometro (tambien usado como "Pausar": reanudar es un nuevo Start).
        /// </summary>
        public void Stop(int timeEntryId, int userId)
        {
            using (var dal = TimeEntryDAL.Instance)
            {
                var entry = dal.GetById(timeEntryId)
                    ?? throw new InvalidOperationException("El registro de tiempo no existe.");

                if (entry.UserId != userId)
                {
                    throw new InvalidOperationException("No puedes detener el cronometro de otro usuario.");
                }

                StopAndRecalculate(dal, timeEntryId, entry.ActivityId, userId);
            }
        }

        private void StopAndRecalculate(TimeEntryDAL dal, int timeEntryId, int activityId, int userId)
        {
            dal.Stop(timeEntryId, userId);
            RecalculateActivityHours(dal, activityId, userId);
        }

        public int CreateManual(TimeEntry entry, int userId)
        {
            entry.UserId = userId;
            entry.CreatedBy = userId;
            entry.IsManual = true;
            entry.EndTime ??= entry.StartTime.AddHours((double)entry.DurationHours);

            using (var dal = TimeEntryDAL.Instance)
            {
                int newId = dal.CreateManual(entry);
                RecalculateActivityHours(dal, entry.ActivityId, userId);
                return newId;
            }
        }

        public void Update(int timeEntryId, decimal durationHours, bool isOvertime, string notes, int userId)
        {
            using (var dal = TimeEntryDAL.Instance)
            {
                var entry = dal.GetById(timeEntryId)
                    ?? throw new InvalidOperationException("El registro de tiempo no existe.");

                if (entry.UserId != userId)
                {
                    throw new InvalidOperationException("No puedes editar el registro de tiempo de otro usuario.");
                }

                dal.Update(timeEntryId, durationHours, isOvertime, notes, userId);
                RecalculateActivityHours(dal, entry.ActivityId, userId);
            }
        }

        public void Delete(int timeEntryId, int userId)
        {
            using (var dal = TimeEntryDAL.Instance)
            {
                var entry = dal.GetById(timeEntryId)
                    ?? throw new InvalidOperationException("El registro de tiempo no existe.");

                if (entry.UserId != userId)
                {
                    throw new InvalidOperationException("No puedes eliminar el registro de tiempo de otro usuario.");
                }

                dal.Delete(timeEntryId, userId);
                RecalculateActivityHours(dal, entry.ActivityId, userId);
            }
        }

        private void RecalculateActivityHours(TimeEntryDAL dal, int activityId, int userId)
        {
            decimal totalHours = dal.GetTotalHoursByActivity(activityId);
            _activityBL.UpdateWorkedHours(activityId, totalHours, userId);
        }
    }
}
