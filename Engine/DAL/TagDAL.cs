using Aelbry.BO;

namespace Aelbry.DAL
{
    public class TagDAL : MSSqlDatabase
    {
        public static TagDAL Instance
        {
            get { return new TagDAL(); }
        }

        private TagDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<Tag> GetByCompany(int companyId)
        {
            var list = new List<Tag>();
            const string sSp = "SP_TAG_GET_BY_COMPANY";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", companyId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapTag(reader));
                    }
                }
            }

            return list;
        }

        public int Create(Tag tag)
        {
            const string sSp = "SP_TAG_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_COMPANY_ID", tag.CompanyId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", tag.Name));
                cmd.Parameters.Add(CreateParameter("@P_COLOR_HEX", tag.ColorHex));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", tag.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_TAG_ID", DbType.Int32, 4);
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

        public void Update(Tag tag)
        {
            const string sSp = "SP_TAG_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TAG_ID", tag.TagId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", tag.Name));
                cmd.Parameters.Add(CreateParameter("@P_COLOR_HEX", tag.ColorHex));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", tag.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)tag.ModifiedBy ?? DBNull.Value));

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

        public void Delete(int tagId, int modifiedBy)
        {
            const string sSp = "SP_TAG_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TAG_ID", tagId));
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

        private Tag MapTag(IDataReader reader)
        {
            return new Tag
            {
                TagId = Validate.getDefaultIntIfDBNull(reader["TAG_ID"]),
                CompanyId = Validate.getDefaultIntIfDBNull(reader["COMPANY_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                ColorHex = Validate.getDefaultStringIfDBNull(reader["COLOR_HEX"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
