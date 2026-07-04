/*
    Modulo 6 (parte 1) - Stored Procedures de Time Tracking.
*/

USE AelbryDb;
GO

--------------------------------------------------------------------------------------------
-- TIME ENTRY
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_GET_BY_ID
    @P_TIME_ENTRY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT te.TimeEntryId AS TIME_ENTRY_ID, te.ActivityId AS ACTIVITY_ID, a.Name AS ACTIVITY_NAME,
           te.UserId AS USER_ID, u.FirstName + ' ' + u.LastName AS USER_NAME,
           te.StartTime AS START_TIME, te.EndTime AS END_TIME, te.DurationHours AS DURATION_HOURS,
           te.IsManual AS IS_MANUAL, te.IsOvertime AS IS_OVERTIME, te.Notes AS NOTES,
           te.CreatedBy AS CREATED_BY, te.CreatedDate AS CREATED_DATE, te.ModifiedBy AS MODIFIED_BY, te.ModifiedDate AS MODIFIED_DATE
    FROM dbo.TimeEntry te
    JOIN dbo.Activity a ON a.ActivityId = te.ActivityId
    JOIN dbo.[User] u ON u.UserId = te.UserId
    WHERE te.TimeEntryId = @P_TIME_ENTRY_ID AND te.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_GET_RUNNING_BY_USER
    @P_USER_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 te.TimeEntryId AS TIME_ENTRY_ID, te.ActivityId AS ACTIVITY_ID, a.Name AS ACTIVITY_NAME,
           te.UserId AS USER_ID, u.FirstName + ' ' + u.LastName AS USER_NAME,
           te.StartTime AS START_TIME, te.EndTime AS END_TIME, te.DurationHours AS DURATION_HOURS,
           te.IsManual AS IS_MANUAL, te.IsOvertime AS IS_OVERTIME, te.Notes AS NOTES,
           te.CreatedBy AS CREATED_BY, te.CreatedDate AS CREATED_DATE, te.ModifiedBy AS MODIFIED_BY, te.ModifiedDate AS MODIFIED_DATE
    FROM dbo.TimeEntry te
    JOIN dbo.Activity a ON a.ActivityId = te.ActivityId
    JOIN dbo.[User] u ON u.UserId = te.UserId
    WHERE te.UserId = @P_USER_ID AND te.EndTime IS NULL AND te.IsDeleted = 0
    ORDER BY te.StartTime DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_GET_BY_ACTIVITY
    @P_ACTIVITY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT te.TimeEntryId AS TIME_ENTRY_ID, te.ActivityId AS ACTIVITY_ID, a.Name AS ACTIVITY_NAME,
           te.UserId AS USER_ID, u.FirstName + ' ' + u.LastName AS USER_NAME,
           te.StartTime AS START_TIME, te.EndTime AS END_TIME, te.DurationHours AS DURATION_HOURS,
           te.IsManual AS IS_MANUAL, te.IsOvertime AS IS_OVERTIME, te.Notes AS NOTES,
           te.CreatedBy AS CREATED_BY, te.CreatedDate AS CREATED_DATE, te.ModifiedBy AS MODIFIED_BY, te.ModifiedDate AS MODIFIED_DATE
    FROM dbo.TimeEntry te
    JOIN dbo.Activity a ON a.ActivityId = te.ActivityId
    JOIN dbo.[User] u ON u.UserId = te.UserId
    WHERE te.ActivityId = @P_ACTIVITY_ID AND te.IsDeleted = 0
    ORDER BY te.StartTime DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_GET_BY_USER
    @P_USER_ID INT,
    @P_START_DATE DATE = NULL,
    @P_END_DATE DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT te.TimeEntryId AS TIME_ENTRY_ID, te.ActivityId AS ACTIVITY_ID, a.Name AS ACTIVITY_NAME,
           te.UserId AS USER_ID, u.FirstName + ' ' + u.LastName AS USER_NAME,
           te.StartTime AS START_TIME, te.EndTime AS END_TIME, te.DurationHours AS DURATION_HOURS,
           te.IsManual AS IS_MANUAL, te.IsOvertime AS IS_OVERTIME, te.Notes AS NOTES,
           te.CreatedBy AS CREATED_BY, te.CreatedDate AS CREATED_DATE, te.ModifiedBy AS MODIFIED_BY, te.ModifiedDate AS MODIFIED_DATE
    FROM dbo.TimeEntry te
    JOIN dbo.Activity a ON a.ActivityId = te.ActivityId
    JOIN dbo.[User] u ON u.UserId = te.UserId
    WHERE te.UserId = @P_USER_ID AND te.IsDeleted = 0
      AND (@P_START_DATE IS NULL OR CAST(te.StartTime AS DATE) >= @P_START_DATE)
      AND (@P_END_DATE IS NULL OR CAST(te.StartTime AS DATE) <= @P_END_DATE)
    ORDER BY te.StartTime DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_GET_TOTAL_HOURS_BY_ACTIVITY
    @P_ACTIVITY_ID INT,
    @P_TOTAL_HOURS DECIMAL(9,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @P_TOTAL_HOURS = ISNULL(SUM(DurationHours), 0)
    FROM dbo.TimeEntry
    WHERE ActivityId = @P_ACTIVITY_ID AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_START
    @P_ACTIVITY_ID INT,
    @P_USER_ID INT,
    @P_CREATED_BY INT,
    @P_NEW_TIME_ENTRY_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.TimeEntry (ActivityId, UserId, StartTime, IsManual, CreatedBy)
        VALUES (@P_ACTIVITY_ID, @P_USER_ID, SYSUTCDATETIME(), 0, @P_CREATED_BY);

        SET @P_NEW_TIME_ENTRY_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_STOP
    @P_TIME_ENTRY_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.TimeEntry
        SET EndTime = SYSUTCDATETIME(),
            DurationHours = ROUND(DATEDIFF(SECOND, StartTime, SYSUTCDATETIME()) / 3600.0, 2),
            ModifiedBy = @P_MODIFIED_BY,
            ModifiedDate = SYSUTCDATETIME()
        WHERE TimeEntryId = @P_TIME_ENTRY_ID AND EndTime IS NULL AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_INSERT_MANUAL
    @P_ACTIVITY_ID INT,
    @P_USER_ID INT,
    @P_START_TIME DATETIME2,
    @P_END_TIME DATETIME2,
    @P_DURATION_HOURS DECIMAL(9,2),
    @P_IS_OVERTIME BIT,
    @P_NOTES NVARCHAR(500) = NULL,
    @P_CREATED_BY INT,
    @P_NEW_TIME_ENTRY_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.TimeEntry (ActivityId, UserId, StartTime, EndTime, DurationHours, IsManual, IsOvertime, Notes, CreatedBy)
        VALUES (@P_ACTIVITY_ID, @P_USER_ID, @P_START_TIME, @P_END_TIME, @P_DURATION_HOURS, 1, @P_IS_OVERTIME, @P_NOTES, @P_CREATED_BY);

        SET @P_NEW_TIME_ENTRY_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_UPDATE
    @P_TIME_ENTRY_ID INT,
    @P_DURATION_HOURS DECIMAL(9,2),
    @P_IS_OVERTIME BIT,
    @P_NOTES NVARCHAR(500) = NULL,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.TimeEntry
        SET DurationHours = @P_DURATION_HOURS,
            IsOvertime = @P_IS_OVERTIME,
            Notes = @P_NOTES,
            ModifiedBy = @P_MODIFIED_BY,
            ModifiedDate = SYSUTCDATETIME()
        WHERE TimeEntryId = @P_TIME_ENTRY_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TIME_ENTRY_DELETE
    @P_TIME_ENTRY_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.TimeEntry
        SET IsDeleted = 1, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE TimeEntryId = @P_TIME_ENTRY_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- ACTIVITY (extension Modulo 6: persistir las horas trabajadas agregadas desde TimeEntry)
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_UPDATE_WORKED_HOURS
    @P_ACTIVITY_ID INT,
    @P_WORKED_HOURS DECIMAL(9,2),
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Activity
        SET WorkedHours = @P_WORKED_HOURS, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ActivityId = @P_ACTIVITY_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
