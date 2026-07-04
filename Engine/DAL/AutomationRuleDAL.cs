using Aelbry.BO;

namespace Aelbry.DAL
{
    public class AutomationRuleDAL : MSSqlDatabase
    {
        public static AutomationRuleDAL Instance
        {
            get { return new AutomationRuleDAL(); }
        }

        private AutomationRuleDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<AutomationRule> GetByCompany(int companyId)
        {
            var list = new List<AutomationRule>();
            const string sSp = "SP_AUTOMATION_RULE_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapRule(reader));
                    }
                }
            }

            return list;
        }

        public AutomationRule GetById(int automationRuleId)
        {
            AutomationRule rule = null;
            const string sSp = "SP_AUTOMATION_RULE_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_AUTOMATION_RULE_ID", automationRuleId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rule = MapRule(reader);
                    }
                }
            }

            return rule;
        }

        public List<AutomationRule> GetActiveByActivityTrigger(int triggerActivityId, AutomationTriggerType triggerType)
        {
            var list = new List<AutomationRule>();
            const string sSp = "SP_AUTOMATION_RULE_GET_ACTIVE_BY_ACTIVITY_TRIGGER";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_ACTIVITY_ID", triggerActivityId));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_TYPE", (byte)triggerType));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapRule(reader));
                    }
                }
            }

            return list;
        }

        public List<AutomationRule> GetActiveByProjectTrigger(int triggerProjectId, AutomationTriggerType triggerType)
        {
            var list = new List<AutomationRule>();
            const string sSp = "SP_AUTOMATION_RULE_GET_ACTIVE_BY_PROJECT_TRIGGER";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_PROJECT_ID", triggerProjectId));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_TYPE", (byte)triggerType));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapRule(reader));
                    }
                }
            }

            return list;
        }

        public int Create(AutomationRule rule)
        {
            const string sSp = "SP_AUTOMATION_RULE_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", rule.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", rule.Name));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_TYPE", (byte)rule.TriggerType));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_ACTIVITY_ID", (object)rule.TriggerActivityId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_PROJECT_ID", (object)rule.TriggerProjectId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_THRESHOLD_PERCENT", (object)rule.TriggerThresholdPercent ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_STATUS", rule.TriggerStatus.HasValue ? (byte)rule.TriggerStatus.Value : (object)DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_TYPE", (byte)rule.ActionType));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_TARGET_ACTIVITY_ID", (object)rule.ActionTargetActivityId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_TARGET_PROJECT_ID", (object)rule.ActionTargetProjectId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_NEW_ACTIVITY_STATUS", rule.ActionNewActivityStatus.HasValue ? (byte)rule.ActionNewActivityStatus.Value : (object)DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_NEW_PROJECT_STATUS_ID", (object)rule.ActionNewProjectStatusId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_NOTIFICATION_MESSAGE", (object)rule.ActionNotificationMessage ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_NOTIFICATION_USER_ID", (object)rule.ActionNotificationUserId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", rule.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_AUTOMATION_RULE_ID", DbType.Int32, 4);
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

        public void Update(AutomationRule rule)
        {
            const string sSp = "SP_AUTOMATION_RULE_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_AUTOMATION_RULE_ID", rule.AutomationRuleId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", rule.Name));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_TYPE", (byte)rule.TriggerType));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_ACTIVITY_ID", (object)rule.TriggerActivityId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_PROJECT_ID", (object)rule.TriggerProjectId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_THRESHOLD_PERCENT", (object)rule.TriggerThresholdPercent ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TRIGGER_STATUS", rule.TriggerStatus.HasValue ? (byte)rule.TriggerStatus.Value : (object)DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_TYPE", (byte)rule.ActionType));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_TARGET_ACTIVITY_ID", (object)rule.ActionTargetActivityId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_TARGET_PROJECT_ID", (object)rule.ActionTargetProjectId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_NEW_ACTIVITY_STATUS", rule.ActionNewActivityStatus.HasValue ? (byte)rule.ActionNewActivityStatus.Value : (object)DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_NEW_PROJECT_STATUS_ID", (object)rule.ActionNewProjectStatusId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_NOTIFICATION_MESSAGE", (object)rule.ActionNotificationMessage ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_ACTION_NOTIFICATION_USER_ID", (object)rule.ActionNotificationUserId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", rule.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)rule.ModifiedBy ?? DBNull.Value));

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

        public void Delete(int automationRuleId, int modifiedBy)
        {
            const string sSp = "SP_AUTOMATION_RULE_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_AUTOMATION_RULE_ID", automationRuleId));
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

        public List<AutomationRuleLog> GetLogsByRule(int automationRuleId)
        {
            var list = new List<AutomationRuleLog>();
            const string sSp = "SP_AUTOMATION_RULE_LOG_GET_BY_RULE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_AUTOMATION_RULE_ID", automationRuleId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new AutomationRuleLog
                        {
                            AutomationRuleLogId = Validate.getDefaultIntIfDBNull(reader["AUTOMATION_RULE_LOG_ID"]),
                            AutomationRuleId = Validate.getDefaultIntIfDBNull(reader["AUTOMATION_RULE_ID"]),
                            TriggeredDate = Validate.getDefaultDateIfDBNull(reader["TRIGGERED_DATE"]),
                            Success = Validate.getDefaultBoolIfDBNull(reader["SUCCESS"]),
                            Details = Validate.getDefaultStringIfDBNull(reader["DETAILS"]),
                        });
                    }
                }
            }

            return list;
        }

        public void AddLog(int automationRuleId, bool success, string details)
        {
            const string sSp = "SP_AUTOMATION_RULE_LOG_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_AUTOMATION_RULE_ID", automationRuleId));
                cmd.Parameters.Add(CreateParameter("@P_SUCCESS", success));
                cmd.Parameters.Add(CreateParameter("@P_DETAILS", (object)details ?? DBNull.Value));

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

        private AutomationRule MapRule(IDataReader reader)
        {
            return new AutomationRule
            {
                AutomationRuleId = Validate.getDefaultIntIfDBNull(reader["AUTOMATION_RULE_ID"]),
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                TriggerType = (AutomationTriggerType)Validate.getDefaultIntIfDBNull(reader["TRIGGER_TYPE"]),
                TriggerActivityId = Validate.getDefaultNullableIntIfDBNull(reader["TRIGGER_ACTIVITY_ID"]),
                TriggerActivityName = Validate.getDefaultStringIfDBNull(reader["TRIGGER_ACTIVITY_NAME"]),
                TriggerProjectId = Validate.getDefaultNullableIntIfDBNull(reader["TRIGGER_PROJECT_ID"]),
                TriggerProjectName = Validate.getDefaultStringIfDBNull(reader["TRIGGER_PROJECT_NAME"]),
                TriggerThresholdPercent = reader["TRIGGER_THRESHOLD_PERCENT"] == DBNull.Value ? (decimal?)null : Validate.getDefaultDecimalIfDBNull(reader["TRIGGER_THRESHOLD_PERCENT"]),
                TriggerStatus = reader["TRIGGER_STATUS"] == DBNull.Value ? (ActivityStatus?)null : (ActivityStatus)Validate.getDefaultIntIfDBNull(reader["TRIGGER_STATUS"]),
                ActionType = (AutomationActionType)Validate.getDefaultIntIfDBNull(reader["ACTION_TYPE"]),
                ActionTargetActivityId = Validate.getDefaultNullableIntIfDBNull(reader["ACTION_TARGET_ACTIVITY_ID"]),
                ActionTargetActivityName = Validate.getDefaultStringIfDBNull(reader["ACTION_TARGET_ACTIVITY_NAME"]),
                ActionTargetProjectId = Validate.getDefaultNullableIntIfDBNull(reader["ACTION_TARGET_PROJECT_ID"]),
                ActionTargetProjectName = Validate.getDefaultStringIfDBNull(reader["ACTION_TARGET_PROJECT_NAME"]),
                ActionNewActivityStatus = reader["ACTION_NEW_ACTIVITY_STATUS"] == DBNull.Value ? (ActivityStatus?)null : (ActivityStatus)Validate.getDefaultIntIfDBNull(reader["ACTION_NEW_ACTIVITY_STATUS"]),
                ActionNewProjectStatusId = Validate.getDefaultNullableIntIfDBNull(reader["ACTION_NEW_PROJECT_STATUS_ID"]),
                ActionNotificationMessage = Validate.getDefaultStringIfDBNull(reader["ACTION_NOTIFICATION_MESSAGE"]),
                ActionNotificationUserId = Validate.getDefaultNullableIntIfDBNull(reader["ACTION_NOTIFICATION_USER_ID"]),
                ActionNotificationUserName = Validate.getDefaultStringIfDBNull(reader["ACTION_NOTIFICATION_USER_NAME"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
