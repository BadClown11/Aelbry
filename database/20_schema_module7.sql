/*
    Modulo 7 (parte 1) - Motor de Reglas (Triggers & Actions)
    Motor: SQL Server
    Ejemplos que cubre: "Si Actividad llega al 100% -> Cambiar estado a Finalizada",
    "Si termina la Fase 1 -> Desbloquear Fase 2" (via ActivityStatusChanged + ChangeActivityStatus).
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.AutomationRuleLog', 'U') IS NOT NULL DROP TABLE dbo.AutomationRuleLog;
IF OBJECT_ID('dbo.AutomationRule', 'U') IS NOT NULL DROP TABLE dbo.AutomationRule;
GO

-- TriggerType: 1 ActivityProgressThreshold, 2 ActivityStatusChanged, 3 ProjectProgressThreshold
-- ActionType:  1 ChangeActivityStatus,      2 ChangeProjectStatus
-- Las columnas de disparador/accion son nullable a proposito: cada combinacion de tipo usa
-- solo el subconjunto que le corresponde (ver CK_AutomationRule_TriggerConfig/ActionConfig).
CREATE TABLE dbo.AutomationRule
(
    AutomationRuleId          INT IDENTITY(1,1) NOT NULL,
    CompanyId                 INT               NOT NULL,
    Name                      NVARCHAR(200)     NOT NULL,
    TriggerType                TINYINT          NOT NULL,
    TriggerActivityId         INT               NULL,
    TriggerProjectId          INT               NULL,
    TriggerThresholdPercent   DECIMAL(5,2)      NULL,
    TriggerStatus             TINYINT           NULL,
    ActionType                TINYINT           NOT NULL,
    ActionTargetActivityId    INT               NULL,
    ActionTargetProjectId     INT               NULL,
    ActionNewActivityStatus   TINYINT           NULL,
    ActionNewProjectStatusId INT               NULL,
    IsActive                  BIT               NOT NULL CONSTRAINT DF_AutomationRule_IsActive DEFAULT (1),
    CreatedBy                 INT               NOT NULL,
    CreatedDate                DATETIME2        NOT NULL CONSTRAINT DF_AutomationRule_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy                 INT              NULL,
    ModifiedDate               DATETIME2        NULL,
    IsDeleted                  BIT              NOT NULL CONSTRAINT DF_AutomationRule_IsDeleted DEFAULT (0),
    CONSTRAINT PK_AutomationRule PRIMARY KEY CLUSTERED (AutomationRuleId),
    CONSTRAINT FK_AutomationRule_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT FK_AutomationRule_TriggerActivity FOREIGN KEY (TriggerActivityId) REFERENCES dbo.Activity (ActivityId),
    CONSTRAINT FK_AutomationRule_TriggerProject FOREIGN KEY (TriggerProjectId) REFERENCES dbo.Project (ProjectId),
    CONSTRAINT FK_AutomationRule_ActionTargetActivity FOREIGN KEY (ActionTargetActivityId) REFERENCES dbo.Activity (ActivityId),
    CONSTRAINT FK_AutomationRule_ActionTargetProject FOREIGN KEY (ActionTargetProjectId) REFERENCES dbo.Project (ProjectId),
    CONSTRAINT FK_AutomationRule_ActionNewProjectStatus FOREIGN KEY (ActionNewProjectStatusId) REFERENCES dbo.ProjectStatus (ProjectStatusId),
    CONSTRAINT CK_AutomationRule_TriggerType CHECK (TriggerType BETWEEN 1 AND 3),
    CONSTRAINT CK_AutomationRule_ActionType CHECK (ActionType BETWEEN 1 AND 2)
);
GO

CREATE TABLE dbo.AutomationRuleLog
(
    AutomationRuleLogId  INT IDENTITY(1,1) NOT NULL,
    AutomationRuleId     INT               NOT NULL,
    TriggeredDate        DATETIME2         NOT NULL CONSTRAINT DF_AutomationRuleLog_TriggeredDate DEFAULT (SYSUTCDATETIME()),
    Success              BIT               NOT NULL,
    Details              NVARCHAR(500)     NULL,
    CONSTRAINT PK_AutomationRuleLog PRIMARY KEY CLUSTERED (AutomationRuleLogId),
    CONSTRAINT FK_AutomationRuleLog_Rule FOREIGN KEY (AutomationRuleId) REFERENCES dbo.AutomationRule (AutomationRuleId)
);
GO

CREATE NONCLUSTERED INDEX IX_AutomationRule_CompanyId ON dbo.AutomationRule (CompanyId);
CREATE NONCLUSTERED INDEX IX_AutomationRule_TriggerActivityId ON dbo.AutomationRule (TriggerActivityId);
CREATE NONCLUSTERED INDEX IX_AutomationRule_TriggerProjectId ON dbo.AutomationRule (TriggerProjectId);
CREATE NONCLUSTERED INDEX IX_AutomationRuleLog_AutomationRuleId ON dbo.AutomationRuleLog (AutomationRuleId);
GO
