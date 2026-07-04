/*
    Modulo 6 (parte 3) - Stored Procedures de Documentos & Archivos.
*/

USE AelbryDb;
GO

--------------------------------------------------------------------------------------------
-- DOCUMENT
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_DOCUMENT_GET_BY_PROJECT
    @P_PROJECT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.DocumentId AS DOCUMENT_ID, d.ProjectId AS PROJECT_ID, d.Title AS TITLE,
           d.CreatedBy AS CREATED_BY, d.CreatedDate AS CREATED_DATE, d.ModifiedBy AS MODIFIED_BY, d.ModifiedDate AS MODIFIED_DATE,
           (SELECT MAX(VersionNumber) FROM dbo.DocumentVersion WHERE DocumentId = d.DocumentId) AS LATEST_VERSION_NUMBER
    FROM dbo.Document d
    WHERE d.ProjectId = @P_PROJECT_ID AND d.IsDeleted = 0
    ORDER BY d.Title;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DOCUMENT_GET_BY_ID
    @P_DOCUMENT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.DocumentId AS DOCUMENT_ID, d.ProjectId AS PROJECT_ID, d.Title AS TITLE,
           d.CreatedBy AS CREATED_BY, d.CreatedDate AS CREATED_DATE, d.ModifiedBy AS MODIFIED_BY, d.ModifiedDate AS MODIFIED_DATE,
           (SELECT MAX(VersionNumber) FROM dbo.DocumentVersion WHERE DocumentId = d.DocumentId) AS LATEST_VERSION_NUMBER
    FROM dbo.Document d
    WHERE d.DocumentId = @P_DOCUMENT_ID AND d.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DOCUMENT_INSERT
    @P_PROJECT_ID INT,
    @P_TITLE NVARCHAR(200),
    @P_CONTENT_MARKDOWN NVARCHAR(MAX),
    @P_CREATED_BY INT,
    @P_NEW_DOCUMENT_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Document (ProjectId, Title, CreatedBy)
        VALUES (@P_PROJECT_ID, @P_TITLE, @P_CREATED_BY);

        SET @P_NEW_DOCUMENT_ID = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.DocumentVersion (DocumentId, VersionNumber, ContentMarkdown, CreatedBy)
        VALUES (@P_NEW_DOCUMENT_ID, 1, @P_CONTENT_MARKDOWN, @P_CREATED_BY);

        COMMIT TRANSACTION;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DOCUMENT_UPDATE_TITLE
    @P_DOCUMENT_ID INT,
    @P_TITLE NVARCHAR(200),
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Document
        SET Title = @P_TITLE, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE DocumentId = @P_DOCUMENT_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DOCUMENT_DELETE
    @P_DOCUMENT_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Document
        SET IsDeleted = 1, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE DocumentId = @P_DOCUMENT_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- DOCUMENT VERSION
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_DOCUMENT_VERSION_GET_BY_DOCUMENT
    @P_DOCUMENT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DocumentVersionId AS DOCUMENT_VERSION_ID, DocumentId AS DOCUMENT_ID, VersionNumber AS VERSION_NUMBER,
           ContentMarkdown AS CONTENT_MARKDOWN, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE
    FROM dbo.DocumentVersion
    WHERE DocumentId = @P_DOCUMENT_ID
    ORDER BY VersionNumber DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DOCUMENT_VERSION_GET_LATEST
    @P_DOCUMENT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 DocumentVersionId AS DOCUMENT_VERSION_ID, DocumentId AS DOCUMENT_ID, VersionNumber AS VERSION_NUMBER,
           ContentMarkdown AS CONTENT_MARKDOWN, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE
    FROM dbo.DocumentVersion
    WHERE DocumentId = @P_DOCUMENT_ID
    ORDER BY VersionNumber DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DOCUMENT_VERSION_INSERT
    @P_DOCUMENT_ID INT,
    @P_CONTENT_MARKDOWN NVARCHAR(MAX),
    @P_CREATED_BY INT,
    @P_NEW_VERSION_NUMBER INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT @P_NEW_VERSION_NUMBER = ISNULL(MAX(VersionNumber), 0) + 1 FROM dbo.DocumentVersion WHERE DocumentId = @P_DOCUMENT_ID;

        INSERT INTO dbo.DocumentVersion (DocumentId, VersionNumber, ContentMarkdown, CreatedBy)
        VALUES (@P_DOCUMENT_ID, @P_NEW_VERSION_NUMBER, @P_CONTENT_MARKDOWN, @P_CREATED_BY);

        UPDATE dbo.Document SET ModifiedBy = @P_CREATED_BY, ModifiedDate = SYSUTCDATETIME() WHERE DocumentId = @P_DOCUMENT_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- FILE FOLDER
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_FILE_FOLDER_GET_BY_PROJECT
    @P_PROJECT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileFolderId AS FILE_FOLDER_ID, ProjectId AS PROJECT_ID, ParentFolderId AS PARENT_FOLDER_ID,
           Name AS NAME, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE
    FROM dbo.FileFolder
    WHERE ProjectId = @P_PROJECT_ID AND IsDeleted = 0
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_FILE_FOLDER_INSERT
    @P_PROJECT_ID INT,
    @P_PARENT_FOLDER_ID INT = NULL,
    @P_NAME NVARCHAR(200),
    @P_CREATED_BY INT,
    @P_NEW_FILE_FOLDER_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.FileFolder (ProjectId, ParentFolderId, Name, CreatedBy)
        VALUES (@P_PROJECT_ID, @P_PARENT_FOLDER_ID, @P_NAME, @P_CREATED_BY);

        SET @P_NEW_FILE_FOLDER_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_FILE_FOLDER_DELETE
    @P_FILE_FOLDER_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.FileFolder WHERE ParentFolderId = @P_FILE_FOLDER_ID AND IsDeleted = 0)
        BEGIN
            RAISERROR('No es posible eliminar una carpeta que tiene subcarpetas.', 16, 1);
        END

        IF EXISTS (SELECT 1 FROM dbo.FileAttachment WHERE FileFolderId = @P_FILE_FOLDER_ID AND IsDeleted = 0)
        BEGIN
            RAISERROR('No es posible eliminar una carpeta que tiene archivos.', 16, 1);
        END

        UPDATE dbo.FileFolder SET IsDeleted = 1 WHERE FileFolderId = @P_FILE_FOLDER_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- FILE ATTACHMENT
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_FILE_ATTACHMENT_GET_BY_PROJECT
    @P_PROJECT_ID INT,
    @P_FILE_FOLDER_ID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.FileAttachmentId AS FILE_ATTACHMENT_ID, a.ProjectId AS PROJECT_ID, a.FileFolderId AS FILE_FOLDER_ID,
           a.FileName AS FILE_NAME, a.CreatedBy AS CREATED_BY, a.CreatedDate AS CREATED_DATE,
           (SELECT MAX(VersionNumber) FROM dbo.FileAttachmentVersion WHERE FileAttachmentId = a.FileAttachmentId) AS LATEST_VERSION_NUMBER,
           (SELECT TOP 1 FileSizeBytes FROM dbo.FileAttachmentVersion WHERE FileAttachmentId = a.FileAttachmentId ORDER BY VersionNumber DESC) AS LATEST_FILE_SIZE_BYTES
    FROM dbo.FileAttachment a
    WHERE a.ProjectId = @P_PROJECT_ID AND a.IsDeleted = 0
      AND ((@P_FILE_FOLDER_ID IS NULL AND a.FileFolderId IS NULL) OR a.FileFolderId = @P_FILE_FOLDER_ID)
    ORDER BY a.FileName;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_FILE_ATTACHMENT_GET_BY_ID
    @P_FILE_ATTACHMENT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileAttachmentId AS FILE_ATTACHMENT_ID, ProjectId AS PROJECT_ID, FileFolderId AS FILE_FOLDER_ID,
           FileName AS FILE_NAME, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE
    FROM dbo.FileAttachment
    WHERE FileAttachmentId = @P_FILE_ATTACHMENT_ID AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_FILE_ATTACHMENT_INSERT
    @P_PROJECT_ID INT,
    @P_FILE_FOLDER_ID INT = NULL,
    @P_FILE_NAME NVARCHAR(260),
    @P_CREATED_BY INT,
    @P_NEW_FILE_ATTACHMENT_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.FileAttachment (ProjectId, FileFolderId, FileName, CreatedBy)
        VALUES (@P_PROJECT_ID, @P_FILE_FOLDER_ID, @P_FILE_NAME, @P_CREATED_BY);

        SET @P_NEW_FILE_ATTACHMENT_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_FILE_ATTACHMENT_DELETE
    @P_FILE_ATTACHMENT_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.FileAttachment SET IsDeleted = 1 WHERE FileAttachmentId = @P_FILE_ATTACHMENT_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- FILE ATTACHMENT VERSION
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_FILE_ATTACHMENT_VERSION_GET_BY_ATTACHMENT
    @P_FILE_ATTACHMENT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileAttachmentVersionId AS FILE_ATTACHMENT_VERSION_ID, FileAttachmentId AS FILE_ATTACHMENT_ID,
           VersionNumber AS VERSION_NUMBER, StoredFileName AS STORED_FILE_NAME, OriginalFileName AS ORIGINAL_FILE_NAME,
           ContentType AS CONTENT_TYPE, FileSizeBytes AS FILE_SIZE_BYTES, UploadedBy AS UPLOADED_BY, UploadedDate AS UPLOADED_DATE
    FROM dbo.FileAttachmentVersion
    WHERE FileAttachmentId = @P_FILE_ATTACHMENT_ID
    ORDER BY VersionNumber DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_FILE_ATTACHMENT_VERSION_GET_LATEST
    @P_FILE_ATTACHMENT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 FileAttachmentVersionId AS FILE_ATTACHMENT_VERSION_ID, FileAttachmentId AS FILE_ATTACHMENT_ID,
           VersionNumber AS VERSION_NUMBER, StoredFileName AS STORED_FILE_NAME, OriginalFileName AS ORIGINAL_FILE_NAME,
           ContentType AS CONTENT_TYPE, FileSizeBytes AS FILE_SIZE_BYTES, UploadedBy AS UPLOADED_BY, UploadedDate AS UPLOADED_DATE
    FROM dbo.FileAttachmentVersion
    WHERE FileAttachmentId = @P_FILE_ATTACHMENT_ID
    ORDER BY VersionNumber DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_FILE_ATTACHMENT_VERSION_GET_BY_ID
    @P_FILE_ATTACHMENT_VERSION_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FileAttachmentVersionId AS FILE_ATTACHMENT_VERSION_ID, FileAttachmentId AS FILE_ATTACHMENT_ID,
           VersionNumber AS VERSION_NUMBER, StoredFileName AS STORED_FILE_NAME, OriginalFileName AS ORIGINAL_FILE_NAME,
           ContentType AS CONTENT_TYPE, FileSizeBytes AS FILE_SIZE_BYTES, UploadedBy AS UPLOADED_BY, UploadedDate AS UPLOADED_DATE
    FROM dbo.FileAttachmentVersion
    WHERE FileAttachmentVersionId = @P_FILE_ATTACHMENT_VERSION_ID;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_FILE_ATTACHMENT_VERSION_INSERT
    @P_FILE_ATTACHMENT_ID INT,
    @P_STORED_FILE_NAME NVARCHAR(300),
    @P_ORIGINAL_FILE_NAME NVARCHAR(260),
    @P_CONTENT_TYPE NVARCHAR(150) = NULL,
    @P_FILE_SIZE_BYTES BIGINT,
    @P_UPLOADED_BY INT,
    @P_NEW_VERSION_NUMBER INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT @P_NEW_VERSION_NUMBER = ISNULL(MAX(VersionNumber), 0) + 1 FROM dbo.FileAttachmentVersion WHERE FileAttachmentId = @P_FILE_ATTACHMENT_ID;

        INSERT INTO dbo.FileAttachmentVersion (FileAttachmentId, VersionNumber, StoredFileName, OriginalFileName, ContentType, FileSizeBytes, UploadedBy)
        VALUES (@P_FILE_ATTACHMENT_ID, @P_NEW_VERSION_NUMBER, @P_STORED_FILE_NAME, @P_ORIGINAL_FILE_NAME, @P_CONTENT_TYPE, @P_FILE_SIZE_BYTES, @P_UPLOADED_BY);

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
