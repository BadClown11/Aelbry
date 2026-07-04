/*
    Modulo 6 (parte 1) - Time Tracking
    Motor: SQL Server
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.TimeEntry', 'U') IS NOT NULL DROP TABLE dbo.TimeEntry;
GO

-- Cada ciclo de cronometro (Start->Stop) o cada registro manual es una fila independiente.
-- "Pausar" el cronometro en el frontend es, a nivel de datos, un Stop; "reanudar" es un nuevo Start.
CREATE TABLE dbo.TimeEntry
(
    TimeEntryId    INT IDENTITY(1,1) NOT NULL,
    ActivityId     INT               NOT NULL,
    UserId         INT               NOT NULL,
    StartTime      DATETIME2         NOT NULL,
    EndTime        DATETIME2         NULL,
    DurationHours  DECIMAL(9,2)      NOT NULL CONSTRAINT DF_TimeEntry_DurationHours DEFAULT (0),
    IsManual       BIT               NOT NULL CONSTRAINT DF_TimeEntry_IsManual DEFAULT (0),
    IsOvertime     BIT               NOT NULL CONSTRAINT DF_TimeEntry_IsOvertime DEFAULT (0),
    Notes          NVARCHAR(500)     NULL,
    CreatedBy      INT               NOT NULL,
    CreatedDate    DATETIME2         NOT NULL CONSTRAINT DF_TimeEntry_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy     INT               NULL,
    ModifiedDate   DATETIME2         NULL,
    IsDeleted      BIT               NOT NULL CONSTRAINT DF_TimeEntry_IsDeleted DEFAULT (0),
    CONSTRAINT PK_TimeEntry PRIMARY KEY CLUSTERED (TimeEntryId),
    CONSTRAINT FK_TimeEntry_Activity FOREIGN KEY (ActivityId) REFERENCES dbo.Activity (ActivityId),
    CONSTRAINT FK_TimeEntry_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId)
);
GO

CREATE NONCLUSTERED INDEX IX_TimeEntry_ActivityId ON dbo.TimeEntry (ActivityId);
CREATE NONCLUSTERED INDEX IX_TimeEntry_UserId ON dbo.TimeEntry (UserId);
GO
