using Aelbry.BO;

namespace Aelbry.DAL
{
    public class ActivityDAL : MSSqlDatabase
    {
        public static ActivityDAL Instance
        {
            get { return new ActivityDAL(); }
        }

        private ActivityDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        /// <summary>
        /// Trae todas las actividades del proyecto (planas) y ensambla el arbol en memoria
        /// usando ParentActivityId. Los nodos raiz son los que quedan en la lista devuelta.
        /// </summary>
        public List<Activity> GetTreeByProject(int projectId)
        {
            var all = GetFlatByProject(projectId);
            var byId = all.ToDictionary(a => a.ActivityId);
            var roots = new List<Activity>();

            foreach (var activity in all)
            {
                if (activity.ParentActivityId.HasValue && byId.TryGetValue(activity.ParentActivityId.Value, out var parent))
                {
                    parent.Children.Add(activity);
                }
                else
                {
                    roots.Add(activity);
                }
            }

            return roots;
        }

        public List<Activity> GetFlatByProject(int projectId)
        {
            var list = new List<Activity>();
            const string sSp = "SP_ACTIVITY_GET_BY_PROJECT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapActivity(reader));
                    }
                }
            }

            return list;
        }

        public Activity GetById(int activityId)
        {
            Activity activity = null;
            const string sSp = "SP_ACTIVITY_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        activity = MapActivity(reader);
                    }
                }
            }

            if (activity != null)
            {
                activity.Participants = GetParticipants(activityId);
                activity.Tags = GetTags(activityId);
                activity.ChecklistItems = GetChecklistItems(activityId);
                activity.Dependencies = GetDependencies(activityId);
            }

            return activity;
        }

        public List<(int ActivityId, decimal Weight, decimal Progress)> GetChildrenWeightProgress(int parentActivityId)
        {
            return GetWeightProgress("SP_ACTIVITY_GET_CHILDREN", "@P_PARENT_ACTIVITY_ID", parentActivityId);
        }

        public List<(int ActivityId, decimal Weight, decimal Progress)> GetRootWeightProgressByProject(int projectId)
        {
            return GetWeightProgress("SP_ACTIVITY_GET_ROOT_BY_PROJECT", "@P_PROJECT_ID", projectId);
        }

        public List<(int ActivityId, decimal Weight, decimal Progress)> GetByUser(int userId)
        {
            return GetWeightProgress("SP_ACTIVITY_GET_BY_USER", "@P_USER_ID", userId);
        }

        public List<(int ActivityId, decimal Weight, decimal Progress)> GetByTeam(int teamId)
        {
            return GetWeightProgress("SP_ACTIVITY_GET_BY_TEAM", "@P_TEAM_ID", teamId);
        }

        private List<(int ActivityId, decimal Weight, decimal Progress)> GetWeightProgress(string sSp, string paramName, int paramValue)
        {
            var list = new List<(int, decimal, decimal)>();

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter(paramName, paramValue));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((
                            Validate.getDefaultIntIfDBNull(reader["ACTIVITY_ID"]),
                            Validate.getDefaultDecimalIfDBNull(reader["WEIGHT"]),
                            Validate.getDefaultDecimalIfDBNull(reader["PROGRESS_PERCENTAGE"])));
                    }
                }
            }

            return list;
        }

        public int Create(Activity activity)
        {
            const string sSp = "SP_ACTIVITY_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", activity.ProjectId));
                cmd.Parameters.Add(CreateParameter("@P_PARENT_ACTIVITY_ID", (object)activity.ParentActivityId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CODE", activity.Code));
                cmd.Parameters.Add(CreateParameter("@P_NAME", activity.Name));
                cmd.Parameters.Add(CreateParameter("@P_DESCRIPTION", (object)activity.Description ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CATEGORY", (object)activity.Category ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_COLOR_HEX", activity.ColorHex));
                cmd.Parameters.Add(CreateParameter("@P_STATUS", (byte)activity.Status));
                cmd.Parameters.Add(CreateParameter("@P_PRIORITY", (byte)activity.Priority));
                cmd.Parameters.Add(CreateParameter("@P_RESPONSIBLE_USER_ID", activity.ResponsibleUserId));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_START_DATE", (object)activity.EstimatedStartDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_END_DATE", (object)activity.EstimatedEndDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_WEIGHT", activity.Weight));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_HOURS", activity.EstimatedHours));
                cmd.Parameters.Add(CreateParameter("@P_SEQUENCE", activity.Sequence));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", activity.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_ACTIVITY_ID", DbType.Int32, 4);
                cmd.Parameters.Add(pNewId);

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }

                return Validate.getDefaultIntIfDBNull(pNewId.Value);
            }
        }

        public void Update(Activity activity)
        {
            const string sSp = "SP_ACTIVITY_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activity.ActivityId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", activity.Name));
                cmd.Parameters.Add(CreateParameter("@P_DESCRIPTION", (object)activity.Description ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CATEGORY", (object)activity.Category ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_COLOR_HEX", activity.ColorHex));
                cmd.Parameters.Add(CreateParameter("@P_STATUS", (byte)activity.Status));
                cmd.Parameters.Add(CreateParameter("@P_PRIORITY", (byte)activity.Priority));
                cmd.Parameters.Add(CreateParameter("@P_RESPONSIBLE_USER_ID", activity.ResponsibleUserId));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_START_DATE", (object)activity.EstimatedStartDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_END_DATE", (object)activity.EstimatedEndDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTUAL_START_DATE", (object)activity.ActualStartDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTUAL_END_DATE", (object)activity.ActualEndDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_WEIGHT", activity.Weight));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_HOURS", activity.EstimatedHours));
                cmd.Parameters.Add(CreateParameter("@P_WORKED_HOURS", activity.WorkedHours));
                cmd.Parameters.Add(CreateParameter("@P_SEQUENCE", activity.Sequence));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", activity.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)activity.ModifiedBy ?? DBNull.Value));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public void UpdateProgress(int activityId, decimal progressPercentage, int modifiedBy)
        {
            const string sSp = "SP_ACTIVITY_UPDATE_PROGRESS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_PROGRESS_PERCENTAGE", progressPercentage));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", modifiedBy));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        /// <summary>
        /// Actualizacion liviana de solo las horas trabajadas (Modulo 6: agregado desde TimeEntry).
        /// </summary>
        public void UpdateWorkedHours(int activityId, decimal workedHours, int modifiedBy)
        {
            const string sSp = "SP_ACTIVITY_UPDATE_WORKED_HOURS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_WORKED_HOURS", workedHours));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", modifiedBy));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        /// <summary>
        /// Actualizacion liviana de solo el estado (Modulo 5: drag&amp;drop del tablero Kanban).
        /// </summary>
        public void UpdateStatus(int activityId, ActivityStatus status, int modifiedBy)
        {
            const string sSp = "SP_ACTIVITY_UPDATE_STATUS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_STATUS", (byte)status));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", modifiedBy));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        /// <summary>
        /// Actualizacion liviana de solo las fechas estimadas (Modulo 5: arrastre de barras en el Gantt).
        /// </summary>
        public void UpdateDates(int activityId, DateTime? estimatedStartDate, DateTime? estimatedEndDate, int modifiedBy)
        {
            const string sSp = "SP_ACTIVITY_UPDATE_DATES";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_START_DATE", (object)estimatedStartDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_END_DATE", (object)estimatedEndDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", modifiedBy));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public void Delete(int activityId, int modifiedBy)
        {
            const string sSp = "SP_ACTIVITY_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", modifiedBy));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public List<ActivityParticipant> GetParticipants(int activityId)
        {
            var list = new List<ActivityParticipant>();
            const string sSp = "SP_ACTIVITY_GET_PARTICIPANTS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ActivityParticipant
                        {
                            UserId = Validate.getDefaultIntIfDBNull(reader["USER_ID"]),
                            FirstName = Validate.getDefaultStringIfDBNull(reader["FIRST_NAME"]),
                            LastName = Validate.getDefaultStringIfDBNull(reader["LAST_NAME"]),
                            Email = Validate.getDefaultStringIfDBNull(reader["EMAIL"]),
                            JobTitle = Validate.getDefaultStringIfDBNull(reader["JOB_TITLE"]),
                            PhotoUrl = Validate.getDefaultStringIfDBNull(reader["PHOTO_URL"]),
                            AddedDate = Validate.getDefaultDateIfDBNull(reader["ADDED_DATE"]),
                        });
                    }
                }
            }

            return list;
        }

        public void AddParticipant(int activityId, int userId)
        {
            const string sSp = "SP_ACTIVITY_ADD_PARTICIPANT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public void RemoveParticipant(int activityId, int userId)
        {
            const string sSp = "SP_ACTIVITY_REMOVE_PARTICIPANT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public List<Tag> GetTags(int activityId)
        {
            var list = new List<Tag>();
            const string sSp = "SP_ACTIVITY_GET_TAGS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Tag
                        {
                            TagId = Validate.getDefaultIntIfDBNull(reader["TAG_ID"]),
                            CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                            Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                            ColorHex = Validate.getDefaultStringIfDBNull(reader["COLOR_HEX"]),
                            IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                        });
                    }
                }
            }

            return list;
        }

        public void AddTag(int activityId, int tagId)
        {
            const string sSp = "SP_ACTIVITY_ADD_TAG";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_TAG_ID", tagId));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public void RemoveTag(int activityId, int tagId)
        {
            const string sSp = "SP_ACTIVITY_REMOVE_TAG";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_TAG_ID", tagId));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public List<ChecklistItem> GetChecklistItems(int activityId)
        {
            var list = new List<ChecklistItem>();
            const string sSp = "SP_CHECKLIST_ITEM_GET_BY_ACTIVITY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ChecklistItem
                        {
                            ChecklistItemId = Validate.getDefaultIntIfDBNull(reader["CHECKLIST_ITEM_ID"]),
                            ActivityId = Validate.getDefaultIntIfDBNull(reader["ACTIVITY_ID"]),
                            Text = Validate.getDefaultStringIfDBNull(reader["TEXT"]),
                            IsChecked = Validate.getDefaultBoolIfDBNull(reader["IS_CHECKED"]),
                            Sequence = Validate.getDefaultIntIfDBNull(reader["SEQUENCE"]),
                            CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                            CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                            CompletedBy = Validate.getDefaultNullableIntIfDBNull(reader["COMPLETED_BY"]),
                            CompletedDate = Validate.getDefaultNullableDateIfDBNull(reader["COMPLETED_DATE"]),
                        });
                    }
                }
            }

            return list;
        }

        public int AddChecklistItem(int activityId, string text, int sequence, int createdBy)
        {
            const string sSp = "SP_CHECKLIST_ITEM_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_TEXT", text));
                cmd.Parameters.Add(CreateParameter("@P_SEQUENCE", sequence));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", createdBy));

                var pNewId = CreateParameterOut("@P_NEW_CHECKLIST_ITEM_ID", DbType.Int32, 4);
                cmd.Parameters.Add(pNewId);

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }

                return Validate.getDefaultIntIfDBNull(pNewId.Value);
            }
        }

        public void ToggleChecklistItem(int checklistItemId, bool isChecked, int modifiedBy)
        {
            const string sSp = "SP_CHECKLIST_ITEM_TOGGLE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_CHECKLIST_ITEM_ID", checklistItemId));
                cmd.Parameters.Add(CreateParameter("@P_IS_CHECKED", isChecked));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", modifiedBy));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public void DeleteChecklistItem(int checklistItemId)
        {
            const string sSp = "SP_CHECKLIST_ITEM_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_CHECKLIST_ITEM_ID", checklistItemId));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public List<ActivityDependency> GetDependencies(int activityId)
        {
            var list = new List<ActivityDependency>();
            const string sSp = "SP_ACTIVITY_DEPENDENCY_GET_BY_ACTIVITY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ActivityDependency
                        {
                            ActivityDependencyId = Validate.getDefaultIntIfDBNull(reader["ACTIVITY_DEPENDENCY_ID"]),
                            ActivityId = Validate.getDefaultIntIfDBNull(reader["ACTIVITY_ID"]),
                            DependsOnActivityId = Validate.getDefaultIntIfDBNull(reader["DEPENDS_ON_ACTIVITY_ID"]),
                            DependsOnActivityName = Validate.getDefaultStringIfDBNull(reader["DEPENDS_ON_ACTIVITY_NAME"]),
                            DependencyType = (DependencyType)Validate.getDefaultIntIfDBNull(reader["DEPENDENCY_TYPE"]),
                            CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                            CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                        });
                    }
                }
            }

            return list;
        }

        public int AddDependency(int activityId, int dependsOnActivityId, DependencyType dependencyType, int createdBy)
        {
            const string sSp = "SP_ACTIVITY_DEPENDENCY_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_DEPENDS_ON_ACTIVITY_ID", dependsOnActivityId));
                cmd.Parameters.Add(CreateParameter("@P_DEPENDENCY_TYPE", (byte)dependencyType));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", createdBy));

                var pNewId = CreateParameterOut("@P_NEW_ACTIVITY_DEPENDENCY_ID", DbType.Int32, 4);
                cmd.Parameters.Add(pNewId);

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }

                return Validate.getDefaultIntIfDBNull(pNewId.Value);
            }
        }

        public void RemoveDependency(int activityDependencyId)
        {
            const string sSp = "SP_ACTIVITY_DEPENDENCY_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_DEPENDENCY_ID", activityDependencyId));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        private Activity MapActivity(IDataReader reader)
        {
            return new Activity
            {
                ActivityId = Validate.getDefaultIntIfDBNull(reader["ACTIVITY_ID"]),
                ProjectId = Validate.getDefaultIntIfDBNull(reader["PROJECT_ID"]),
                ParentActivityId = Validate.getDefaultNullableIntIfDBNull(reader["PARENT_ACTIVITY_ID"]),
                Code = Validate.getDefaultStringIfDBNull(reader["CODE"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                Description = Validate.getDefaultStringIfDBNull(reader["DESCRIPTION"]),
                Category = Validate.getDefaultStringIfDBNull(reader["CATEGORY"]),
                ColorHex = Validate.getDefaultStringIfDBNull(reader["COLOR_HEX"]),
                Status = (ActivityStatus)Validate.getDefaultIntIfDBNull(reader["STATUS"]),
                Priority = (ProjectPriority)Validate.getDefaultIntIfDBNull(reader["PRIORITY"]),
                ResponsibleUserId = Validate.getDefaultIntIfDBNull(reader["RESPONSIBLE_USER_ID"]),
                ResponsibleName = Validate.getDefaultStringIfDBNull(reader["RESPONSIBLE_NAME"]),
                EstimatedStartDate = Validate.getDefaultNullableDateIfDBNull(reader["ESTIMATED_START_DATE"]),
                EstimatedEndDate = Validate.getDefaultNullableDateIfDBNull(reader["ESTIMATED_END_DATE"]),
                ActualStartDate = Validate.getDefaultNullableDateIfDBNull(reader["ACTUAL_START_DATE"]),
                ActualEndDate = Validate.getDefaultNullableDateIfDBNull(reader["ACTUAL_END_DATE"]),
                Weight = Validate.getDefaultDecimalIfDBNull(reader["WEIGHT"]),
                EstimatedHours = Validate.getDefaultDecimalIfDBNull(reader["ESTIMATED_HOURS"]),
                WorkedHours = Validate.getDefaultDecimalIfDBNull(reader["WORKED_HOURS"]),
                ProgressPercentage = Validate.getDefaultDecimalIfDBNull(reader["PROGRESS_PERCENTAGE"]),
                Sequence = Validate.getDefaultIntIfDBNull(reader["SEQUENCE"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
