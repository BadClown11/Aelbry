using Aelbry.BO;
using Aelbry.BO.Reports;

namespace Aelbry.DAL
{
    public class ReportDAL : MSSqlDatabase
    {
        public static ReportDAL Instance
        {
            get { return new ReportDAL(); }
        }

        private ReportDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<ReportActivityRow> GetActivities(ReportFilter filter)
        {
            var list = new List<ReportActivityRow>();
            const string sSp = "SP_REPORT_GET_ACTIVITIES";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", filter.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", (object)filter.ProjectId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_RESPONSIBLE_USER_ID", (object)filter.UserId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TEAM_ID", (object)filter.TeamId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_DEPARTMENT_ID", (object)filter.DepartmentId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_DUE_START_DATE", (object)filter.StartDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_DUE_END_DATE", (object)filter.EndDate ?? DBNull.Value));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapRow(reader));
                    }
                }
            }

            return list;
        }

        private ReportActivityRow MapRow(IDataReader reader)
        {
            return new ReportActivityRow
            {
                ActivityId = Validate.getDefaultIntIfDBNull(reader["ACTIVITY_ID"]),
                Code = Validate.getDefaultStringIfDBNull(reader["CODE"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                Description = Validate.getDefaultStringIfDBNull(reader["DESCRIPTION"]),
                ProjectId = Validate.getDefaultIntIfDBNull(reader["PROJECT_ID"]),
                ProjectName = Validate.getDefaultStringIfDBNull(reader["PROJECT_NAME"]),
                ResponsibleUserId = Validate.getDefaultIntIfDBNull(reader["RESPONSIBLE_USER_ID"]),
                ResponsibleName = Validate.getDefaultStringIfDBNull(reader["RESPONSIBLE_NAME"]),
                TeamId = Validate.getDefaultNullableIntIfDBNull(reader["TEAM_ID"]),
                TeamName = Validate.getDefaultStringIfDBNull(reader["TEAM_NAME"]),
                DepartmentId = Validate.getDefaultNullableIntIfDBNull(reader["DEPARTMENT_ID"]),
                DepartmentName = Validate.getDefaultStringIfDBNull(reader["DEPARTMENT_NAME"]),
                Status = (ActivityStatus)Validate.getDefaultIntIfDBNull(reader["STATUS"]),
                Priority = (ProjectPriority)Validate.getDefaultIntIfDBNull(reader["PRIORITY"]),
                EstimatedStartDate = Validate.getDefaultNullableDateIfDBNull(reader["ESTIMATED_START_DATE"]),
                EstimatedEndDate = Validate.getDefaultNullableDateIfDBNull(reader["ESTIMATED_END_DATE"]),
                ActualStartDate = Validate.getDefaultNullableDateIfDBNull(reader["ACTUAL_START_DATE"]),
                ActualEndDate = Validate.getDefaultNullableDateIfDBNull(reader["ACTUAL_END_DATE"]),
                EstimatedHours = Validate.getDefaultDecimalIfDBNull(reader["ESTIMATED_HOURS"]),
                WorkedHours = Validate.getDefaultDecimalIfDBNull(reader["WORKED_HOURS"]),
                ProgressPercentage = Validate.getDefaultDecimalIfDBNull(reader["PROGRESS_PERCENTAGE"]),
            };
        }
    }
}
