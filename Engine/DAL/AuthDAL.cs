using Aelbry.BO;

namespace Aelbry.DAL
{
    public class AuthDAL : MSSqlDatabase
    {
        public static AuthDAL Instance
        {
            get { return new AuthDAL(); }
        }

        private AuthDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public int SaveRefreshToken(RefreshToken token)
        {
            const string sSp = "SP_AUTH_SAVE_REFRESH_TOKEN";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", token.UserId));
                cmd.Parameters.Add(CreateParameter("@P_TOKEN", token.Token));
                cmd.Parameters.Add(CreateParameter("@P_EXPIRES_AT", token.ExpiresAt));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY_IP", token.CreatedByIp));

                var pNewId = CreateParameterOut("@P_NEW_REFRESH_TOKEN_ID", DbType.Int32, 4);
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

        public RefreshToken GetByToken(string token)
        {
            RefreshToken refreshToken = null;
            const string sSp = "SP_AUTH_GET_REFRESH_TOKEN";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TOKEN", token));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        refreshToken = new RefreshToken
                        {
                            RefreshTokenId = Validate.getDefaultIntIfDBNull(reader["REFRESH_TOKEN_ID"]),
                            UserId = Validate.getDefaultIntIfDBNull(reader["USER_ID"]),
                            Token = Validate.getDefaultStringIfDBNull(reader["TOKEN"]),
                            ExpiresAt = Validate.getDefaultDateIfDBNull(reader["EXPIRES_AT"]),
                            CreatedAt = Validate.getDefaultDateIfDBNull(reader["CREATED_AT"]),
                            CreatedByIp = Validate.getDefaultStringIfDBNull(reader["CREATED_BY_IP"]),
                            RevokedAt = Validate.getDefaultNullableDateIfDBNull(reader["REVOKED_AT"]),
                            ReplacedByToken = Validate.getDefaultStringIfDBNull(reader["REPLACED_BY_TOKEN"]),
                        };
                    }
                }
            }

            return refreshToken;
        }

        /// <summary>
        /// Revoca el refresh token actual y crea el reemplazo dentro de una misma transaccion,
        /// evitando dejar un token huerfano si el paso de insercion fallara.
        /// </summary>
        public int RotateRefreshToken(string oldToken, RefreshToken newToken, string revokedByIp)
        {
            BeginTransaction();

            try
            {
                int newId = ExecuteRevokeAndReplace(oldToken, newToken, revokedByIp);
                CommitTransaction();
                return newId;
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private int ExecuteRevokeAndReplace(string oldToken, RefreshToken newToken, string revokedByIp)
        {
            const string sSpRevoke = "SP_AUTH_REVOKE_REFRESH_TOKEN";

            using (var cmd = CreateStoredProcCommand(sSpRevoke, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TOKEN", oldToken));
                cmd.Parameters.Add(CreateParameter("@P_REPLACED_BY_TOKEN", newToken.Token));
                cmd.Parameters.Add(CreateParameter("@P_REVOKED_BY_IP", revokedByIp));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }

            const string sSpInsert = "SP_AUTH_SAVE_REFRESH_TOKEN";

            using (var cmd = CreateStoredProcCommand(sSpInsert, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", newToken.UserId));
                cmd.Parameters.Add(CreateParameter("@P_TOKEN", newToken.Token));
                cmd.Parameters.Add(CreateParameter("@P_EXPIRES_AT", newToken.ExpiresAt));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY_IP", newToken.CreatedByIp));

                var pNewId = CreateParameterOut("@P_NEW_REFRESH_TOKEN_ID", DbType.Int32, 4);
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

        public void RevokeToken(string token, string revokedByIp)
        {
            const string sSp = "SP_AUTH_REVOKE_REFRESH_TOKEN";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TOKEN", token));
                cmd.Parameters.Add(CreateParameter("@P_REPLACED_BY_TOKEN", DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_REVOKED_BY_IP", revokedByIp));

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
