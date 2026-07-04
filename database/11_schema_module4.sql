/*
    Modulo 4 (parte 2) - Esqueleto de actividades por plantilla de proyecto.
    Lista plana (sin jerarquia) de actividades iniciales que se clonan como Activity
    reales al aplicar la plantilla (ProjectBL.CreateFromTemplate).
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.ProjectTemplateActivity', 'U') IS NOT NULL DROP TABLE dbo.ProjectTemplateActivity;
GO

CREATE TABLE dbo.ProjectTemplateActivity
(
    ProjectTemplateActivityId  INT IDENTITY(1,1) NOT NULL,
    ProjectTemplateId          INT               NOT NULL,
    Name                       NVARCHAR(200)     NOT NULL,
    Description                NVARCHAR(500)     NULL,
    EstimatedHours             DECIMAL(9,2)      NOT NULL CONSTRAINT DF_ProjectTemplateActivity_EstimatedHours DEFAULT (0),
    Sequence                   INT               NOT NULL CONSTRAINT DF_ProjectTemplateActivity_Sequence DEFAULT (0),
    CreatedBy                  INT               NOT NULL,
    CreatedDate                DATETIME2         NOT NULL CONSTRAINT DF_ProjectTemplateActivity_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ProjectTemplateActivity PRIMARY KEY CLUSTERED (ProjectTemplateActivityId),
    CONSTRAINT FK_ProjectTemplateActivity_Template FOREIGN KEY (ProjectTemplateId) REFERENCES dbo.ProjectTemplate (ProjectTemplateId)
);
GO

CREATE NONCLUSTERED INDEX IX_ProjectTemplateActivity_ProjectTemplateId ON dbo.ProjectTemplateActivity (ProjectTemplateId);
GO
