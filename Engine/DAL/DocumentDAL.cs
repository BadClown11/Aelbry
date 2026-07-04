using Aelbry.BO.Documents;

namespace Aelbry.DAL
{
    public class DocumentDAL : MSSqlDatabase
    {
        public static DocumentDAL Instance
        {
            get { return new DocumentDAL(); }
        }

        private DocumentDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<Document> GetByProject(int projectId)
        {
            var list = new List<Document>();
            const string sSp = "SP_DOCUMENT_GET_BY_PROJECT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapDocument(reader));
                    }
                }
            }

            return list;
        }

        public Document GetById(int documentId)
        {
            Document document = null;
            const string sSp = "SP_DOCUMENT_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DOCUMENT_ID", documentId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        document = MapDocument(reader);
                    }
                }
            }

            return document;
        }

        public int Create(int projectId, string title, string contentMarkdown, int createdBy)
        {
            const string sSp = "SP_DOCUMENT_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
                cmd.Parameters.Add(CreateParameter("@P_TITLE", title));
                cmd.Parameters.Add(CreateParameter("@P_CONTENT_MARKDOWN", contentMarkdown));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", createdBy));

                var pNewId = CreateParameterOut("@P_NEW_DOCUMENT_ID", DbType.Int32, 4);
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

        public void UpdateTitle(int documentId, string title, int modifiedBy)
        {
            const string sSp = "SP_DOCUMENT_UPDATE_TITLE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DOCUMENT_ID", documentId));
                cmd.Parameters.Add(CreateParameter("@P_TITLE", title));
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

        public void Delete(int documentId, int modifiedBy)
        {
            const string sSp = "SP_DOCUMENT_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DOCUMENT_ID", documentId));
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

        public List<DocumentVersion> GetVersionsByDocument(int documentId)
        {
            var list = new List<DocumentVersion>();
            const string sSp = "SP_DOCUMENT_VERSION_GET_BY_DOCUMENT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DOCUMENT_ID", documentId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapVersion(reader));
                    }
                }
            }

            return list;
        }

        public DocumentVersion GetLatestVersion(int documentId)
        {
            DocumentVersion version = null;
            const string sSp = "SP_DOCUMENT_VERSION_GET_LATEST";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DOCUMENT_ID", documentId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        version = MapVersion(reader);
                    }
                }
            }

            return version;
        }

        public int CreateVersion(int documentId, string contentMarkdown, int createdBy)
        {
            const string sSp = "SP_DOCUMENT_VERSION_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DOCUMENT_ID", documentId));
                cmd.Parameters.Add(CreateParameter("@P_CONTENT_MARKDOWN", contentMarkdown));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", createdBy));

                var pNewVersion = CreateParameterOut("@P_NEW_VERSION_NUMBER", DbType.Int32, 4);
                cmd.Parameters.Add(pNewVersion);

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }

                return Validate.getDefaultIntIfDBNull(pNewVersion.Value);
            }
        }

        private Document MapDocument(IDataReader reader)
        {
            return new Document
            {
                DocumentId = Validate.getDefaultIntIfDBNull(reader["DOCUMENT_ID"]),
                ProjectId = Validate.getDefaultIntIfDBNull(reader["PROJECT_ID"]),
                Title = Validate.getDefaultStringIfDBNull(reader["TITLE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
                LatestVersionNumber = Validate.getDefaultIntIfDBNull(reader["LATEST_VERSION_NUMBER"]),
            };
        }

        private DocumentVersion MapVersion(IDataReader reader)
        {
            return new DocumentVersion
            {
                DocumentVersionId = Validate.getDefaultIntIfDBNull(reader["DOCUMENT_VERSION_ID"]),
                DocumentId = Validate.getDefaultIntIfDBNull(reader["DOCUMENT_ID"]),
                VersionNumber = Validate.getDefaultIntIfDBNull(reader["VERSION_NUMBER"]),
                ContentMarkdown = Validate.getDefaultStringIfDBNull(reader["CONTENT_MARKDOWN"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
            };
        }
    }
}
