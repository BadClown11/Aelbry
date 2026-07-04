/*
    Modulo 9: Auditoria global.
    Registra un subconjunto representativo de acciones sensibles (CRUD de Usuarios, Roles,
    Permisos, Empresas, Proyectos y cambios de estado de Actividades) con el snapshot JSON
    antes/despues, quien la hizo y desde que IP. No es un log exhaustivo de cada metodo BL.
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.AuditLog', 'U') IS NOT NULL DROP TABLE dbo.AuditLog;
GO

CREATE TABLE dbo.AuditLog
(
    AuditLogId    INT IDENTITY(1,1) NOT NULL,
    CompanyId     INT               NOT NULL,
    UserId        INT               NOT NULL,
    UserName      NVARCHAR(200)     NOT NULL,
    IPAddress     NVARCHAR(64)      NULL,
    Module        NVARCHAR(50)      NOT NULL,
    Action        NVARCHAR(50)      NOT NULL,
    EntityId      INT               NULL,
    DataBefore    NVARCHAR(MAX)     NULL,
    DataAfter     NVARCHAR(MAX)     NULL,
    CreatedDate   DATETIME2         NOT NULL CONSTRAINT DF_AuditLog_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_AuditLog PRIMARY KEY CLUSTERED (AuditLogId),
    CONSTRAINT FK_AuditLog_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId)
);
GO

CREATE NONCLUSTERED INDEX IX_AuditLog_CompanyId_CreatedDate ON dbo.AuditLog (CompanyId, CreatedDate DESC);
CREATE NONCLUSTERED INDEX IX_AuditLog_Module ON dbo.AuditLog (Module);
GO
