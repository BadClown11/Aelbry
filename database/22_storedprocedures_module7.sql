/*
    Modulo 7 (parte 1) - Stored Procedures del Motor de Reglas.
*/

USE AelbryDb;
GO

--------------------------------------------------------------------------------------------
-- AUTOMATION RULE
--------------------------------------------------------------------------------------------
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
           r.IsActive AS IS_ACTIVE, r.CreatedBy AS CREATED_BY, r.CreatedDate AS CREATED_DATE,
           r.ModifiedBy AS MODIFIED_BY, r.ModifiedDate AS MODIFIED_DATE
    FROM dbo.AutomationRule r
    LEFT JOIN dbo.Activity ta ON ta.ActivityId = r.TriggerActivityId
    LEFT JOIN dbo.Project tp ON tp.ProjectId = r.TriggerProjectId
    LEFT JOIN dbo.Activity aa ON aa.ActivityId = r.ActionTargetActivityId
    LEFT JOIN dbo.Project ap ON ap.ProjectId = r.ActionTargetProjectId
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
           r.IsActive AS IS_ACTIVE, r.CreatedBy AS CREATED_BY, r.CreatedDate AS CREATED_DATE,
           r.ModifiedBy AS MODIFIED_BY, r.ModifiedDate AS MODIFIED_DATE
    FROM dbo.AutomationRule r
    LEFT JOIN dbo.Activity ta ON ta.ActivityId = r.TriggerActivityId
    LEFT JOIN dbo.Project tp ON tp.ProjectId = r.TriggerProjectId
    LEFT JOIN dbo.Activity aa ON aa.ActivityId = r.ActionTargetActivityId
    LEFT JOIN dbo.Project ap ON ap.ProjectId = r.ActionTargetProjectId
    WHERE r.AutomationRuleId = @P_AUTOMATION_RULE_ID AND r.IsDeleted = 0;
END
GO

-- Reglas activas de un tipo de disparador ligadas a una actividad especifica (usado por el motor
-- en tiempo de ejecucion; el filtro fino de umbral/estado se hace en C# sobre el resultado).
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
           IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE,
           ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.AutomationRule
    WHERE TriggerProjectId = @P_TRIGGER_PROJECT_ID AND TriggerType = @P_TRIGGER_TYPE
      AND IsActive = 1 AND IsDeleted = 0;
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
    @P_CREATED_BY INT,
    @P_NEW_AUTOMATION_RULE_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.AutomationRule
            (CompanyId, Name, TriggerType, TriggerActivityId, TriggerProjectId, TriggerThresholdPercent, TriggerStatus,
             ActionType, ActionTargetActivityId, ActionTargetProjectId, ActionNewActivityStatus, ActionNewProjectStatusId, CreatedBy)
        VALUES
            (@P_COMPANY_ID, @P_NAME, @P_TRIGGER_TYPE, @P_TRIGGER_ACTIVITY_ID, @P_TRIGGER_PROJECT_ID, @P_TRIGGER_THRESHOLD_PERCENT, @P_TRIGGER_STATUS,
             @P_ACTION_TYPE, @P_ACTION_TARGET_ACTIVITY_ID, @P_ACTION_TARGET_PROJECT_ID, @P_ACTION_NEW_ACTIVITY_STATUS, @P_ACTION_NEW_PROJECT_STATUS_ID, @P_CREATED_BY);

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

CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_DELETE
    @P_AUTOMATION_RULE_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.AutomationRule
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE AutomationRuleId = @P_AUTOMATION_RULE_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- AUTOMATION RULE LOG
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_LOG_GET_BY_RULE
    @P_AUTOMATION_RULE_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AutomationRuleLogId AS AUTOMATION_RULE_LOG_ID, AutomationRuleId AS AUTOMATION_RULE_ID,
           TriggeredDate AS TRIGGERED_DATE, Success AS SUCCESS, Details AS DETAILS
    FROM dbo.AutomationRuleLog
    WHERE AutomationRuleId = @P_AUTOMATION_RULE_ID
    ORDER BY TriggeredDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTOMATION_RULE_LOG_INSERT
    @P_AUTOMATION_RULE_ID INT,
    @P_SUCCESS BIT,
    @P_DETAILS NVARCHAR(500) = NULL,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.AutomationRuleLog (AutomationRuleId, Success, Details)
        VALUES (@P_AUTOMATION_RULE_ID, @P_SUCCESS, @P_DETAILS);

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- PROJECT (extension Modulo 7: cambiar solo el estado de flujo de trabajo, accion de una regla)
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_UPDATE_STATUS
    @P_PROJECT_ID INT,
    @P_PROJECT_STATUS_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Project
        SET ProjectStatusId = @P_PROJECT_STATUS_ID, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ProjectId = @P_PROJECT_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
