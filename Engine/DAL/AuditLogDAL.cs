using Aelbry.BO;

namespace Aelbry.DAL
{
    public class AuditLogDAL : MSSqlDatabase
    {
        public static AuditLogDAL Instance
        {
            get { return new AuditLogDAL(); }
        }

        private AuditLogDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public void Insert(int companyId, int userId, string userName, string ipAddress, string module, string action, int? entityId, string dataBefore, string dataAfter)
        {
            const string sSp = "SP_AUDIT_LOG_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_USER_NAME", userName));
                cmd.Parameters.Add(CreateParameter("@P_IP_ADDRESS", (object)ipAddress ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_MODULE", module));
                cmd.Parameters.Add(CreateParameter("@P_ACTION", action));
                cmd.Parameters.Add(CreateParameter("@P_ENTITY_ID", (object)entityId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_DATA_BEFORE", (object)dataBefore ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_DATA_AFTER", (object)dataAfter ?? DBNull.Value));

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

        public List<AuditLog> GetByCompany(AuditLogFilter filter)
        {
            var list = new List<AuditLog>();
            const string sSp = "SP_AUDIT_LOG_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", filter.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_MODULE", (object)filter.Module ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", (object)filter.UserId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_START_DATE", (object)filter.StartDate ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_END_DATE", (object)filter.EndDate ?? DBNull.Value));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapAuditLog(reader));
                    }
                }
            }

            return list;
        }

        private AuditLog MapAuditLog(IDataReader reader)
        {
            return new AuditLog
            {
                AuditLogId = Validate.getDefaultIntIfDBNull(reader["AUDIT_LOG_ID"]),
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                UserId = Validate.getDefaultIntIfDBNull(reader["USER_ID"]),
                UserName = Validate.getDefaultStringIfDBNull(reader["USER_NAME"]),
                IPAddress = Validate.getDefaultStringIfDBNull(reader["IP_ADDRESS"]),
                Module = Validate.getDefaultStringIfDBNull(reader["MODULE"]),
                Action = Validate.getDefaultStringIfDBNull(reader["ACTION"]),
                EntityId = Validate.getDefaultNullableIntIfDBNull(reader["ENTITY_ID"]),
                DataBefore = Validate.getDefaultStringIfDBNull(reader["DATA_BEFORE"]),
                DataAfter = Validate.getDefaultStringIfDBNull(reader["DATA_AFTER"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
            };
        }
    }
}
