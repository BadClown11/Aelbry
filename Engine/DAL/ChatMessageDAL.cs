using Aelbry.BO;

namespace Aelbry.DAL
{
    public class ChatMessageDAL : MSSqlDatabase
    {
        public static ChatMessageDAL Instance
        {
            get { return new ChatMessageDAL(); }
        }

        private ChatMessageDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public ChatMessage GetById(int chatMessageId)
        {
            ChatMessage message = null;
            const string sSp = "SP_CHAT_MESSAGE_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_CHAT_MESSAGE_ID", chatMessageId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        message = MapMessage(reader);
                    }
                }
            }

            return message;
        }

        public List<ChatMessage> GetByProject(int projectId, int top)
        {
            var list = new List<ChatMessage>();
            const string sSp = "SP_CHAT_MESSAGE_GET_BY_PROJECT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
                cmd.Parameters.Add(CreateParameter("@P_TOP", top));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapMessage(reader));
                    }
                }
            }

            list.Reverse();
            return list;
        }

        public List<ChatMessage> GetByConversation(int conversationId, int top)
        {
            var list = new List<ChatMessage>();
            const string sSp = "SP_CHAT_MESSAGE_GET_BY_CONVERSATION";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_CONVERSATION_ID", conversationId));
                cmd.Parameters.Add(CreateParameter("@P_TOP", top));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapMessage(reader));
                    }
                }
            }

            list.Reverse();
            return list;
        }

        public int Insert(int? projectId, int? conversationId, int senderUserId, int? parentMessageId, string text)
        {
            const string sSp = "SP_CHAT_MESSAGE_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", (object)projectId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CONVERSATION_ID", (object)conversationId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_SENDER_USER_ID", senderUserId));
                cmd.Parameters.Add(CreateParameter("@P_PARENT_MESSAGE_ID", (object)parentMessageId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TEXT", text));

                var pNewId = CreateParameterOut("@P_NEW_CHAT_MESSAGE_ID", DbType.Int32, 4);
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

        public void Delete(int chatMessageId, int userId)
        {
            const string sSp = "SP_CHAT_MESSAGE_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_CHAT_MESSAGE_ID", chatMessageId));
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

        public List<ChatMessageReaction> GetReactionsByMessage(int chatMessageId)
        {
            return GetReactions("SP_CHAT_MESSAGE_GET_REACTIONS_BY_MESSAGE", "@P_CHAT_MESSAGE_ID", chatMessageId);
        }

        public List<ChatMessageReaction> GetReactionsByProject(int projectId)
        {
            return GetReactions("SP_CHAT_MESSAGE_GET_REACTIONS_BY_PROJECT", "@P_PROJECT_ID", projectId);
        }

        public List<ChatMessageReaction> GetReactionsByConversation(int conversationId)
        {
            return GetReactions("SP_CHAT_MESSAGE_GET_REACTIONS_BY_CONVERSATION", "@P_CONVERSATION_ID", conversationId);
        }

        private List<ChatMessageReaction> GetReactions(string sSp, string paramName, int paramValue)
        {
            var list = new List<ChatMessageReaction>();

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter(paramName, paramValue));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ChatMessageReaction
                        {
                            ChatMessageId = Validate.getDefaultIntIfDBNull(reader["CHAT_MESSAGE_ID"]),
                            UserId = Validate.getDefaultIntIfDBNull(reader["USER_ID"]),
                            UserName = Validate.getDefaultStringIfDBNull(reader["USER_NAME"]),
                            Emoji = Validate.getDefaultStringIfDBNull(reader["EMOJI"]),
                            CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                        });
                    }
                }
            }

            return list;
        }

        public void AddReaction(int chatMessageId, int userId, string emoji)
        {
            const string sSp = "SP_CHAT_MESSAGE_ADD_REACTION";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_CHAT_MESSAGE_ID", chatMessageId));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_EMOJI", emoji));

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

        public void RemoveReaction(int chatMessageId, int userId, string emoji)
        {
            const string sSp = "SP_CHAT_MESSAGE_REMOVE_REACTION";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_CHAT_MESSAGE_ID", chatMessageId));
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_EMOJI", emoji));

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

        private ChatMessage MapMessage(IDataReader reader)
        {
            return new ChatMessage
            {
                ChatMessageId = Validate.getDefaultIntIfDBNull(reader["CHAT_MESSAGE_ID"]),
                ProjectId = Validate.getDefaultNullableIntIfDBNull(reader["PROJECT_ID"]),
                ConversationId = Validate.getDefaultNullableIntIfDBNull(reader["CONVERSATION_ID"]),
                SenderUserId = Validate.getDefaultIntIfDBNull(reader["SENDER_USER_ID"]),
                SenderName = Validate.getDefaultStringIfDBNull(reader["SENDER_NAME"]),
                ParentMessageId = Validate.getDefaultNullableIntIfDBNull(reader["PARENT_MESSAGE_ID"]),
                Text = Validate.getDefaultStringIfDBNull(reader["TEXT"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                IsDeleted = Validate.getDefaultBoolIfDBNull(reader["IS_DELETED"]),
            };
        }
    }
}
