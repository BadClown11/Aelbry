using Aelbry.BO;

namespace Aelbry.DAL
{
    public class ProjectStatusDAL : MSSqlDatabase
    {
        public static ProjectStatusDAL Instance
        {
            get { return new ProjectStatusDAL(); }
        }

        private ProjectStatusDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<ProjectStatus> GetByCompany(int companyId)
        {
            var list = new List<ProjectStatus>();
            const string sSp = "SP_PROJECT_STATUS_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapProjectStatus(reader));
                    }
                }
            }

            return list;
        }

        public ProjectStatus GetById(int projectStatusId)
        {
            ProjectStatus status = null;
            const string sSp = "SP_PROJECT_STATUS_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_STATUS_ID", projectStatusId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        status = MapProjectStatus(reader);
                    }
                }
            }

            return status;
        }

        public int Create(ProjectStatus status)
        {
            const string sSp = "SP_PROJECT_STATUS_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", status.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", status.Name));
                cmd.Parameters.Add(CreateParameter("@P_COLOR_HEX", status.ColorHex));
                cmd.Parameters.Add(CreateParameter("@P_SEQUENCE", status.Sequence));
                cmd.Parameters.Add(CreateParameter("@P_IS_FINAL", status.IsFinal));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", status.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_PROJECT_STATUS_ID", DbType.Int32, 4);
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

        public void Update(ProjectStatus status)
        {
            const string sSp = "SP_PROJECT_STATUS_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_STATUS_ID", status.ProjectStatusId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", status.Name));
                cmd.Parameters.Add(CreateParameter("@P_COLOR_HEX", status.ColorHex));
                cmd.Parameters.Add(CreateParameter("@P_SEQUENCE", status.Sequence));
                cmd.Parameters.Add(CreateParameter("@P_IS_FINAL", status.IsFinal));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", status.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)status.ModifiedBy ?? DBNull.Value));

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

        public void Delete(int projectStatusId, int modifiedBy)
        {
            const string sSp = "SP_PROJECT_STATUS_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_STATUS_ID", projectStatusId));
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

        private ProjectStatus MapProjectStatus(IDataReader reader)
        {
            return new ProjectStatus
            {
                ProjectStatusId = Validate.getDefaultIntIfDBNull(reader["PROJECT_STATUS_ID"]),
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                ColorHex = Validate.getDefaultStringIfDBNull(reader["COLOR_HEX"]),
                Sequence = Validate.getDefaultIntIfDBNull(reader["SEQUENCE"]),
                IsFinal = Validate.getDefaultBoolIfDBNull(reader["IS_FINAL"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
