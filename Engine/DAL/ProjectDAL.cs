using Aelbry.BO;

namespace Aelbry.DAL
{
    public class ProjectDAL : MSSqlDatabase
    {
        public static ProjectDAL Instance
        {
            get { return new ProjectDAL(); }
        }

        private ProjectDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<Project> GetByCompany(int companyId)
        {
            var list = new List<Project>();
            const string sSp = "SP_PROJECT_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapProject(reader));
                    }
                }
            }

            return list;
        }

        public Project GetById(int projectId)
        {
            Project project = null;
            const string sSp = "SP_PROJECT_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        project = MapProject(reader);
                    }
                }
            }

            if (project != null)
            {
                project.Members = GetMembers(projectId);
                project.Tags = GetTags(projectId);
            }

            return project;
        }

        public int Create(Project project)
        {
            const string sSp = "SP_PROJECT_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", project.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_CODE", project.Code));
                cmd.Parameters.Add(CreateParameter("@P_NAME", project.Name));
                cmd.Parameters.Add(CreateParameter("@P_COLOR_HEX", project.ColorHex));
                cmd.Parameters.Add(CreateParameter("@P_COVER_IMAGE_URL", (object)project.CoverImageUrl ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CLIENT_NAME", (object)project.ClientName ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_STATUS_ID", project.ProjectStatusId));
                cmd.Parameters.Add(CreateParameter("@P_PRIORITY", (byte)project.Priority));
                cmd.Parameters.Add(CreateParameter("@P_RISK_LEVEL", (byte)project.RiskLevel));
                cmd.Parameters.Add(CreateParameter("@P_START_DATE", (object)project.StartDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_END_DATE", (object)project.EndDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_BUDGET", (object)project.Budget ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_HOURS", project.EstimatedHours));
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_MANAGER_ID", project.ProjectManagerId));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", project.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_PROJECT_ID", DbType.Int32, 4);
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

        public void Update(Project project)
        {
            const string sSp = "SP_PROJECT_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", project.ProjectId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", project.Name));
                cmd.Parameters.Add(CreateParameter("@P_COLOR_HEX", project.ColorHex));
                cmd.Parameters.Add(CreateParameter("@P_COVER_IMAGE_URL", (object)project.CoverImageUrl ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CLIENT_NAME", (object)project.ClientName ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_STATUS_ID", project.ProjectStatusId));
                cmd.Parameters.Add(CreateParameter("@P_PRIORITY", (byte)project.Priority));
                cmd.Parameters.Add(CreateParameter("@P_RISK_LEVEL", (byte)project.RiskLevel));
                cmd.Parameters.Add(CreateParameter("@P_START_DATE", (object)project.StartDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_END_DATE", (object)project.EndDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_BUDGET", (object)project.Budget ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ESTIMATED_HOURS", project.EstimatedHours));
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_MANAGER_ID", project.ProjectManagerId));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", project.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)project.ModifiedBy ?? DBNull.Value));

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
        /// SP dedicado usado por el servicio de recalculo de avance del Modulo 3 (ActivityBL).
        /// </summary>
        public void UpdateProgress(int projectId, decimal progressPercentage, int modifiedBy)
        {
            const string sSp = "SP_PROJECT_UPDATE_PROGRESS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
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

        public void Delete(int projectId, int modifiedBy)
        {
            const string sSp = "SP_PROJECT_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
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

        public List<ProjectMember> GetMembers(int projectId)
        {
            var list = new List<ProjectMember>();
            const string sSp = "SP_PROJECT_GET_MEMBERS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ProjectMember
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

        public void AddMember(int projectId, int userId)
        {
            const string sSp = "SP_PROJECT_ADD_MEMBER";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
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

        public void RemoveMember(int projectId, int userId)
        {
            const string sSp = "SP_PROJECT_REMOVE_MEMBER";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
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

        public List<Tag> GetTags(int projectId)
        {
            var list = new List<Tag>();
            const string sSp = "SP_PROJECT_GET_TAGS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));

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

        public void AddTag(int projectId, int tagId)
        {
            const string sSp = "SP_PROJECT_ADD_TAG";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
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

        public void RemoveTag(int projectId, int tagId)
        {
            const string sSp = "SP_PROJECT_REMOVE_TAG";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
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

        private Project MapProject(IDataReader reader)
        {
            return new Project
            {
                ProjectId = Validate.getDefaultIntIfDBNull(reader["PROJECT_ID"]),
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                Code = Validate.getDefaultStringIfDBNull(reader["CODE"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                ColorHex = Validate.getDefaultStringIfDBNull(reader["COLOR_HEX"]),
                CoverImageUrl = Validate.getDefaultStringIfDBNull(reader["COVER_IMAGE_URL"]),
                ClientName = Validate.getDefaultStringIfDBNull(reader["CLIENT_NAME"]),
                ProjectStatusId = Validate.getDefaultIntIfDBNull(reader["PROJECT_STATUS_ID"]),
                ProjectStatusName = Validate.getDefaultStringIfDBNull(reader["PROJECT_STATUS_NAME"]),
                Priority = (ProjectPriority)Validate.getDefaultIntIfDBNull(reader["PRIORITY"]),
                RiskLevel = (ProjectRiskLevel)Validate.getDefaultIntIfDBNull(reader["RISK_LEVEL"]),
                StartDate = Validate.getDefaultNullableDateIfDBNull(reader["START_DATE"]),
                EndDate = Validate.getDefaultNullableDateIfDBNull(reader["END_DATE"]),
                Budget = reader["BUDGET"] == DBNull.Value ? (decimal?)null : Validate.getDefaultDecimalIfDBNull(reader["BUDGET"]),
                EstimatedHours = Validate.getDefaultDecimalIfDBNull(reader["ESTIMATED_HOURS"]),
                WorkedHours = Validate.getDefaultDecimalIfDBNull(reader["WORKED_HOURS"]),
                ProgressPercentage = Validate.getDefaultDecimalIfDBNull(reader["PROGRESS_PERCENTAGE"]),
                ProjectManagerId = Validate.getDefaultIntIfDBNull(reader["PROJECT_MANAGER_ID"]),
                ProjectManagerName = Validate.getDefaultStringIfDBNull(reader["PROJECT_MANAGER_NAME"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
