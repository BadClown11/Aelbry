/*
    Modulo 7 (parte 2) - Stored Procedures de Notificaciones + actualizacion de los SP de
    AutomationRule para incluir el tercer tipo de accion (SendNotification).
*/

USE AelbryDb;
GO

--------------------------------------------------------------------------------------------
-- NOTIFICATION
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_NOTIFICATION_GET_BY_ID
    @P_NOTIFICATION_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NotificationId AS NOTIFICATION_ID, UserId AS USER_ID, Title AS TITLE, Message AS MESSAGE,
           Link AS LINK, IsRead AS IS_READ, CreatedDate AS CREATED_DATE
    FROM dbo.Notification
    WHERE NotificationId = @P_NOTIFICATION_ID;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_NOTIFICATION_GET_BY_USER
    @P_USER_ID INT,
    @P_UNREAD_ONLY BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (50) NotificationId AS NOTIFICATION_ID, UserId AS USER_ID, Title AS TITLE, Message AS MESSAGE,
           Link AS LINK, IsRead AS IS_READ, CreatedDate AS CREATED_DATE
    FROM dbo.Notification
    WHERE UserId = @P_USER_ID AND (@P_UNREAD_ONLY = 0 OR IsRead = 0)
    ORDER BY CreatedDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_NOTIFICATION_GET_UNREAD_COUNT
    @P_USER_ID INT,
    @P_COUNT INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @P_COUNT = COUNT(*) FROM dbo.Notification WHERE UserId = @P_USER_ID AND IsRead = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_NOTIFICATION_INSERT
    @P_USER_ID INT,
    @P_TITLE NVARCHAR(200),
    @P_MESSAGE NVARCHAR(500),
    @P_LINK NVARCHAR(500) = NULL,
    @P_NEW_NOTIFICATION_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Notification (UserId, Title, Message, Link)
        VALUES (@P_USER_ID, @P_TITLE, @P_MESSAGE, @P_LINK);

        SET @P_NEW_NOTIFICATION_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_NOTIFICATION_MARK_AS_READ
    @P_NOTIFICATION_ID INT,
    @P_USER_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Notification
        SET IsRead = 1
        WHERE NotificationId = @P_NOTIFICATION_ID AND UserId = @P_USER_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_NOTIFICATION_MARK_ALL_AS_READ
    @P_USER_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Notification SET IsRead = 1 WHERE UserId = @P_USER_ID AND IsRead = 0;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- ACTIVITY DUE REMINDER (recordatorios de vencimiento para el BackgroundService)
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_DUE_REMINDER_GET_CANDIDATES
    @P_DAYS_AHEAD INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.ActivityId AS ACTIVITY_ID, a.Code AS CODE, a.Name AS NAME, a.EstimatedEndDate AS ESTIMATED_END_DATE,
           a.ResponsibleUserId AS RESPONSIBLE_USER_ID, u.Email AS RESPONSIBLE_EMAIL,
           u.FirstName + ' ' + u.LastName AS RESPONSIBLE_NAME, a.ProjectId AS PROJECT_ID
    FROM dbo.Activity a
    JOIN dbo.[User] u ON u.UserId = a.ResponsibleUserId
    WHERE a.IsDeleted = 0 AND a.Status NOT IN (4, 5) -- Completada, Cancelada
      AND a.EstimatedEndDate IS NOT NULL
      AND a.EstimatedEndDate BETWEEN CAST(SYSUTCDATETIME() AS DATE) AND DATEADD(DAY, @P_DAYS_AHEAD, CAST(SYSUTCDATETIME() AS DATE))
      AND NOT EXISTS (SELECT 1 FROM dbo.ActivityDueReminder r WHERE r.ActivityId = a.ActivityId);
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_DUE_REMINDER_MARK_SENT
    @P_ACTIVITY_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.ActivityDueReminder WHERE ActivityId = @P_ACTIVITY_ID)
        BEGIN
            INSERT INTO dbo.ActivityDueReminder (ActivityId) VALUES (@P_ACTIVITY_ID);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- AUTOMATION RULE: se redefinen para incluir el tercer tipo de accion (SendNotification).
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_GET_ACTIVE_BY_ACTIVITY_TRIGGER
    @P_TRIGGER_ACTIVITY_ID INT,
    @P_TRIGGER_TYPE TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AutomationRuleId AS AUTOMATION_RULE_ID, CompanyId AS COMPANY_ID, Name AS NAME,
           TriggerType AS TRIGGER_TYPE, TriggerActivityId AS TRIGGER_ACTIVITY_ID, NULL AS TRIGGER_ACTIVITY_NAME,
           TriggerProjectId AS TRIGGER_PROJECT_ID, NULL AS TRIGGER_PROJECT_NAME,
           TriggerThresholdPercent AS TRIGGER_THRESHOLD_PERCENT, TriggerStatus AS TRIGGER_STATUS,
           ActionType AS ACTION_TYPE, ActionTargetActivityId AS ACTION_TARGET_ACTIVITY_ID, NULL AS ACTION_TARGET_ACTIVITY_NAME,
           ActionTargetProjectId AS ACTION_TARGET_PROJECT_ID, NULL AS ACTION_TARGET_PROJECT_NAME,
           ActionNewActivityStatus AS ACTION_NEW_ACTIVITY_STATUS, ActionNewProjectStatusId AS ACTION_NEW_PROJECT_STATUS_ID,
           ActionNotificationMessage AS ACTION_NOTIFICATION_MESSAGE, ActionNotificationUserId AS ACTION_NOTIFICATION_USER_ID,
           NULL AS ACTION_NOTIFICATION_USER_NAME,
           IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE,
           ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.AutomationRule
    WHERE TriggerActivityId = @P_TRIGGER_ACTIVITY_ID AND TriggerType = @P_TRIGGER_TYPE
      AND IsActive = 1 AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_GET_ACTIVE_BY_PROJECT_TRIGGER
    @P_TRIGGER_PROJECT_ID INT,
    @P_TRIGGER_TYPE TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AutomationRuleId AS AUTOMATION_RULE_ID, CompanyId AS COMPANY_ID, Name AS NAME,
           TriggerType AS TRIGGER_TYPE, TriggerActivityId AS TRIGGER_ACTIVITY_ID, NULL AS TRIGGER_ACTIVITY_NAME,
           TriggerProjectId AS TRIGGER_PROJECT_ID, NULL AS TRIGGER_PROJECT_NAME,
           TriggerThresholdPercent AS TRIGGER_THRESHOLD_PERCENT, TriggerStatus AS TRIGGER_STATUS,
           ActionType AS ACTION_TYPE, ActionTargetActivityId AS ACTION_TARGET_ACTIVITY_ID, NULL AS ACTION_TARGET_ACTIVITY_NAME,
           ActionTargetProjectId AS ACTION_TARGET_PROJECT_ID, NULL AS ACTION_TARGET_PROJECT_NAME,
           ActionNewActivityStatus AS ACTION_NEW_ACTIVITY_STATUS, ActionNewProjectStatusId AS ACTION_NEW_PROJECT_STATUS_ID,
           ActionNotificationMessage AS ACTION_NOTIFICATION_MESSAGE, ActionNotificationUserId AS ACTION_NOTIFICATION_USER_ID,
           NULL AS ACTION_NOTIFICATION_USER_NAME,
           IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE,
           ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.AutomationRule
    WHERE TriggerProjectId = @P_TRIGGER_PROJECT_ID AND TriggerType = @P_TRIGGER_TYPE
      AND IsActive = 1 AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_GET_BY_COMPANY
    @P_COMPANY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.AutomationRuleId AS AUTOMATION_RULE_ID, r.CompanyId AS COMPANY_ID, r.Name AS NAME,
           r.TriggerType AS TRIGGER_TYPE, r.TriggerActivityId AS TRIGGER_ACTIVITY_ID, ta.Name AS TRIGGER_ACTIVITY_NAME,
           r.TriggerProjectId AS TRIGGER_PROJECT_ID, tp.Name AS TRIGGER_PROJECT_NAME,
           r.TriggerThresholdPercent AS TRIGGER_THRESHOLD_PERCENT, r.TriggerStatus AS TRIGGER_STATUS,
           r.ActionType AS ACTION_TYPE, r.ActionTargetActivityId AS ACTION_TARGET_ACTIVITY_ID, aa.Name AS ACTION_TARGET_ACTIVITY_NAME,
           r.ActionTargetProjectId AS ACTION_TARGET_PROJECT_ID, ap.Name AS ACTION_TARGET_PROJECT_NAME,
           r.ActionNewActivityStatus AS ACTION_NEW_ACTIVITY_STATUS, r.ActionNewProjectStatusId AS ACTION_NEW_PROJECT_STATUS_ID,
           r.ActionNotificationMessage AS ACTION_NOTIFICATION_MESSAGE, r.ActionNotificationUserId AS ACTION_NOTIFICATION_USER_ID,
           anu.FirstName + ' ' + anu.LastName AS ACTION_NOTIFICATION_USER_NAME,
           r.IsActive AS IS_ACTIVE, r.CreatedBy AS CREATED_BY, r.CreatedDate AS CREATED_DATE,
           r.ModifiedBy AS MODIFIED_BY, r.ModifiedDate AS MODIFIED_DATE
    FROM dbo.AutomationRule r
    LEFT JOIN dbo.Activity ta ON ta.ActivityId = r.TriggerActivityId
    LEFT JOIN dbo.Project tp ON tp.ProjectId = r.TriggerProjectId
    LEFT JOIN dbo.Activity aa ON aa.ActivityId = r.ActionTargetActivityId
    LEFT JOIN dbo.Project ap ON ap.ProjectId = r.ActionTargetProjectId
    LEFT JOIN dbo.[User] anu ON anu.UserId = r.ActionNotificationUserId
    WHERE r.CompanyId = @P_COMPANY_ID AND r.IsDeleted = 0
    ORDER BY r.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_GET_BY_ID
    @P_AUTOMATION_RULE_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.AutomationRuleId AS AUTOMATION_RULE_ID, r.CompanyId AS COMPANY_ID, r.Name AS NAME,
           r.TriggerType AS TRIGGER_TYPE, r.TriggerActivityId AS TRIGGER_ACTIVITY_ID, ta.Name AS TRIGGER_ACTIVITY_NAME,
           r.TriggerProjectId AS TRIGGER_PROJECT_ID, tp.Name AS TRIGGER_PROJECT_NAME,
           r.TriggerThresholdPercent AS TRIGGER_THRESHOLD_PERCENT, r.TriggerStatus AS TRIGGER_STATUS,
           r.ActionType AS ACTION_TYPE, r.ActionTargetActivityId AS ACTION_TARGET_ACTIVITY_ID, aa.Name AS ACTION_TARGET_ACTIVITY_NAME,
           r.ActionTargetProjectId AS ACTION_TARGET_PROJECT_ID, ap.Name AS ACTION_TARGET_PROJECT_NAME,
           r.ActionNewActivityStatus AS ACTION_NEW_ACTIVITY_STATUS, r.ActionNewProjectStatusId AS ACTION_NEW_PROJECT_STATUS_ID,
           r.ActionNotificationMessage AS ACTION_NOTIFICATION_MESSAGE, r.ActionNotificationUserId AS ACTION_NOTIFICATION_USER_ID,
           anu.FirstName + ' ' + anu.LastName AS ACTION_NOTIFICATION_USER_NAME,
           r.IsActive AS IS_ACTIVE, r.CreatedBy AS CREATED_BY, r.CreatedDate AS CREATED_DATE,
           r.ModifiedBy AS MODIFIED_BY, r.ModifiedDate AS MODIFIED_DATE
    FROM dbo.AutomationRule r
    LEFT JOIN dbo.Activity ta ON ta.ActivityId = r.TriggerActivityId
    LEFT JOIN dbo.Project tp ON tp.ProjectId = r.TriggerProjectId
    LEFT JOIN dbo.Activity aa ON aa.ActivityId = r.ActionTargetActivityId
    LEFT JOIN dbo.Project ap ON ap.ProjectId = r.ActionTargetProjectId
    LEFT JOIN dbo.[User] anu ON anu.UserId = r.ActionNotificationUserId
    WHERE r.AutomationRuleId = @P_AUTOMATION_RULE_ID AND r.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_INSERT
    @P_COMPANY_ID INT,
    @P_NAME NVARCHAR(200),
    @P_TRIGGER_TYPE TINYINT,
    @P_TRIGGER_ACTIVITY_ID INT = NULL,
    @P_TRIGGER_PROJECT_ID INT = NULL,
    @P_TRIGGER_THRESHOLD_PERCENT DECIMAL(5,2) = NULL,
    @P_TRIGGER_STATUS TINYINT = NULL,
    @P_ACTION_TYPE TINYINT,
    @P_ACTION_TARGET_ACTIVITY_ID INT = NULL,
    @P_ACTION_TARGET_PROJECT_ID INT = NULL,
    @P_ACTION_NEW_ACTIVITY_STATUS TINYINT = NULL,
    @P_ACTION_NEW_PROJECT_STATUS_ID INT = NULL,
    @P_ACTION_NOTIFICATION_MESSAGE NVARCHAR(500) = NULL,
    @P_ACTION_NOTIFICATION_USER_ID INT = NULL,
    @P_CREATED_BY INT,
    @P_NEW_AUTOMATION_RULE_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.AutomationRule
            (CompanyId, Name, TriggerType, TriggerActivityId, TriggerProjectId, TriggerThresholdPercent, TriggerStatus,
             ActionType, ActionTargetActivityId, ActionTargetProjectId, ActionNewActivityStatus, ActionNewProjectStatusId,
             ActionNotificationMessage, ActionNotificationUserId, CreatedBy)
        VALUES
            (@P_COMPANY_ID, @P_NAME, @P_TRIGGER_TYPE, @P_TRIGGER_ACTIVITY_ID, @P_TRIGGER_PROJECT_ID, @P_TRIGGER_THRESHOLD_PERCENT, @P_TRIGGER_STATUS,
             @P_ACTION_TYPE, @P_ACTION_TARGET_ACTIVITY_ID, @P_ACTION_TARGET_PROJECT_ID, @P_ACTION_NEW_ACTIVITY_STATUS, @P_ACTION_NEW_PROJECT_STATUS_ID,
             @P_ACTION_NOTIFICATION_MESSAGE, @P_ACTION_NOTIFICATION_USER_ID, @P_CREATED_BY);

        SET @P_NEW_AUTOMATION_RULE_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_UPDATE
    @P_AUTOMATION_RULE_ID INT,
    @P_NAME NVARCHAR(200),
    @P_TRIGGER_TYPE TINYINT,
    @P_TRIGGER_ACTIVITY_ID INT = NULL,
    @P_TRIGGER_PROJECT_ID INT = NULL,
    @P_TRIGGER_THRESHOLD_PERCENT DECIMAL(5,2) = NULL,
    @P_TRIGGER_STATUS TINYINT = NULL,
    @P_ACTION_TYPE TINYINT,
    @P_ACTION_TARGET_ACTIVITY_ID INT = NULL,
    @P_ACTION_TARGET_PROJECT_ID INT = NULL,
    @P_ACTION_NEW_ACTIVITY_STATUS TINYINT = NULL,
    @P_ACTION_NEW_PROJECT_STATUS_ID INT = NULL,
    @P_ACTION_NOTIFICATION_MESSAGE NVARCHAR(500) = NULL,
    @P_ACTION_NOTIFICATION_USER_ID INT = NULL,
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.AutomationRule
        SET Name = @P_NAME,
            TriggerType = @P_TRIGGER_TYPE,
            TriggerActivityId = @P_TRIGGER_ACTIVITY_ID,
            TriggerProjectId = @P_TRIGGER_PROJECT_ID,
            TriggerThresholdPercent = @P_TRIGGER_THRESHOLD_PERCENT,
            TriggerStatus = @P_TRIGGER_STATUS,
            ActionType = @P_ACTION_TYPE,
            ActionTargetActivityId = @P_ACTION_TARGET_ACTIVITY_ID,
            ActionTargetProjectId = @P_ACTION_TARGET_PROJECT_ID,
            ActionNewActivityStatus = @P_ACTION_NEW_ACTIVITY_STATUS,
            ActionNewProjectStatusId = @P_ACTION_NEW_PROJECT_STATUS_ID,
            ActionNotificationMessage = @P_ACTION_NOTIFICATION_MESSAGE,
            ActionNotificationUserId = @P_ACTION_NOTIFICATION_USER_ID,
            IsActive = @P_IS_ACTIVE,
            ModifiedBy = @P_MODIFIED_BY,
            ModifiedDate = SYSUTCDATETIME()
        WHERE AutomationRuleId = @P_AUTOMATION_RULE_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
