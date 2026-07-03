using Aelbry.BO;

namespace Aelbry.DAL
{
    public class CompanyDAL : MSSqlDatabase
    {
        // "Instance" entrega una conexion nueva por cada llamada (no un singleton compartido):
        // el BL la consume dentro de un using(...) que hace Dispose() y cierra la conexion al terminar.
        public static CompanyDAL Instance
        {
            get { return new CompanyDAL(); }
        }

        private CompanyDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<Company> GetAll()
        {
            var list = new List<Company>();
            const string sSp = "SP_COMPANY_GET_ALL";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapCompany(reader));
                    }
                }
            }

            return list;
        }

        public Company GetById(int companyId)
        {
            Company company = null;
            const string sSp = "SP_COMPANY_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        company = MapCompany(reader);
                    }
                }
            }

            return company;
        }

        public int Create(Company company)
        {
            const string sSp = "SP_COMPANY_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_NAME", company.Name));
                cmd.Parameters.Add(CreateParameter("@P_LEGAL_TAX_ID", (object)company.LegalTaxId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_LOGO_URL", (object)company.LogoUrl ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TIME_ZONE", company.TimeZone));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", company.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_COMPANY_ID", DbType.Int32, 4);
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

        public void Update(Company company)
        {
            const string sSp = "SP_COMPANY_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", company.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", company.Name));
                cmd.Parameters.Add(CreateParameter("@P_LEGAL_TAX_ID", (object)company.LegalTaxId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_LOGO_URL", (object)company.LogoUrl ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_TIME_ZONE", company.TimeZone));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", company.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)company.ModifiedBy ?? DBNull.Value));

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

        public void Delete(int companyId, int modifiedBy)
        {
            const string sSp = "SP_COMPANY_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));
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

        private Company MapCompany(IDataReader reader)
        {
            return new Company
            {
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                LegalTaxId = Validate.getDefaultStringIfDBNull(reader["LEGAL_TAX_ID"]),
                LogoUrl = Validate.getDefaultStringIfDBNull(reader["LOGO_URL"]),
                TimeZone = Validate.getDefaultStringIfDBNull(reader["TIME_ZONE"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
