using Aelbry.BO;

namespace Aelbry.DAL
{
    public class ChatConversationDAL : MSSqlDatabase
    {
        public static ChatConversationDAL Instance
        {
            get { return new ChatConversationDAL(); }
        }

        private ChatConversationDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public int GetOrCreate(int userId1, int userId2)
        {
            const string sSp = "SP_CHAT_CONVERSATION_GET_OR_CREATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID_1", userId1));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID_2", userId2));

                var pConversationId = CreateParameterOut("@P_CONVERSATION_ID", DbType.Int32, 4);
                cmd.Parameters.Add(pConversationId);

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }

                return Validate.getDefaultIntIfDBNull(pConversationId.Value);
            }
        }

        public List<ChatConversation> GetByUser(int userId)
        {
            var list = new List<ChatConversation>();
            const string sSp = "SP_CHAT_CONVERSATION_GET_BY_USER";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ChatConversation
                        {
                            ConversationId = Validate.getDefaultIntIfDBNull(reader["CONVERSATION_ID"]),
                            OtherUserId = Validate.getDefaultIntIfDBNull(reader["OTHER_USER_ID"]),
                            OtherUserName = Validate.getDefaultStringIfDBNull(reader["OTHER_USER_NAME"]),
                            CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                        });
                    }
                }
            }

            return list;
        }
    }
}
