using Aelbry.BO.Documents;

namespace Aelbry.DAL
{
    public class FileDAL : MSSqlDatabase
    {
        public static FileDAL Instance
        {
            get { return new FileDAL(); }
        }

        private FileDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<FileFolder> GetFoldersByProject(int projectId)
        {
            var list = new List<FileFolder>();
            const string sSp = "SP_FILE_FOLDER_GET_BY_PROJECT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapFolder(reader));
                    }
                }
            }

            return list;
        }

        public int CreateFolder(int projectId, int? parentFolderId, string name, int createdBy)
        {
            const string sSp = "SP_FILE_FOLDER_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
                cmd.Parameters.Add(CreateParameter("@P_PARENT_FOLDER_ID", (object)parentFolderId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_NAME", name));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", createdBy));

                var pNewId = CreateParameterOut("@P_NEW_FILE_FOLDER_ID", DbType.Int32, 4);
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

        public void DeleteFolder(int fileFolderId)
        {
            const string sSp = "SP_FILE_FOLDER_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_FILE_FOLDER_ID", fileFolderId));

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

        public List<FileAttachment> GetAttachmentsByProject(int projectId, int? fileFolderId)
        {
            var list = new List<FileAttachment>();
            const string sSp = "SP_FILE_ATTACHMENT_GET_BY_PROJECT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
                cmd.Parameters.Add(CreateParameter("@P_FILE_FOLDER_ID", (object)fileFolderId ?? DBNull.Value));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapAttachment(reader));
                    }
                }
            }

            return list;
        }

        public FileAttachment GetAttachmentById(int fileAttachmentId)
        {
            FileAttachment attachment = null;
            const string sSp = "SP_FILE_ATTACHMENT_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_FILE_ATTACHMENT_ID", fileAttachmentId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        attachment = MapAttachmentBasic(reader);
                    }
                }
            }

            return attachment;
        }

        public int CreateAttachment(int projectId, int? fileFolderId, string fileName, int createdBy)
        {
            const string sSp = "SP_FILE_ATTACHMENT_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_PROJECT_ID", projectId));
                cmd.Parameters.Add(CreateParameter("@P_FILE_FOLDER_ID", (object)fileFolderId ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_FILE_NAME", fileName));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", createdBy));

                var pNewId = CreateParameterOut("@P_NEW_FILE_ATTACHMENT_ID", DbType.Int32, 4);
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

        public void DeleteAttachment(int fileAttachmentId)
        {
            const string sSp = "SP_FILE_ATTACHMENT_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_FILE_ATTACHMENT_ID", fileAttachmentId));

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

        public List<FileAttachmentVersion> GetVersionsByAttachment(int fileAttachmentId)
        {
            var list = new List<FileAttachmentVersion>();
            const string sSp = "SP_FILE_ATTACHMENT_VERSION_GET_BY_ATTACHMENT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_FILE_ATTACHMENT_ID", fileAttachmentId));

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

        public FileAttachmentVersion GetLatestVersion(int fileAttachmentId)
        {
            FileAttachmentVersion version = null;
            const string sSp = "SP_FILE_ATTACHMENT_VERSION_GET_LATEST";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_FILE_ATTACHMENT_ID", fileAttachmentId));

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

        public FileAttachmentVersion GetVersionById(int fileAttachmentVersionId)
        {
            FileAttachmentVersion version = null;
            const string sSp = "SP_FILE_ATTACHMENT_VERSION_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_FILE_ATTACHMENT_VERSION_ID", fileAttachmentVersionId));

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

        public int CreateVersion(int fileAttachmentId, string storedFileName, string originalFileName, string contentType, long fileSizeBytes, int uploadedBy)
        {
            const string sSp = "SP_FILE_ATTACHMENT_VERSION_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_FILE_ATTACHMENT_ID", fileAttachmentId));
                cmd.Parameters.Add(CreateParameter("@P_STORED_FILE_NAME", storedFileName));
                cmd.Parameters.Add(CreateParameter("@P_ORIGINAL_FILE_NAME", originalFileName));
                cmd.Parameters.Add(CreateParameter("@P_CONTENT_TYPE", (object)contentType ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("@P_FILE_SIZE_BYTES", fileSizeBytes));
                cmd.Parameters.Add(CreateParameter("@P_UPLOADED_BY", uploadedBy));

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

        private static long GetDefaultLongIfDBNull(object value)
        {
            return (value == null || value == DBNull.Value) ? 0L : Convert.ToInt64(value);
        }

        private FileFolder MapFolder(IDataReader reader)
        {
            return new FileFolder
            {
                FileFolderId = Validate.getDefaultIntIfDBNull(reader["FILE_FOLDER_ID"]),
                ProjectId = Validate.getDefaultIntIfDBNull(reader["PROJECT_ID"]),
                ParentFolderId = Validate.getDefaultNullableIntIfDBNull(reader["PARENT_FOLDER_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
            };
        }

        private FileAttachment MapAttachment(IDataReader reader)
        {
            return new FileAttachment
            {
                FileAttachmentId = Validate.getDefaultIntIfDBNull(reader["FILE_ATTACHMENT_ID"]),
                ProjectId = Validate.getDefaultIntIfDBNull(reader["PROJECT_ID"]),
                FileFolderId = Validate.getDefaultNullableIntIfDBNull(reader["FILE_FOLDER_ID"]),
                FileName = Validate.getDefaultStringIfDBNull(reader["FILE_NAME"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                LatestVersionNumber = Validate.getDefaultIntIfDBNull(reader["LATEST_VERSION_NUMBER"]),
                LatestFileSizeBytes = GetDefaultLongIfDBNull(reader["LATEST_FILE_SIZE_BYTES"]),
            };
        }

        private FileAttachment MapAttachmentBasic(IDataReader reader)
        {
            return new FileAttachment
            {
                FileAttachmentId = Validate.getDefaultIntIfDBNull(reader["FILE_ATTACHMENT_ID"]),
                ProjectId = Validate.getDefaultIntIfDBNull(reader["PROJECT_ID"]),
                FileFolderId = Validate.getDefaultNullableIntIfDBNull(reader["FILE_FOLDER_ID"]),
                FileName = Validate.getDefaultStringIfDBNull(reader["FILE_NAME"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
            };
        }

        private FileAttachmentVersion MapVersion(IDataReader reader)
        {
            return new FileAttachmentVersion
            {
                FileAttachmentVersionId = Validate.getDefaultIntIfDBNull(reader["FILE_ATTACHMENT_VERSION_ID"]),
                FileAttachmentId = Validate.getDefaultIntIfDBNull(reader["FILE_ATTACHMENT_ID"]),
                VersionNumber = Validate.getDefaultIntIfDBNull(reader["VERSION_NUMBER"]),
                StoredFileName = Validate.getDefaultStringIfDBNull(reader["STORED_FILE_NAME"]),
                OriginalFileName = Validate.getDefaultStringIfDBNull(reader["ORIGINAL_FILE_NAME"]),
                ContentType = Validate.getDefaultStringIfDBNull(reader["CONTENT_TYPE"]),
                FileSizeBytes = GetDefaultLongIfDBNull(reader["FILE_SIZE_BYTES"]),
                UploadedBy = Validate.getDefaultIntIfDBNull(reader["UPLOADED_BY"]),
                UploadedDate = Validate.getDefaultDateIfDBNull(reader["UPLOADED_DATE"]),
            };
        }
    }
}
