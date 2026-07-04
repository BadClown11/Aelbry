/*
    Modulo 3 - Stored Procedures
    Convencion: toda SP de escritura retorna @OUT_RESULT OUTPUT ('OK' o el mensaje de error
    capturado con ERROR_MESSAGE() en el bloque CATCH). El DAL valida ese valor contra C.OK
    y lanza DataBaseException si difiere.
*/

USE AelbryDb;
GO

--------------------------------------------------------------------------------------------
-- ACTIVITY
--------------------------------------------------------------------------------------------

-- Trae TODAS las actividades (planas) de un proyecto; el DAL ensambla el arbol en memoria
-- usando ParentActivityId.
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_GET_BY_PROJECT
    @P_PROJECT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.ActivityId AS ACTIVITY_ID, a.ProjectId AS PROJECT_ID, a.ParentActivityId AS PARENT_ACTIVITY_ID,
           a.Code AS CODE, a.Name AS NAME, a.Description AS DESCRIPTION, a.Category AS CATEGORY,
           a.ColorHex AS COLOR_HEX, a.Status AS STATUS, a.Priority AS PRIORITY,
           a.ResponsibleUserId AS RESPONSIBLE_USER_ID, u.FirstName + ' ' + u.LastName AS RESPONSIBLE_NAME,
           a.EstimatedStartDate AS ESTIMATED_START_DATE, a.EstimatedEndDate AS ESTIMATED_END_DATE,
           a.ActualStartDate AS ACTUAL_START_DATE, a.ActualEndDate AS ACTUAL_END_DATE,
           a.Weight AS WEIGHT, a.EstimatedHours AS ESTIMATED_HOURS, a.WorkedHours AS WORKED_HOURS,
           a.ProgressPercentage AS PROGRESS_PERCENTAGE, a.Sequence AS SEQUENCE, a.IsActive AS IS_ACTIVE,
           a.CreatedBy AS CREATED_BY, a.CreatedDate AS CREATED_DATE, a.ModifiedBy AS MODIFIED_BY, a.ModifiedDate AS MODIFIED_DATE
    FROM dbo.Activity a
    JOIN dbo.[User] u ON u.UserId = a.ResponsibleUserId
    WHERE a.ProjectId = @P_PROJECT_ID AND a.IsDeleted = 0
    ORDER BY a.ParentActivityId, a.Sequence, a.ActivityId;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_GET_BY_ID
    @P_ACTIVITY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.ActivityId AS ACTIVITY_ID, a.ProjectId AS PROJECT_ID, a.ParentActivityId AS PARENT_ACTIVITY_ID,
           a.Code AS CODE, a.Name AS NAME, a.Description AS DESCRIPTION, a.Category AS CATEGORY,
           a.ColorHex AS COLOR_HEX, a.Status AS STATUS, a.Priority AS PRIORITY,
           a.ResponsibleUserId AS RESPONSIBLE_USER_ID, u.FirstName + ' ' + u.LastName AS RESPONSIBLE_NAME,
           a.EstimatedStartDate AS ESTIMATED_START_DATE, a.EstimatedEndDate AS ESTIMATED_END_DATE,
           a.ActualStartDate AS ACTUAL_START_DATE, a.ActualEndDate AS ACTUAL_END_DATE,
           a.Weight AS WEIGHT, a.EstimatedHours AS ESTIMATED_HOURS, a.WorkedHours AS WORKED_HOURS,
           a.ProgressPercentage AS PROGRESS_PERCENTAGE, a.Sequence AS SEQUENCE, a.IsActive AS IS_ACTIVE,
           a.CreatedBy AS CREATED_BY, a.CreatedDate AS CREATED_DATE, a.ModifiedBy AS MODIFIED_BY, a.ModifiedDate AS MODIFIED_DATE
    FROM dbo.Activity a
    JOIN dbo.[User] u ON u.UserId = a.ResponsibleUserId
    WHERE a.ActivityId = @P_ACTIVITY_ID AND a.IsDeleted = 0;
END
GO

-- Devuelve los hijos directos de una actividad (usado por el BL para recorrer el arbol al recalcular).
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_GET_CHILDREN
    @P_PARENT_ACTIVITY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ActivityId AS ACTIVITY_ID, Weight AS WEIGHT, ProgressPercentage AS PROGRESS_PERCENTAGE
    FROM dbo.Activity
    WHERE ParentActivityId = @P_PARENT_ACTIVITY_ID AND IsDeleted = 0;
END
GO

-- Devuelve los hijos directos de nivel raiz (ParentActivityId NULL) de un proyecto.
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_GET_ROOT_BY_PROJECT
    @P_PROJECT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ActivityId AS ACTIVITY_ID, Weight AS WEIGHT, ProgressPercentage AS PROGRESS_PERCENTAGE
    FROM dbo.Activity
    WHERE ProjectId = @P_PROJECT_ID AND ParentActivityId IS NULL AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_INSERT
    @P_PROJECT_ID INT,
    @P_PARENT_ACTIVITY_ID INT = NULL,
    @P_CODE NVARCHAR(40),
    @P_NAME NVARCHAR(200),
    @P_DESCRIPTION NVARCHAR(MAX) = NULL,
    @P_CATEGORY NVARCHAR(100) = NULL,
    @P_COLOR_HEX CHAR(7),
    @P_STATUS TINYINT,
    @P_PRIORITY TINYINT,
    @P_RESPONSIBLE_USER_ID INT,
    @P_ESTIMATED_START_DATE DATE = NULL,
    @P_ESTIMATED_END_DATE DATE = NULL,
    @P_WEIGHT DECIMAL(9,2),
    @P_ESTIMATED_HOURS DECIMAL(9,2),
    @P_SEQUENCE INT,
    @P_CREATED_BY INT,
    @P_NEW_ACTIVITY_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Activity
            (ProjectId, ParentActivityId, Code, Name, Description, Category, ColorHex, Status, Priority,
             ResponsibleUserId, EstimatedStartDate, EstimatedEndDate, Weight, EstimatedHours, Sequence, CreatedBy)
        VALUES
            (@P_PROJECT_ID, @P_PARENT_ACTIVITY_ID, @P_CODE, @P_NAME, @P_DESCRIPTION, @P_CATEGORY, ISNULL(@P_COLOR_HEX, '#4C6EF5'),
             @P_STATUS, @P_PRIORITY, @P_RESPONSIBLE_USER_ID, @P_ESTIMATED_START_DATE, @P_ESTIMATED_END_DATE,
             @P_WEIGHT, @P_ESTIMATED_HOURS, @P_SEQUENCE, @P_CREATED_BY);

        SET @P_NEW_ACTIVITY_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_UPDATE
    @P_ACTIVITY_ID INT,
    @P_NAME NVARCHAR(200),
    @P_DESCRIPTION NVARCHAR(MAX) = NULL,
    @P_CATEGORY NVARCHAR(100) = NULL,
    @P_COLOR_HEX CHAR(7),
    @P_STATUS TINYINT,
    @P_PRIORITY TINYINT,
    @P_RESPONSIBLE_USER_ID INT,
    @P_ESTIMATED_START_DATE DATE = NULL,
    @P_ESTIMATED_END_DATE DATE = NULL,
    @P_ACTUAL_START_DATE DATE = NULL,
    @P_ACTUAL_END_DATE DATE = NULL,
    @P_WEIGHT DECIMAL(9,2),
    @P_ESTIMATED_HOURS DECIMAL(9,2),
    @P_WORKED_HOURS DECIMAL(9,2),
    @P_SEQUENCE INT,
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Activity
        SET Name = @P_NAME,
            Description = @P_DESCRIPTION,
            Category = @P_CATEGORY,
            ColorHex = @P_COLOR_HEX,
            Status = @P_STATUS,
            Priority = @P_PRIORITY,
            ResponsibleUserId = @P_RESPONSIBLE_USER_ID,
            EstimatedStartDate = @P_ESTIMATED_START_DATE,
            EstimatedEndDate = @P_ESTIMATED_END_DATE,
            ActualStartDate = @P_ACTUAL_START_DATE,
            ActualEndDate = @P_ACTUAL_END_DATE,
            Weight = @P_WEIGHT,
            EstimatedHours = @P_ESTIMATED_HOURS,
            WorkedHours = @P_WORKED_HOURS,
            Sequence = @P_SEQUENCE,
            IsActive = @P_IS_ACTIVE,
            ModifiedBy = @P_MODIFIED_BY,
            ModifiedDate = SYSUTCDATETIME()
        WHERE ActivityId = @P_ACTIVITY_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

-- SP dedicado y liviano usado por el servicio de recalculo de avance (no toca el resto de columnas).
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_UPDATE_PROGRESS
    @P_ACTIVITY_ID INT,
    @P_PROGRESS_PERCENTAGE DECIMAL(5,2),
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Activity
        SET ProgressPercentage = @P_PROGRESS_PERCENTAGE, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ActivityId = @P_ACTIVITY_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_DELETE
    @P_ACTIVITY_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Activity WHERE ParentActivityId = @P_ACTIVITY_ID AND IsDeleted = 0)
        BEGIN
            RAISERROR('No es posible eliminar una actividad que tiene subactividades.', 16, 1);
        END

        IF EXISTS (SELECT 1 FROM dbo.ActivityDependency WHERE DependsOnActivityId = @P_ACTIVITY_ID)
        BEGIN
            RAISERROR('No es posible eliminar una actividad de la que otras dependen.', 16, 1);
        END

        UPDATE dbo.Activity
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ActivityId = @P_ACTIVITY_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- ACTIVITY PARTICIPANTS
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_GET_PARTICIPANTS
    @P_ACTIVITY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId AS USER_ID, u.FirstName AS FIRST_NAME, u.LastName AS LAST_NAME, u.Email AS EMAIL,
           u.JobTitle AS JOB_TITLE, u.PhotoUrl AS PHOTO_URL, ap.AddedDate AS ADDED_DATE
    FROM dbo.ActivityParticipant ap
    JOIN dbo.[User] u ON u.UserId = ap.UserId
    WHERE ap.ActivityId = @P_ACTIVITY_ID
    ORDER BY u.FirstName, u.LastName;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_ADD_PARTICIPANT
    @P_ACTIVITY_ID INT,
    @P_USER_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.ActivityParticipant WHERE ActivityId = @P_ACTIVITY_ID AND UserId = @P_USER_ID)
        BEGIN
            INSERT INTO dbo.ActivityParticipant (ActivityId, UserId) VALUES (@P_ACTIVITY_ID, @P_USER_ID);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_REMOVE_PARTICIPANT
    @P_ACTIVITY_ID INT,
    @P_USER_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.ActivityParticipant WHERE ActivityId = @P_ACTIVITY_ID AND UserId = @P_USER_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- ACTIVITY TAGS
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_GET_TAGS
    @P_ACTIVITY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.TagId AS TAG_ID, t.CompanyId AS COMPANY_ID, t.Name AS NAME, t.ColorHex AS COLOR_HEX, t.IsActive AS IS_ACTIVE
    FROM dbo.ActivityTag at
    JOIN dbo.Tag t ON t.TagId = at.TagId
    WHERE at.ActivityId = @P_ACTIVITY_ID
    ORDER BY t.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_ADD_TAG
    @P_ACTIVITY_ID INT,
    @P_TAG_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.ActivityTag WHERE ActivityId = @P_ACTIVITY_ID AND TagId = @P_TAG_ID)
        BEGIN
            INSERT INTO dbo.ActivityTag (ActivityId, TagId) VALUES (@P_ACTIVITY_ID, @P_TAG_ID);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_REMOVE_TAG
    @P_ACTIVITY_ID INT,
    @P_TAG_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.ActivityTag WHERE ActivityId = @P_ACTIVITY_ID AND TagId = @P_TAG_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- CHECKLIST ITEM
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_CHECKLIST_ITEM_GET_BY_ACTIVITY
    @P_ACTIVITY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ChecklistItemId AS CHECKLIST_ITEM_ID, ActivityId AS ACTIVITY_ID, Text AS TEXT, IsChecked AS IS_CHECKED,
           Sequence AS SEQUENCE, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE,
           CompletedBy AS COMPLETED_BY, CompletedDate AS COMPLETED_DATE
    FROM dbo.ChecklistItem
    WHERE ActivityId = @P_ACTIVITY_ID
    ORDER BY Sequence, ChecklistItemId;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHECKLIST_ITEM_INSERT
    @P_ACTIVITY_ID INT,
    @P_TEXT NVARCHAR(300),
    @P_SEQUENCE INT,
    @P_CREATED_BY INT,
    @P_NEW_CHECKLIST_ITEM_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.ChecklistItem (ActivityId, Text, Sequence, CreatedBy)
        VALUES (@P_ACTIVITY_ID, @P_TEXT, @P_SEQUENCE, @P_CREATED_BY);

        SET @P_NEW_CHECKLIST_ITEM_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHECKLIST_ITEM_TOGGLE
    @P_CHECKLIST_ITEM_ID INT,
    @P_IS_CHECKED BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.ChecklistItem
        SET IsChecked = @P_IS_CHECKED,
            CompletedBy = CASE WHEN @P_IS_CHECKED = 1 THEN @P_MODIFIED_BY ELSE NULL END,
            CompletedDate = CASE WHEN @P_IS_CHECKED = 1 THEN SYSUTCDATETIME() ELSE NULL END
        WHERE ChecklistItemId = @P_CHECKLIST_ITEM_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHECKLIST_ITEM_DELETE
    @P_CHECKLIST_ITEM_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.ChecklistItem WHERE ChecklistItemId = @P_CHECKLIST_ITEM_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- ACTIVITY DEPENDENCY
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_DEPENDENCY_GET_BY_ACTIVITY
    @P_ACTIVITY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.ActivityDependencyId AS ACTIVITY_DEPENDENCY_ID, d.ActivityId AS ACTIVITY_ID,
           d.DependsOnActivityId AS DEPENDS_ON_ACTIVITY_ID, a.Name AS DEPENDS_ON_ACTIVITY_NAME,
           d.DependencyType AS DEPENDENCY_TYPE, d.CreatedBy AS CREATED_BY, d.CreatedDate AS CREATED_DATE
    FROM dbo.ActivityDependency d
    JOIN dbo.Activity a ON a.ActivityId = d.DependsOnActivityId
    WHERE d.ActivityId = @P_ACTIVITY_ID
    ORDER BY d.CreatedDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_DEPENDENCY_INSERT
    @P_ACTIVITY_ID INT,
    @P_DEPENDS_ON_ACTIVITY_ID INT,
    @P_DEPENDENCY_TYPE TINYINT,
    @P_CREATED_BY INT,
    @P_NEW_ACTIVITY_DEPENDENCY_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @P_ACTIVITY_ID = @P_DEPENDS_ON_ACTIVITY_ID
        BEGIN
            RAISERROR('Una actividad no puede depender de si misma.', 16, 1);
        END

        INSERT INTO dbo.ActivityDependency (ActivityId, DependsOnActivityId, DependencyType, CreatedBy)
        VALUES (@P_ACTIVITY_ID, @P_DEPENDS_ON_ACTIVITY_ID, @P_DEPENDENCY_TYPE, @P_CREATED_BY);

        SET @P_NEW_ACTIVITY_DEPENDENCY_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_DEPENDENCY_DELETE
    @P_ACTIVITY_DEPENDENCY_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.ActivityDependency WHERE ActivityDependencyId = @P_ACTIVITY_DEPENDENCY_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- PROGRESO POR EMPLEADO / EQUIPO (consultas de agregacion bajo demanda)
--------------------------------------------------------------------------------------------

-- Actividades activas donde el usuario es responsable o participante (para el promedio ponderado por empleado).
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_GET_BY_USER
    @P_USER_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT a.ActivityId AS ACTIVITY_ID, a.Weight AS WEIGHT, a.ProgressPercentage AS PROGRESS_PERCENTAGE
    FROM dbo.Activity a
    LEFT JOIN dbo.ActivityParticipant ap ON ap.ActivityId = a.ActivityId
    JOIN dbo.Project p ON p.ProjectId = a.ProjectId
    WHERE a.IsDeleted = 0 AND a.IsActive = 1 AND p.IsDeleted = 0
      AND (a.ResponsibleUserId = @P_USER_ID OR ap.UserId = @P_USER_ID);
END
GO

-- Igual que el anterior, pero para todos los usuarios de un equipo (promedio ponderado por equipo).
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_GET_BY_TEAM
    @P_TEAM_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT a.ActivityId AS ACTIVITY_ID, a.Weight AS WEIGHT, a.ProgressPercentage AS PROGRESS_PERCENTAGE
    FROM dbo.Activity a
    LEFT JOIN dbo.ActivityParticipant ap ON ap.ActivityId = a.ActivityId
    JOIN dbo.Project p ON p.ProjectId = a.ProjectId
    JOIN dbo.[User] u ON u.UserId = a.ResponsibleUserId OR u.UserId = ap.UserId
    WHERE a.IsDeleted = 0 AND a.IsActive = 1 AND p.IsDeleted = 0 AND u.TeamId = @P_TEAM_ID;
END
GO

--------------------------------------------------------------------------------------------
-- PROJECT (extension Modulo 3: persistir el avance recalculado en cascada)
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_UPDATE_PROGRESS
    @P_PROJECT_ID INT,
    @P_PROGRESS_PERCENTAGE DECIMAL(5,2),
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Project
        SET ProgressPercentage = @P_PROGRESS_PERCENTAGE, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ProjectId = @P_PROJECT_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
