using Aelbry.BO;

namespace Aelbry.DAL
{
    public class ActivityDueReminderDAL : MSSqlDatabase
    {
        public static ActivityDueReminderDAL Instance
        {
            get { return new ActivityDueReminderDAL(); }
        }

        private ActivityDueReminderDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<ActivityDueReminderCandidate> GetCandidates(int daysAhead)
        {
            var list = new List<ActivityDueReminderCandidate>();
            const string sSp = "SP_ACTIVITY_DUE_REMINDER_GET_CANDIDATES";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DAYS_AHEAD", daysAhead));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ActivityDueReminderCandidate
                        {
                            ActivityId = Validate.getDefaultIntIfDBNull(reader["ACTIVITY_ID"]),
                            Code = Validate.getDefaultStringIfDBNull(reader["CODE"]),
                            Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                            EstimatedEndDate = Validate.getDefaultDateIfDBNull(reader["ESTIMATED_END_DATE"]),
                            ResponsibleUserId = Validate.getDefaultIntIfDBNull(reader["RESPONSIBLE_USER_ID"]),
                            ResponsibleEmail = Validate.getDefaultStringIfDBNull(reader["RESPONSIBLE_EMAIL"]),
                            ResponsibleName = Validate.getDefaultStringIfDBNull(reader["RESPONSIBLE_NAME"]),
                            ProjectId = Validate.getDefaultIntIfDBNull(reader["PROJECT_ID"]),
                        });
                    }
                }
            }

            return list;
        }

        public void MarkSent(int activityId)
        {
            const string sSp = "SP_ACTIVITY_DUE_REMINDER_MARK_SENT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));

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
    }
}
