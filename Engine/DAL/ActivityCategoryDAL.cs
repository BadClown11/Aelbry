using Aelbry.BO;

namespace Aelbry.DAL
{
    public class ActivityCategoryDAL : MSSqlDatabase
    {
        public static ActivityCategoryDAL Instance
        {
            get { return new ActivityCategoryDAL(); }
        }

        private ActivityCategoryDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<ActivityCategory> GetByCompany(int companyId)
        {
            var list = new List<ActivityCategory>();
            const string sSp = "SP_ACTIVITY_CATEGORY_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapActivityCategory(reader));
                    }
                }
            }

            return list;
        }

        public int Create(ActivityCategory category)
        {
            const string sSp = "SP_ACTIVITY_CATEGORY_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", category.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", category.Name));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", category.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_ACTIVITY_CATEGORY_ID", DbType.Int32, 4);
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

        private ActivityCategory MapActivityCategory(IDataReader reader)
        {
            return new ActivityCategory
            {
                ActivityCategoryId = Validate.getDefaultIntIfDBNull(reader["ACTIVITY_CATEGORY_ID"]),
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
