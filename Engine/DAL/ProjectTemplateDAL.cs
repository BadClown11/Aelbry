using Aelbry.BO;

namespace Aelbry.DAL
{
    public class ProjectTemplateDAL : MSSqlDatabase
    {
        public static ProjectTemplateDAL Instance
        {
            get { return new ProjectTemplateDAL(); }
        }

        private ProjectTemplateDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<ProjectTemplate> GetByCompany(int companyId)
        {
            var list = new List<ProjectTemplate>();
            const string sSp = "SP_PROJECT_TEMPLATE_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapProjectTemplate(reader));
                    }
                }
            }

            return list;
        }

        public ProjectTemplate GetById(int projectTemplateId)
        {
            ProjectTemplate template = null;
            const string sSp = "SP_PROJECT_TEMPLATE_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_TEMPLATE_ID", projectTemplateId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        template = MapProjectTemplate(reader);
                    }
                }
            }

            return template;
        }

        public int Create(ProjectTemplate template)
        {
            const string sSp = "SP_PROJECT_TEMPLATE_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", template.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", template.Name));
                cmd.Parameters.Add(CreateParameter("@P_DESCRIPTION", (object)template.Description ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_DEFAULT_PRIORITY", (byte)template.DefaultPriority));
                cmd.Parameters.Add(CreateParameter("@P_DEFAULT_ESTIMATED_HOURS", template.DefaultEstimatedHours));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", template.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_PROJECT_TEMPLATE_ID", DbType.Int32, 4);
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

        public void Update(ProjectTemplate template)
        {
            const string sSp = "SP_PROJECT_TEMPLATE_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_TEMPLATE_ID", template.ProjectTemplateId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", template.Name));
                cmd.Parameters.Add(CreateParameter("@P_DESCRIPTION", (object)template.Description ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_DEFAULT_PRIORITY", (byte)template.DefaultPriority));
                cmd.Parameters.Add(CreateParameter("@P_DEFAULT_ESTIMATED_HOURS", template.DefaultEstimatedHours));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", template.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)template.ModifiedBy ?? DBNull.Value));

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

        public void Delete(int projectTemplateId, int modifiedBy)
        {
            const string sSp = "SP_PROJECT_TEMPLATE_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_TEMPLATE_ID", projectTemplateId));
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

        private ProjectTemplate MapProjectTemplate(IDataReader reader)
        {
            return new ProjectTemplate
            {
                ProjectTemplateId = Validate.getDefaultIntIfDBNull(reader["PROJECT_TEMPLATE_ID"]),
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                Description = Validate.getDefaultStringIfDBNull(reader["DESCRIPTION"]),
                DefaultPriority = (ProjectPriority)Validate.getDefaultIntIfDBNull(reader["DEFAULT_PRIORITY"]),
                DefaultEstimatedHours = Validate.getDefaultDecimalIfDBNull(reader["DEFAULT_ESTIMATED_HOURS"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
