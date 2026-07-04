/*
    Modulo 7 (parte 2) - Sistema de Notificaciones
    Motor: SQL Server
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.ActivityDueReminder', 'U') IS NOT NULL DROP TABLE dbo.ActivityDueReminder;
IF OBJECT_ID('dbo.Notification', 'U') IS NOT NULL DROP TABLE dbo.Notification;
GO

CREATE TABLE dbo.Notification
(
    NotificationId  INT IDENTITY(1,1) NOT NULL,
    UserId          INT               NOT NULL,
    Title           NVARCHAR(200)     NOT NULL,
    Message         NVARCHAR(500)     NOT NULL,
    Link            NVARCHAR(500)     NULL,
    IsRead          BIT               NOT NULL CONSTRAINT DF_Notification_IsRead DEFAULT (0),
    CreatedDate     DATETIME2         NOT NULL CONSTRAINT DF_Notification_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Notification PRIMARY KEY CLUSTERED (NotificationId),
    CONSTRAINT FK_Notification_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId)
);
GO

-- Marca las actividades que ya generaron su recordatorio de vencimiento, para que el
-- BackgroundService no vuelva a notificar cada vez que corre (ver DueDateReminderService).
CREATE TABLE dbo.ActivityDueReminder
(
    ActivityId  INT       NOT NULL,
    SentDate    DATETIME2 NOT NULL CONSTRAINT DF_ActivityDueReminder_SentDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ActivityDueReminder PRIMARY KEY CLUSTERED (ActivityId),
    CONSTRAINT FK_ActivityDueReminder_Activity FOREIGN KEY (ActivityId) REFERENCES dbo.Activity (ActivityId)
);
GO

CREATE NONCLUSTERED INDEX IX_Notification_UserId ON dbo.Notification (UserId);
GO

--------------------------------------------------------------------------------------------
-- Extension a AutomationRule (Modulo 7 parte 1): tercer tipo de accion "SendNotification",
-- deliberadamente diferido hasta tener el Sistema de Notificaciones construido.
--------------------------------------------------------------------------------------------
ALTER TABLE dbo.AutomationRule ADD ActionNotificationMessage NVARCHAR(500) NULL;
GO
ALTER TABLE dbo.AutomationRule ADD ActionNotificationUserId INT NULL;
GO
ALTER TABLE dbo.AutomationRule ADD CONSTRAINT FK_AutomationRule_NotificationUser FOREIGN KEY (ActionNotificationUserId) REFERENCES dbo.[User] (UserId);
GO
ALTER TABLE dbo.AutomationRule DROP CONSTRAINT CK_AutomationRule_ActionType;
GO
ALTER TABLE dbo.AutomationRule ADD CONSTRAINT CK_AutomationRule_ActionType CHECK (ActionType BETWEEN 1 AND 3);
GO
