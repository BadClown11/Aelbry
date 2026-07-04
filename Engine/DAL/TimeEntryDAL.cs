using Aelbry.BO;

namespace Aelbry.DAL
{
    public class TimeEntryDAL : MSSqlDatabase
    {
        public static TimeEntryDAL Instance
        {
            get { return new TimeEntryDAL(); }
        }

        private TimeEntryDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public TimeEntry GetById(int timeEntryId)
        {
            TimeEntry entry = null;
            const string sSp = "SP_TIME_ENTRY_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TIME_ENTRY_ID", timeEntryId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        entry = MapTimeEntry(reader);
                    }
                }
            }

            return entry;
        }

        public TimeEntry GetRunningByUser(int userId)
        {
            TimeEntry entry = null;
            const string sSp = "SP_TIME_ENTRY_GET_RUNNING_BY_USER";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        entry = MapTimeEntry(reader);
                    }
                }
            }

            return entry;
        }

        public List<TimeEntry> GetByActivity(int activityId)
        {
            var list = new List<TimeEntry>();
            const string sSp = "SP_TIME_ENTRY_GET_BY_ACTIVITY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapTimeEntry(reader));
                    }
                }
            }

            return list;
        }

        public List<TimeEntry> GetByUser(int userId, DateTime? startDate, DateTime? endDate)
        {
            var list = new List<TimeEntry>();
            const string sSp = "SP_TIME_ENTRY_GET_BY_USER";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_START_DATE", (object)startDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_END_DATE", (object)endDate ?? DBNull.Value));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapTimeEntry(reader));
                    }
                }
            }

            return list;
        }

        public decimal GetTotalHoursByActivity(int activityId)
        {
            const string sSp = "SP_TIME_ENTRY_GET_TOTAL_HOURS_BY_ACTIVITY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));

                var pTotal = CreateParameterOut("@P_TOTAL_HOURS", DbType.Decimal, 9);
                cmd.Parameters.Add(pTotal);

                cmd.ExecuteNonQuery();

                return Validate.getDefaultDecimalIfDBNull(pTotal.Value);
            }
        }

        public int Start(int activityId, int userId, int createdBy)
        {
            const string sSp = "SP_TIME_ENTRY_START";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", activityId));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", createdBy));

                var pNewId = CreateParameterOut("@P_NEW_TIME_ENTRY_ID", DbType.Int32, 4);
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

        public void Stop(int timeEntryId, int modifiedBy)
        {
            const string sSp = "SP_TIME_ENTRY_STOP";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TIME_ENTRY_ID", timeEntryId));
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

        public int CreateManual(TimeEntry entry)
        {
            const string sSp = "SP_TIME_ENTRY_INSERT_MANUAL";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ACTIVITY_ID", entry.ActivityId));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", entry.UserId));
                cmd.Parameters.Add(CreateParameter("@P_START_TIME", entry.StartTime));
                cmd.Parameters.Add(CreateParameter("@P_END_TIME", (object)entry.EndTime ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_DURATION_HOURS", entry.DurationHours));
                cmd.Parameters.Add(CreateParameter("@P_IS_OVERTIME", entry.IsOvertime));
                cmd.Parameters.Add(CreateParameter("@P_NOTES", (object)entry.Notes ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", entry.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_TIME_ENTRY_ID", DbType.Int32, 4);
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

        public void Update(int timeEntryId, decimal durationHours, bool isOvertime, string notes, int modifiedBy)
        {
            const string sSp = "SP_TIME_ENTRY_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TIME_ENTRY_ID", timeEntryId));
                cmd.Parameters.Add(CreateParameter("@P_DURATION_HOURS", durationHours));
                cmd.Parameters.Add(CreateParameter("@P_IS_OVERTIME", isOvertime));
                cmd.Parameters.Add(CreateParameter("@P_NOTES", (object)notes ?? DBNull.Value));
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

        public void Delete(int timeEntryId, int modifiedBy)
        {
            const string sSp = "SP_TIME_ENTRY_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TIME_ENTRY_ID", timeEntryId));
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

        private TimeEntry MapTimeEntry(IDataReader reader)
        {
            return new TimeEntry
            {
                TimeEntryId = Validate.getDefaultIntIfDBNull(reader["TIME_ENTRY_ID"]),
                ActivityId = Validate.getDefaultIntIfDBNull(reader["ACTIVITY_ID"]),
                ActivityName = Validate.getDefaultStringIfDBNull(reader["ACTIVITY_NAME"]),
                UserId = Validate.getDefaultIntIfDBNull(reader["USER_ID"]),
                UserName = Validate.getDefaultStringIfDBNull(reader["USER_NAME"]),
                StartTime = Validate.getDefaultDateIfDBNull(reader["START_TIME"]),
                EndTime = Validate.getDefaultNullableDateIfDBNull(reader["END_TIME"]),
                DurationHours = Validate.getDefaultDecimalIfDBNull(reader["DURATION_HOURS"]),
                IsManual = Validate.getDefaultBoolIfDBNull(reader["IS_MANUAL"]),
                IsOvertime = Validate.getDefaultBoolIfDBNull(reader["IS_OVERTIME"]),
                Notes = Validate.getDefaultStringIfDBNull(reader["NOTES"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
