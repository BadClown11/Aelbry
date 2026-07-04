/*
    Modulo 2 - Core de Proyectos & Catalogos Base
    Motor: SQL Server
    Contenido: tablas, llaves foraneas, restricciones unicas e indices.
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.ProjectTemplate', 'U') IS NOT NULL DROP TABLE dbo.ProjectTemplate;
IF OBJECT_ID('dbo.ProjectTag', 'U') IS NOT NULL DROP TABLE dbo.ProjectTag;
IF OBJECT_ID('dbo.ProjectMember', 'U') IS NOT NULL DROP TABLE dbo.ProjectMember;
IF OBJECT_ID('dbo.Project', 'U') IS NOT NULL DROP TABLE dbo.Project;
IF OBJECT_ID('dbo.Tag', 'U') IS NOT NULL DROP TABLE dbo.Tag;
IF OBJECT_ID('dbo.ProjectStatus', 'U') IS NOT NULL DROP TABLE dbo.ProjectStatus;
GO

-- Catalogo de estados de proyecto, configurable por empresa (define tambien el orden del flujo de trabajo).
CREATE TABLE dbo.ProjectStatus
(
    ProjectStatusId  INT IDENTITY(1,1) NOT NULL,
    CompanyId        INT               NOT NULL,
    Name             NVARCHAR(100)     NOT NULL,
    ColorHex         CHAR(7)           NOT NULL CONSTRAINT DF_ProjectStatus_ColorHex DEFAULT ('#6C757D'),
    Sequence         INT               NOT NULL CONSTRAINT DF_ProjectStatus_Sequence DEFAULT (0),
    IsFinal          BIT               NOT NULL CONSTRAINT DF_ProjectStatus_IsFinal DEFAULT (0),
    IsActive         BIT               NOT NULL CONSTRAINT DF_ProjectStatus_IsActive DEFAULT (1),
    CreatedBy        INT               NOT NULL,
    CreatedDate      DATETIME2         NOT NULL CONSTRAINT DF_ProjectStatus_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy       INT               NULL,
    ModifiedDate     DATETIME2         NULL,
    IsDeleted        BIT               NOT NULL CONSTRAINT DF_ProjectStatus_IsDeleted DEFAULT (0),
    CONSTRAINT PK_ProjectStatus PRIMARY KEY CLUSTERED (ProjectStatusId),
    CONSTRAINT FK_ProjectStatus_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT UQ_ProjectStatus_Company_Name UNIQUE (CompanyId, Name)
);
GO

-- Catalogo de etiquetas reutilizable (proyectos y, mas adelante, actividades del Modulo 3).
CREATE TABLE dbo.Tag
(
    TagId         INT IDENTITY(1,1) NOT NULL,
    CompanyId     INT               NOT NULL,
    Name          NVARCHAR(100)     NOT NULL,
    ColorHex      CHAR(7)           NOT NULL CONSTRAINT DF_Tag_ColorHex DEFAULT ('#868E96'),
    IsActive      BIT               NOT NULL CONSTRAINT DF_Tag_IsActive DEFAULT (1),
    CreatedBy     INT               NOT NULL,
    CreatedDate   DATETIME2         NOT NULL CONSTRAINT DF_Tag_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy    INT               NULL,
    ModifiedDate  DATETIME2         NULL,
    IsDeleted     BIT               NOT NULL CONSTRAINT DF_Tag_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Tag PRIMARY KEY CLUSTERED (TagId),
    CONSTRAINT FK_Tag_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT UQ_Tag_Company_Name UNIQUE (CompanyId, Name)
);
GO

CREATE TABLE dbo.Project
(
    ProjectId           INT IDENTITY(1,1) NOT NULL,
    CompanyId           INT               NOT NULL,
    Code                NVARCHAR(30)      NOT NULL,
    Name                NVARCHAR(200)     NOT NULL,
    ColorHex            CHAR(7)           NOT NULL CONSTRAINT DF_Project_ColorHex DEFAULT ('#4C6EF5'),
    CoverImageUrl       NVARCHAR(500)     NULL,
    ClientName          NVARCHAR(200)     NULL,
    ProjectStatusId     INT               NOT NULL,
    Priority            TINYINT           NOT NULL CONSTRAINT DF_Project_Priority DEFAULT (2), -- 1 Baja, 2 Media, 3 Alta, 4 Critica
    RiskLevel           TINYINT           NOT NULL CONSTRAINT DF_Project_RiskLevel DEFAULT (1), -- 1 Bajo, 2 Medio, 3 Alto (recalculado en Modulo 3)
    StartDate           DATE              NULL,
    EndDate             DATE              NULL,
    Budget              DECIMAL(18,2)     NULL,
    EstimatedHours       DECIMAL(9,2)      NOT NULL CONSTRAINT DF_Project_EstimatedHours DEFAULT (0),
    WorkedHours         DECIMAL(9,2)      NOT NULL CONSTRAINT DF_Project_WorkedHours DEFAULT (0),
    ProgressPercentage  DECIMAL(5,2)      NOT NULL CONSTRAINT DF_Project_ProgressPercentage DEFAULT (0),
    ProjectManagerId    INT               NOT NULL,
    IsActive            BIT               NOT NULL CONSTRAINT DF_Project_IsActive DEFAULT (1),
    CreatedBy           INT               NOT NULL,
    CreatedDate         DATETIME2         NOT NULL CONSTRAINT DF_Project_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy          INT               NULL,
    ModifiedDate        DATETIME2         NULL,
    IsDeleted           BIT               NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Project PRIMARY KEY CLUSTERED (ProjectId),
    CONSTRAINT FK_Project_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT FK_Project_ProjectStatus FOREIGN KEY (ProjectStatusId) REFERENCES dbo.ProjectStatus (ProjectStatusId),
    CONSTRAINT FK_Project_ProjectManager FOREIGN KEY (ProjectManagerId) REFERENCES dbo.[User] (UserId),
    CONSTRAINT UQ_Project_Company_Code UNIQUE (CompanyId, Code),
    CONSTRAINT CK_Project_Priority CHECK (Priority BETWEEN 1 AND 4),
    CONSTRAINT CK_Project_RiskLevel CHECK (RiskLevel BETWEEN 1 AND 3)
);
GO

-- Miembros del equipo asignados al proyecto (el Project Manager/Responsable vive en Project.ProjectManagerId).
CREATE TABLE dbo.ProjectMember
(
    ProjectId   INT       NOT NULL,
    UserId      INT       NOT NULL,
    AddedDate   DATETIME2 NOT NULL CONSTRAINT DF_ProjectMember_AddedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ProjectMember PRIMARY KEY CLUSTERED (ProjectId, UserId),
    CONSTRAINT FK_ProjectMember_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Project (ProjectId),
    CONSTRAINT FK_ProjectMember_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId)
);
GO

CREATE TABLE dbo.ProjectTag
(
    ProjectId  INT NOT NULL,
    TagId      INT NOT NULL,
    CONSTRAINT PK_ProjectTag PRIMARY KEY CLUSTERED (ProjectId, TagId),
    CONSTRAINT FK_ProjectTag_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Project (ProjectId),
    CONSTRAINT FK_ProjectTag_Tag FOREIGN KEY (TagId) REFERENCES dbo.Tag (TagId)
);
GO

-- Plantillas base reutilizables (la logica de "aplicar" plantilla llega con el Modulo 4).
CREATE TABLE dbo.ProjectTemplate
(
    ProjectTemplateId      INT IDENTITY(1,1) NOT NULL,
    CompanyId              INT               NOT NULL,
    Name                   NVARCHAR(200)     NOT NULL,
    Description            NVARCHAR(500)     NULL,
    DefaultPriority        TINYINT           NOT NULL CONSTRAINT DF_ProjectTemplate_DefaultPriority DEFAULT (2),
    DefaultEstimatedHours  DECIMAL(9,2)      NOT NULL CONSTRAINT DF_ProjectTemplate_DefaultEstimatedHours DEFAULT (0),
    IsActive               BIT               NOT NULL CONSTRAINT DF_ProjectTemplate_IsActive DEFAULT (1),
    CreatedBy              INT               NOT NULL,
    CreatedDate            DATETIME2         NOT NULL CONSTRAINT DF_ProjectTemplate_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy             INT               NULL,
    ModifiedDate           DATETIME2         NULL,
    IsDeleted              BIT               NOT NULL CONSTRAINT DF_ProjectTemplate_IsDeleted DEFAULT (0),
    CONSTRAINT PK_ProjectTemplate PRIMARY KEY CLUSTERED (ProjectTemplateId),
    CONSTRAINT FK_ProjectTemplate_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT UQ_ProjectTemplate_Company_Name UNIQUE (CompanyId, Name)
);
GO

CREATE NONCLUSTERED INDEX IX_ProjectStatus_CompanyId ON dbo.ProjectStatus (CompanyId);
CREATE NONCLUSTERED INDEX IX_Tag_CompanyId ON dbo.Tag (CompanyId);
CREATE NONCLUSTERED INDEX IX_Project_CompanyId ON dbo.Project (CompanyId);
CREATE NONCLUSTERED INDEX IX_Project_ProjectStatusId ON dbo.Project (ProjectStatusId);
CREATE NONCLUSTERED INDEX IX_Project_ProjectManagerId ON dbo.Project (ProjectManagerId);
CREATE NONCLUSTERED INDEX IX_ProjectMember_UserId ON dbo.ProjectMember (UserId);
CREATE NONCLUSTERED INDEX IX_ProjectTag_TagId ON dbo.ProjectTag (TagId);
CREATE NONCLUSTERED INDEX IX_ProjectTemplate_CompanyId ON dbo.ProjectTemplate (CompanyId);
GO
