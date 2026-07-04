/*
    Modulo 6 (parte 3) - Documentos & Archivos con versionado
    Motor: SQL Server
    Documentos: editor Markdown tipo Notion, cada guardado crea una nueva version inmutable.
    Archivos: adjuntos organizados en carpetas, cada re-subida del mismo archivo logico crea
    una nueva version (el binario se guarda en disco, no en la BD).
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.FileAttachmentVersion', 'U') IS NOT NULL DROP TABLE dbo.FileAttachmentVersion;
IF OBJECT_ID('dbo.FileAttachment', 'U') IS NOT NULL DROP TABLE dbo.FileAttachment;
IF OBJECT_ID('dbo.FileFolder', 'U') IS NOT NULL DROP TABLE dbo.FileFolder;
IF OBJECT_ID('dbo.DocumentVersion', 'U') IS NOT NULL DROP TABLE dbo.DocumentVersion;
IF OBJECT_ID('dbo.Document', 'U') IS NOT NULL DROP TABLE dbo.Document;
GO

CREATE TABLE dbo.Document
(
    DocumentId    INT IDENTITY(1,1) NOT NULL,
    ProjectId     INT               NOT NULL,
    Title         NVARCHAR(200)     NOT NULL,
    CreatedBy     INT               NOT NULL,
    CreatedDate   DATETIME2         NOT NULL CONSTRAINT DF_Document_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy    INT               NULL,
    ModifiedDate  DATETIME2         NULL,
    IsDeleted     BIT               NOT NULL CONSTRAINT DF_Document_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Document PRIMARY KEY CLUSTERED (DocumentId),
    CONSTRAINT FK_Document_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Project (ProjectId)
);
GO

-- Cada guardado crea una fila nueva (historial inmutable); "version actual" = MAX(VersionNumber).
CREATE TABLE dbo.DocumentVersion
(
    DocumentVersionId  INT IDENTITY(1,1) NOT NULL,
    DocumentId         INT               NOT NULL,
    VersionNumber      INT               NOT NULL,
    ContentMarkdown    NVARCHAR(MAX)     NOT NULL,
    CreatedBy          INT               NOT NULL,
    CreatedDate        DATETIME2         NOT NULL CONSTRAINT DF_DocumentVersion_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_DocumentVersion PRIMARY KEY CLUSTERED (DocumentVersionId),
    CONSTRAINT FK_DocumentVersion_Document FOREIGN KEY (DocumentId) REFERENCES dbo.Document (DocumentId),
    CONSTRAINT UQ_DocumentVersion_Number UNIQUE (DocumentId, VersionNumber)
);
GO

CREATE TABLE dbo.FileFolder
(
    FileFolderId    INT IDENTITY(1,1) NOT NULL,
    ProjectId       INT               NOT NULL,
    ParentFolderId  INT               NULL,
    Name            NVARCHAR(200)     NOT NULL,
    CreatedBy       INT               NOT NULL,
    CreatedDate     DATETIME2         NOT NULL CONSTRAINT DF_FileFolder_CreatedDate DEFAULT (SYSUTCDATETIME()),
    IsDeleted       BIT               NOT NULL CONSTRAINT DF_FileFolder_IsDeleted DEFAULT (0),
    CONSTRAINT PK_FileFolder PRIMARY KEY CLUSTERED (FileFolderId),
    CONSTRAINT FK_FileFolder_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Project (ProjectId),
    CONSTRAINT FK_FileFolder_Parent FOREIGN KEY (ParentFolderId) REFERENCES dbo.FileFolder (FileFolderId)
);
GO

-- Representa el archivo "logico" (su nombre, carpeta); el binario y los metadatos de cada
-- subida viven en FileAttachmentVersion.
CREATE TABLE dbo.FileAttachment
(
    FileAttachmentId  INT IDENTITY(1,1) NOT NULL,
    ProjectId         INT               NOT NULL,
    FileFolderId      INT               NULL,
    FileName          NVARCHAR(260)     NOT NULL,
    CreatedBy         INT               NOT NULL,
    CreatedDate       DATETIME2         NOT NULL CONSTRAINT DF_FileAttachment_CreatedDate DEFAULT (SYSUTCDATETIME()),
    IsDeleted         BIT               NOT NULL CONSTRAINT DF_FileAttachment_IsDeleted DEFAULT (0),
    CONSTRAINT PK_FileAttachment PRIMARY KEY CLUSTERED (FileAttachmentId),
    CONSTRAINT FK_FileAttachment_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Project (ProjectId),
    CONSTRAINT FK_FileAttachment_Folder FOREIGN KEY (FileFolderId) REFERENCES dbo.FileFolder (FileFolderId)
);
GO

-- StoredFileName es el nombre fisico en disco (GUID + extension), para evitar colisiones y
-- path traversal con nombres de archivo arbitrarios que suba el usuario.
CREATE TABLE dbo.FileAttachmentVersion
(
    FileAttachmentVersionId  INT IDENTITY(1,1) NOT NULL,
    FileAttachmentId         INT               NOT NULL,
    VersionNumber            INT               NOT NULL,
    StoredFileName           NVARCHAR(300)     NOT NULL,
    OriginalFileName         NVARCHAR(260)     NOT NULL,
    ContentType              NVARCHAR(150)     NULL,
    FileSizeBytes            BIGINT            NOT NULL,
    UploadedBy               INT               NOT NULL,
    UploadedDate             DATETIME2         NOT NULL CONSTRAINT DF_FileAttachmentVersion_UploadedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_FileAttachmentVersion PRIMARY KEY CLUSTERED (FileAttachmentVersionId),
    CONSTRAINT FK_FileAttachmentVersion_Attachment FOREIGN KEY (FileAttachmentId) REFERENCES dbo.FileAttachment (FileAttachmentId),
    CONSTRAINT UQ_FileAttachmentVersion_Number UNIQUE (FileAttachmentId, VersionNumber)
);
GO

CREATE NONCLUSTERED INDEX IX_Document_ProjectId ON dbo.Document (ProjectId);
CREATE NONCLUSTERED INDEX IX_DocumentVersion_DocumentId ON dbo.DocumentVersion (DocumentId);
CREATE NONCLUSTERED INDEX IX_FileFolder_ProjectId ON dbo.FileFolder (ProjectId);
CREATE NONCLUSTERED INDEX IX_FileAttachment_ProjectId ON dbo.FileAttachment (ProjectId);
CREATE NONCLUSTERED INDEX IX_FileAttachment_FileFolderId ON dbo.FileAttachment (FileFolderId);
CREATE NONCLUSTERED INDEX IX_FileAttachmentVersion_FileAttachmentId ON dbo.FileAttachmentVersion (FileAttachmentId);
GO
