using Aelbry.BO;

namespace Aelbry.DAL
{
    public class UserDAL : MSSqlDatabase
    {
        public static UserDAL Instance
        {
            get { return new UserDAL(); }
        }

        private UserDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<User> GetByCompany(int companyId)
        {
            var list = new List<User>();
            const string sSp = "SP_USER_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapUser(reader));
                    }
                }
            }

            return list;
        }

        public List<User> GetByTeam(int teamId)
        {
            var list = new List<User>();
            const string sSp = "SP_USER_GET_BY_TEAM";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TEAM_ID", teamId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapUser(reader));
                    }
                }
            }

            return list;
        }

        public User GetById(int userId)
        {
            User user = null;
            const string sSp = "SP_USER_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = MapUser(reader);
                    }
                }
            }

            if (user != null)
            {
                user.Roles = GetRoles(userId);
                user.Permissions = GetPermissions(userId);
            }

            return user;
        }

        /// <summary>
        /// Devuelve el usuario incluyendo PasswordHash; uso exclusivo de AuthDAL/AuthBL para el login.
        /// </summary>
        public User GetByEmail(string email)
        {
            User user = null;
            const string sSp = "SP_USER_GET_BY_EMAIL";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_EMAIL", email));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = MapUser(reader);
                        user.PasswordHash = Validate.getDefaultStringIfDBNull(reader["PASSWORD_HASH"]);
                    }
                }
            }

            if (user != null)
            {
                user.Roles = GetRoles(user.UserId);
                user.Permissions = GetPermissions(user.UserId);
            }

            return user;
        }

        public int Create(User user)
        {
            const string sSp = "SP_USER_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", user.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_DEPARTMENT_ID", (object)user.DepartmentId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TEAM_ID", (object)user.TeamId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_FIRST_NAME", user.FirstName));
                cmd.Parameters.Add(CreateParameter("@P_LAST_NAME", user.LastName));
                cmd.Parameters.Add(CreateParameter("@P_EMAIL", user.Email));
                cmd.Parameters.Add(CreateParameter("@P_PASSWORD_HASH", user.PasswordHash));
                cmd.Parameters.Add(CreateParameter("@P_JOB_TITLE", (object)user.JobTitle ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TIME_ZONE", user.TimeZone));
                cmd.Parameters.Add(CreateParameter("@P_WORK_SCHEDULE_JSON", (object)user.WorkScheduleJson ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_PROFILE_COLOR", (object)user.ProfileColor ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", user.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_USER_ID", DbType.Int32, 4);
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

        public void Update(User user)
        {
            const string sSp = "SP_USER_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", user.UserId));
                cmd.Parameters.Add(CreateParameter("@P_DEPARTMENT_ID", (object)user.DepartmentId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TEAM_ID", (object)user.TeamId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_FIRST_NAME", user.FirstName));
                cmd.Parameters.Add(CreateParameter("@P_LAST_NAME", user.LastName));
                cmd.Parameters.Add(CreateParameter("@P_JOB_TITLE", (object)user.JobTitle ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_PHOTO_URL", (object)user.PhotoUrl ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TIME_ZONE", user.TimeZone));
                cmd.Parameters.Add(CreateParameter("@P_WORK_SCHEDULE_JSON", (object)user.WorkScheduleJson ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_PROFILE_COLOR", (object)user.ProfileColor ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", user.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)user.ModifiedBy ?? DBNull.Value));

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

        public void UpdatePassword(int userId, string passwordHash, int modifiedBy)
        {
            const string sSp = "SP_USER_UPDATE_PASSWORD";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_PASSWORD_HASH", passwordHash));
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

        public void Delete(int userId, int modifiedBy)
        {
            const string sSp = "SP_USER_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
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

        public List<Role> GetRoles(int userId)
        {
            var list = new List<Role>();
            const string sSp = "SP_USER_GET_ROLES";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Role
                        {
                            RoleId = Validate.getDefaultIntIfDBNull(reader["ROLE_ID"]),
                            Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                            Description = Validate.getDefaultStringIfDBNull(reader["DESCRIPTION"]),
                            IsSystemDefault = Validate.getDefaultBoolIfDBNull(reader["IS_SYSTEM_DEFAULT"]),
                            IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                        });
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Permisos efectivos del usuario (union de los permisos de todos sus roles activos).
        /// </summary>
        public List<Permission> GetPermissions(int userId)
        {
            var list = new List<Permission>();
            const string sSp = "SP_USER_GET_PERMISSIONS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Permission
                        {
                            PermissionId = Validate.getDefaultIntIfDBNull(reader["PERMISSION_ID"]),
                            Code = Validate.getDefaultStringIfDBNull(reader["CODE"]),
                            Module = Validate.getDefaultStringIfDBNull(reader["MODULE"]),
                            Description = Validate.getDefaultStringIfDBNull(reader["DESCRIPTION"]),
                        });
                    }
                }
            }

            return list;
        }

        public void AssignRole(int userId, int roleId, int companyId)
        {
            const string sSp = "SP_USER_ASSIGN_ROLE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_ROLE_ID", roleId));
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

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

        public void RemoveRole(int userId, int roleId, int companyId)
        {
            const string sSp = "SP_USER_REMOVE_ROLE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_USER_ID", userId));
                cmd.Parameters.Add(CreateParameter("@P_ROLE_ID", roleId));
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

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

        private User MapUser(IDataReader reader)
        {
            return new User
            {
                UserId = Validate.getDefaultIntIfDBNull(reader["USER_ID"]),
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                DepartmentId = Validate.getDefaultNullableIntIfDBNull(reader["DEPARTMENT_ID"]),
                TeamId = Validate.getDefaultNullableIntIfDBNull(reader["TEAM_ID"]),
                FirstName = Validate.getDefaultStringIfDBNull(reader["FIRST_NAME"]),
                LastName = Validate.getDefaultStringIfDBNull(reader["LAST_NAME"]),
                Email = Validate.getDefaultStringIfDBNull(reader["EMAIL"]),
                JobTitle = Validate.getDefaultStringIfDBNull(reader["JOB_TITLE"]),
                PhotoUrl = Validate.getDefaultStringIfDBNull(reader["PHOTO_URL"]),
                TimeZone = Validate.getDefaultStringIfDBNull(reader["TIME_ZONE"]),
                WorkScheduleJson = Validate.getDefaultStringIfDBNull(reader["WORK_SCHEDULE_JSON"]),
                ProfileColor = Validate.getDefaultStringIfDBNull(reader["PROFILE_COLOR"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
