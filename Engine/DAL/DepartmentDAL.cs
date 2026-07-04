using Aelbry.BO;

namespace Aelbry.DAL
{
    public class DepartmentDAL : MSSqlDatabase
    {
        public static DepartmentDAL Instance
        {
            get { return new DepartmentDAL(); }
        }

        private DepartmentDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<Department> GetByCompany(int companyId)
        {
            var list = new List<Department>();
            const string sSp = "SP_DEPARTMENT_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapDepartment(reader));
                    }
                }
            }

            return list;
        }

        public Department GetById(int departmentId)
        {
            Department department = null;
            const string sSp = "SP_DEPARTMENT_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DEPARTMENT_ID", departmentId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        department = MapDepartment(reader);
                    }
                }
            }

            return department;
        }

        public int Create(Department department)
        {
            const string sSp = "SP_DEPARTMENT_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", department.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", department.Name));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", department.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_DEPARTMENT_ID", DbType.Int32, 4);
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

        public void Update(Department department)
        {
            const string sSp = "SP_DEPARTMENT_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DEPARTMENT_ID", department.DepartmentId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", department.Name));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", department.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)department.ModifiedBy ?? DBNull.Value));

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

        public void Delete(int departmentId, int modifiedBy)
        {
            const string sSp = "SP_DEPARTMENT_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DEPARTMENT_ID", departmentId));
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

        private Department MapDepartment(IDataReader reader)
        {
            return new Department
            {
                DepartmentId = Validate.getDefaultIntIfDBNull(reader["DEPARTMENT_ID"]),
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
