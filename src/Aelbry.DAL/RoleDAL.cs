using Aelbry.BO;

namespace Aelbry.DAL
{
    public class RoleDAL : MSSqlDatabase
    {
        public static RoleDAL Instance
        {
            get { return new RoleDAL(); }
        }

        private RoleDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<Role> GetAll()
        {
            var list = new List<Role>();
            const string sSp = "SP_ROLE_GET_ALL";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapRole(reader));
                    }
                }
            }

            return list;
        }

        public Role GetById(int roleId)
        {
            Role role = null;
            const string sSp = "SP_ROLE_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ROLE_ID", roleId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        role = MapRole(reader);
                    }
                }
            }

            if (role != null)
            {
                role.Permissions = GetPermissions(roleId);
            }

            return role;
        }

        public int Create(Role role)
        {
            const string sSp = "SP_ROLE_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_NAME", role.Name));
                cmd.Parameters.Add(CreateParameter("@P_DESCRIPTION", (object)role.Description ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", role.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_ROLE_ID", DbType.Int32, 4);
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

        public void Update(Role role)
        {
            const string sSp = "SP_ROLE_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ROLE_ID", role.RoleId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", role.Name));
                cmd.Parameters.Add(CreateParameter("@P_DESCRIPTION", (object)role.Description ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", role.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)role.ModifiedBy ?? DBNull.Value));

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

        public void Delete(int roleId, int modifiedBy)
        {
            const string sSp = "SP_ROLE_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ROLE_ID", roleId));
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

        public List<Permission> GetPermissions(int roleId)
        {
            var list = new List<Permission>();
            const string sSp = "SP_ROLE_GET_PERMISSIONS";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ROLE_ID", roleId));

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

        public void AssignPermission(int roleId, int permissionId)
        {
            const string sSp = "SP_ROLE_ASSIGN_PERMISSION";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ROLE_ID", roleId));
                cmd.Parameters.Add(CreateParameter("@P_PERMISSION_ID", permissionId));

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

        public void RemovePermission(int roleId, int permissionId)
        {
            const string sSp = "SP_ROLE_REMOVE_PERMISSION";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_ROLE_ID", roleId));
                cmd.Parameters.Add(CreateParameter("@P_PERMISSION_ID", permissionId));

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

        private Role MapRole(IDataReader reader)
        {
            return new Role
            {
                RoleId = Validate.getDefaultIntIfDBNull(reader["ROLE_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                Description = Validate.getDefaultStringIfDBNull(reader["DESCRIPTION"]),
                IsSystemDefault = Validate.getDefaultBoolIfDBNull(reader["IS_SYSTEM_DEFAULT"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
