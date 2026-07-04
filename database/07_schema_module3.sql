/*
    Modulo 3 - Motor de Actividades & Algoritmo de Avance Ponderado
    Motor: SQL Server
    Contenido: tablas, llaves foraneas, restricciones unicas e indices.
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.ActivityDependency', 'U') IS NOT NULL DROP TABLE dbo.ActivityDependency;
IF OBJECT_ID('dbo.ChecklistItem', 'U') IS NOT NULL DROP TABLE dbo.ChecklistItem;
IF OBJECT_ID('dbo.ActivityTag', 'U') IS NOT NULL DROP TABLE dbo.ActivityTag;
IF OBJECT_ID('dbo.ActivityParticipant', 'U') IS NOT NULL DROP TABLE dbo.ActivityParticipant;
IF OBJECT_ID('dbo.Activity', 'U') IS NOT NULL DROP TABLE dbo.Activity;
GO

-- Actividades WBS: estructura recursiva via ParentActivityId (subactividades sin limite de profundidad).
CREATE TABLE dbo.Activity
(
    ActivityId           INT IDENTITY(1,1) NOT NULL,
    ProjectId            INT               NOT NULL,
    ParentActivityId     INT               NULL,
    Code                 NVARCHAR(40)      NOT NULL,
    Name                 NVARCHAR(200)     NOT NULL,
    Description          NVARCHAR(MAX)     NULL,
    Category             NVARCHAR(100)     NULL,
    ColorHex             CHAR(7)           NOT NULL CONSTRAINT DF_Activity_ColorHex DEFAULT ('#4C6EF5'),
    Status               TINYINT           NOT NULL CONSTRAINT DF_Activity_Status DEFAULT (1), -- 1 Pendiente,2 EnProgreso,3 Bloqueada,4 Completada,5 Cancelada
    Priority             TINYINT           NOT NULL CONSTRAINT DF_Activity_Priority DEFAULT (2), -- 1 Baja,2 Media,3 Alta,4 Critica
    ResponsibleUserId    INT               NOT NULL,
    EstimatedStartDate   DATE              NULL,
    EstimatedEndDate     DATE              NULL,
    ActualStartDate      DATE              NULL,
    ActualEndDate        DATE              NULL,
    Weight               DECIMAL(9,2)      NOT NULL CONSTRAINT DF_Activity_Weight DEFAULT (1),
    EstimatedHours       DECIMAL(9,2)      NOT NULL CONSTRAINT DF_Activity_EstimatedHours DEFAULT (0),
    WorkedHours          DECIMAL(9,2)      NOT NULL CONSTRAINT DF_Activity_WorkedHours DEFAULT (0),
    ProgressPercentage   DECIMAL(5,2)      NOT NULL CONSTRAINT DF_Activity_ProgressPercentage DEFAULT (0),
    Sequence             INT               NOT NULL CONSTRAINT DF_Activity_Sequence DEFAULT (0),
    IsActive             BIT               NOT NULL CONSTRAINT DF_Activity_IsActive DEFAULT (1),
    CreatedBy            INT               NOT NULL,
    CreatedDate          DATETIME2         NOT NULL CONSTRAINT DF_Activity_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy           INT               NULL,
    ModifiedDate         DATETIME2         NULL,
    IsDeleted            BIT               NOT NULL CONSTRAINT DF_Activity_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Activity PRIMARY KEY CLUSTERED (ActivityId),
    CONSTRAINT FK_Activity_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Project (ProjectId),
    CONSTRAINT FK_Activity_Parent FOREIGN KEY (ParentActivityId) REFERENCES dbo.Activity (ActivityId),
    CONSTRAINT FK_Activity_Responsible FOREIGN KEY (ResponsibleUserId) REFERENCES dbo.[User] (UserId),
    CONSTRAINT UQ_Activity_Project_Code UNIQUE (ProjectId, Code),
    CONSTRAINT CK_Activity_Status CHECK (Status BETWEEN 1 AND 5),
    CONSTRAINT CK_Activity_Priority CHECK (Priority BETWEEN 1 AND 4)
);
GO

-- Participantes (colaboradores) de la actividad; el responsable principal vive en Activity.ResponsibleUserId.
CREATE TABLE dbo.ActivityParticipant
(
    ActivityId  INT       NOT NULL,
    UserId      INT       NOT NULL,
    AddedDate   DATETIME2 NOT NULL CONSTRAINT DF_ActivityParticipant_AddedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ActivityParticipant PRIMARY KEY CLUSTERED (ActivityId, UserId),
    CONSTRAINT FK_ActivityParticipant_Activity FOREIGN KEY (ActivityId) REFERENCES dbo.Activity (ActivityId),
    CONSTRAINT FK_ActivityParticipant_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId)
);
GO

-- Reutiliza el catalogo de etiquetas del Modulo 2 (dbo.Tag), ya scoped por empresa.
CREATE TABLE dbo.ActivityTag
(
    ActivityId  INT NOT NULL,
    TagId       INT NOT NULL,
    CONSTRAINT PK_ActivityTag PRIMARY KEY CLUSTERED (ActivityId, TagId),
    CONSTRAINT FK_ActivityTag_Activity FOREIGN KEY (ActivityId) REFERENCES dbo.Activity (ActivityId),
    CONSTRAINT FK_ActivityTag_Tag FOREIGN KEY (TagId) REFERENCES dbo.Tag (TagId)
);
GO

-- Checklist independiente por actividad/subactividad (no requiere borrado logico: es informacion desechable).
CREATE TABLE dbo.ChecklistItem
(
    ChecklistItemId  INT IDENTITY(1,1) NOT NULL,
    ActivityId       INT               NOT NULL,
    Text             NVARCHAR(300)     NOT NULL,
    IsChecked        BIT               NOT NULL CONSTRAINT DF_ChecklistItem_IsChecked DEFAULT (0),
    Sequence         INT               NOT NULL CONSTRAINT DF_ChecklistItem_Sequence DEFAULT (0),
    CreatedBy        INT               NOT NULL,
    CreatedDate       DATETIME2        NOT NULL CONSTRAINT DF_ChecklistItem_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CompletedBy      INT               NULL,
    CompletedDate    DATETIME2         NULL,
    CONSTRAINT PK_ChecklistItem PRIMARY KEY CLUSTERED (ChecklistItemId),
    CONSTRAINT FK_ChecklistItem_Activity FOREIGN KEY (ActivityId) REFERENCES dbo.Activity (ActivityId)
);
GO

-- Dependencias entre actividades (bloqueos). ActivityId es la actividad dependiente/bloqueada.
CREATE TABLE dbo.ActivityDependency
(
    ActivityDependencyId  INT IDENTITY(1,1) NOT NULL,
    ActivityId            INT               NOT NULL,
    DependsOnActivityId   INT               NOT NULL,
    DependencyType        TINYINT           NOT NULL CONSTRAINT DF_ActivityDependency_Type DEFAULT (1), -- 1 FS,2 SS,3 FF,4 SF
    CreatedBy             INT               NOT NULL,
    CreatedDate           DATETIME2         NOT NULL CONSTRAINT DF_ActivityDependency_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ActivityDependency PRIMARY KEY CLUSTERED (ActivityDependencyId),
    CONSTRAINT FK_ActivityDependency_Activity FOREIGN KEY (ActivityId) REFERENCES dbo.Activity (ActivityId),
    CONSTRAINT FK_ActivityDependency_DependsOn FOREIGN KEY (DependsOnActivityId) REFERENCES dbo.Activity (ActivityId),
    CONSTRAINT UQ_ActivityDependency UNIQUE (ActivityId, DependsOnActivityId),
    CONSTRAINT CK_ActivityDependency_NoSelfDependency CHECK (ActivityId <> DependsOnActivityId),
    CONSTRAINT CK_ActivityDependency_Type CHECK (DependencyType BETWEEN 1 AND 4)
);
GO

CREATE NONCLUSTERED INDEX IX_Activity_ProjectId ON dbo.Activity (ProjectId);
CREATE NONCLUSTERED INDEX IX_Activity_ParentActivityId ON dbo.Activity (ParentActivityId);
CREATE NONCLUSTERED INDEX IX_Activity_ResponsibleUserId ON dbo.Activity (ResponsibleUserId);
CREATE NONCLUSTERED INDEX IX_ActivityParticipant_UserId ON dbo.ActivityParticipant (UserId);
CREATE NONCLUSTERED INDEX IX_ActivityTag_TagId ON dbo.ActivityTag (TagId);
CREATE NONCLUSTERED INDEX IX_ChecklistItem_ActivityId ON dbo.ChecklistItem (ActivityId);
CREATE NONCLUSTERED INDEX IX_ActivityDependency_DependsOnActivityId ON dbo.ActivityDependency (DependsOnActivityId);
GO
