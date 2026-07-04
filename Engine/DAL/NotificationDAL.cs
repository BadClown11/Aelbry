using Aelbry.BO;

namespace Aelbry.DAL
{
    public class NotificationDAL : MSSqlDatabase
    {
        public static NotificationDAL Instance
        {
            get { return new NotificationDAL(); }
        }

        private NotificationDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public Notification GetById(int notificationId)
        {
            Notification notification = null;
            const string sSp = "SP_NOTIFICATION_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_NOTIFICATION_ID", notificationId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        notification = MapNotification(reader);
                    }
                }
            }

            return notification;
        }

        public List<Notification> GetByUser(int userId, bool unreadOnly)
        {
            var list = new List<Notification>();
            const string sSp = "SP_NOTIFICATION_GET_BY_USER";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_UNREAD_ONLY", unreadOnly));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapNotification(reader));
                    }
                }
            }

            return list;
        }

        public int GetUnreadCount(int userId)
        {
            const string sSp = "SP_NOTIFICATION_GET_UNREAD_COUNT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));

                var pCount = CreateParameterOut("@P_COUNT", DbType.Int32, 4);
                cmd.Parameters.Add(pCount);

                cmd.ExecuteNonQuery();

                return Validate.getDefaultIntIfDBNull(pCount.Value);
            }
        }

        public int Create(int userId, string title, string message, string link)
        {
            const string sSp = "SP_NOTIFICATION_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_TITLE", title));
                cmd.Parameters.Add(CreateParameter("@P_MESSAGE", message));
                cmd.Parameters.Add(CreateParameter("@P_LINK", (object)link ?? DBNull.Value));

                var pNewId = CreateParameterOut("@P_NEW_NOTIFICATION_ID", DbType.Int32, 4);
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

        public void MarkAsRead(int notificationId, int userId)
        {
            const string sSp = "SP_NOTIFICATION_MARK_AS_READ";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_NOTIFICATION_ID", notificationId));
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

        public void MarkAllAsRead(int userId)
        {
            const string sSp = "SP_NOTIFICATION_MARK_ALL_AS_READ";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
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

        private Notification MapNotification(IDataReader reader)
        {
            return new Notification
            {
                NotificationId = Validate.getDefaultIntIfDBNull(reader["NOTIFICATION_ID"]),
                UserId = Validate.getDefaultIntIfDBNull(reader["USER_ID"]),
                Title = Validate.getDefaultStringIfDBNull(reader["TITLE"]),
                Message = Validate.getDefaultStringIfDBNull(reader["MESSAGE"]),
                Link = Validate.getDefaultStringIfDBNull(reader["LINK"]),
                IsRead = Validate.getDefaultBoolIfDBNull(reader["IS_READ"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
            };
        }
    }
}
