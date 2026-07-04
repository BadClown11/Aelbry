using Aelbry.BO;

namespace Aelbry.DAL
{
    public class PermissionDAL : MSSqlDatabase
    {
        public static PermissionDAL Instance
        {
            get { return new PermissionDAL(); }
        }

        private PermissionDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<Permission> GetAll()
        {
            var list = new List<Permission>();
            const string sSp = "SP_PERMISSION_GET_ALL";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
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
    }
}
