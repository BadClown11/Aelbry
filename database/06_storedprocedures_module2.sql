/*
    Modulo 2 - Stored Procedures
    Convencion: toda SP de escritura retorna @OUT_RESULT OUTPUT ('OK' o el mensaje de error
    capturado con ERROR_MESSAGE() en el bloque CATCH). El DAL valida ese valor contra C.OK
    y lanza DataBaseException si difiere.
*/

USE AelbryDb;
GO

--------------------------------------------------------------------------------------------
-- PROJECT STATUS
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_STATUS_GET_BY_COMPANY
    @P_COMPANY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProjectStatusId AS PROJECT_STATUS_ID, CompanyId AS COMPANY_ID, Name AS NAME, ColorHex AS COLOR_HEX,
           Sequence AS SEQUENCE, IsFinal AS IS_FINAL, IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY,
           CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.ProjectStatus
    WHERE CompanyId = @P_COMPANY_ID AND IsDeleted = 0
    ORDER BY Sequence, Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_STATUS_GET_BY_ID
    @P_PROJECT_STATUS_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProjectStatusId AS PROJECT_STATUS_ID, CompanyId AS COMPANY_ID, Name AS NAME, ColorHex AS COLOR_HEX,
           Sequence AS SEQUENCE, IsFinal AS IS_FINAL, IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY,
           CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.ProjectStatus
    WHERE ProjectStatusId = @P_PROJECT_STATUS_ID AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_STATUS_INSERT
    @P_COMPANY_ID INT,
    @P_NAME NVARCHAR(100),
    @P_COLOR_HEX CHAR(7),
    @P_SEQUENCE INT,
    @P_IS_FINAL BIT,
    @P_CREATED_BY INT,
    @P_NEW_PROJECT_STATUS_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.ProjectStatus (CompanyId, Name, ColorHex, Sequence, IsFinal, CreatedBy)
        VALUES (@P_COMPANY_ID, @P_NAME, ISNULL(@P_COLOR_HEX, '#6C757D'), @P_SEQUENCE, @P_IS_FINAL, @P_CREATED_BY);

        SET @P_NEW_PROJECT_STATUS_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_STATUS_UPDATE
    @P_PROJECT_STATUS_ID INT,
    @P_NAME NVARCHAR(100),
    @P_COLOR_HEX CHAR(7),
    @P_SEQUENCE INT,
    @P_IS_FINAL BIT,
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.ProjectStatus
        SET Name = @P_NAME, ColorHex = @P_COLOR_HEX, Sequence = @P_SEQUENCE, IsFinal = @P_IS_FINAL,
            IsActive = @P_IS_ACTIVE, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ProjectStatusId = @P_PROJECT_STATUS_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_STATUS_DELETE
    @P_PROJECT_STATUS_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Project WHERE ProjectStatusId = @P_PROJECT_STATUS_ID AND IsDeleted = 0)
        BEGIN
            RAISERROR('No es posible eliminar un estado que tiene proyectos asignados.', 16, 1);
        END

        UPDATE dbo.ProjectStatus
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ProjectStatusId = @P_PROJECT_STATUS_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- TAG
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_TAG_GET_BY_COMPANY
    @P_COMPANY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TagId AS TAG_ID, CompanyId AS COMPANY_ID, Name AS NAME, ColorHex AS COLOR_HEX, IsActive AS IS_ACTIVE,
           CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Tag
    WHERE CompanyId = @P_COMPANY_ID AND IsDeleted = 0
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TAG_INSERT
    @P_COMPANY_ID INT,
    @P_NAME NVARCHAR(100),
    @P_COLOR_HEX CHAR(7),
    @P_CREATED_BY INT,
    @P_NEW_TAG_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Tag (CompanyId, Name, ColorHex, CreatedBy)
        VALUES (@P_COMPANY_ID, @P_NAME, ISNULL(@P_COLOR_HEX, '#868E96'), @P_CREATED_BY);

        SET @P_NEW_TAG_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TAG_UPDATE
    @P_TAG_ID INT,
    @P_NAME NVARCHAR(100),
    @P_COLOR_HEX CHAR(7),
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Tag
        SET Name = @P_NAME, ColorHex = @P_COLOR_HEX, IsActive = @P_IS_ACTIVE,
            ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE TagId = @P_TAG_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TAG_DELETE
    @P_TAG_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.ProjectTag WHERE TagId = @P_TAG_ID;

        UPDATE dbo.Tag
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE TagId = @P_TAG_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- PROJECT
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_GET_BY_COMPANY
    @P_COMPANY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.ProjectId AS PROJECT_ID, p.CompanyId AS COMPANY_ID, p.Code AS CODE, p.Name AS NAME,
           p.ColorHex AS COLOR_HEX, p.CoverImageUrl AS COVER_IMAGE_URL, p.ClientName AS CLIENT_NAME,
           p.ProjectStatusId AS PROJECT_STATUS_ID, ps.Name AS PROJECT_STATUS_NAME,
           p.Priority AS PRIORITY, p.RiskLevel AS RISK_LEVEL, p.StartDate AS START_DATE, p.EndDate AS END_DATE,
           p.Budget AS BUDGET, p.EstimatedHours AS ESTIMATED_HOURS, p.WorkedHours AS WORKED_HOURS,
           p.ProgressPercentage AS PROGRESS_PERCENTAGE, p.ProjectManagerId AS PROJECT_MANAGER_ID,
           u.FirstName + ' ' + u.LastName AS PROJECT_MANAGER_NAME,
           p.IsActive AS IS_ACTIVE, p.CreatedBy AS CREATED_BY, p.CreatedDate AS CREATED_DATE,
           p.ModifiedBy AS MODIFIED_BY, p.ModifiedDate AS MODIFIED_DATE
    FROM dbo.Project p
    JOIN dbo.ProjectStatus ps ON ps.ProjectStatusId = p.ProjectStatusId
    JOIN dbo.[User] u ON u.UserId = p.ProjectManagerId
    WHERE p.CompanyId = @P_COMPANY_ID AND p.IsDeleted = 0
    ORDER BY p.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_GET_BY_ID
    @P_PROJECT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.ProjectId AS PROJECT_ID, p.CompanyId AS COMPANY_ID, p.Code AS CODE, p.Name AS NAME,
           p.ColorHex AS COLOR_HEX, p.CoverImageUrl AS COVER_IMAGE_URL, p.ClientName AS CLIENT_NAME,
           p.ProjectStatusId AS PROJECT_STATUS_ID, ps.Name AS PROJECT_STATUS_NAME,
           p.Priority AS PRIORITY, p.RiskLevel AS RISK_LEVEL, p.StartDate AS START_DATE, p.EndDate AS END_DATE,
           p.Budget AS BUDGET, p.EstimatedHours AS ESTIMATED_HOURS, p.WorkedHours AS WORKED_HOURS,
           p.ProgressPercentage AS PROGRESS_PERCENTAGE, p.ProjectManagerId AS PROJECT_MANAGER_ID,
           u.FirstName + ' ' + u.LastName AS PROJECT_MANAGER_NAME,
           p.IsActive AS IS_ACTIVE, p.CreatedBy AS CREATED_BY, p.CreatedDate AS CREATED_DATE,
           p.ModifiedBy AS MODIFIED_BY, p.ModifiedDate AS MODIFIED_DATE
    FROM dbo.Project p
    JOIN dbo.ProjectStatus ps ON ps.ProjectStatusId = p.ProjectStatusId
    JOIN dbo.[User] u ON u.UserId = p.ProjectManagerId
    WHERE p.ProjectId = @P_PROJECT_ID AND p.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_INSERT
    @P_COMPANY_ID INT,
    @P_CODE NVARCHAR(30),
    @P_NAME NVARCHAR(200),
    @P_COLOR_HEX CHAR(7),
    @P_COVER_IMAGE_URL NVARCHAR(500) = NULL,
    @P_CLIENT_NAME NVARCHAR(200) = NULL,
    @P_PROJECT_STATUS_ID INT,
    @P_PRIORITY TINYINT,
    @P_RISK_LEVEL TINYINT,
    @P_START_DATE DATE = NULL,
    @P_END_DATE DATE = NULL,
    @P_BUDGET DECIMAL(18,2) = NULL,
    @P_ESTIMATED_HOURS DECIMAL(9,2),
    @P_PROJECT_MANAGER_ID INT,
    @P_CREATED_BY INT,
    @P_NEW_PROJECT_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Project WHERE CompanyId = @P_COMPANY_ID AND Code = @P_CODE AND IsDeleted = 0)
        BEGIN
            RAISERROR('Ya existe un proyecto con ese codigo en la empresa.', 16, 1);
        END

        INSERT INTO dbo.Project
            (CompanyId, Code, Name, ColorHex, CoverImageUrl, ClientName, ProjectStatusId, Priority, RiskLevel,
             StartDate, EndDate, Budget, EstimatedHours, ProjectManagerId, CreatedBy)
        VALUES
            (@P_COMPANY_ID, @P_CODE, @P_NAME, ISNULL(@P_COLOR_HEX, '#4C6EF5'), @P_COVER_IMAGE_URL, @P_CLIENT_NAME,
             @P_PROJECT_STATUS_ID, @P_PRIORITY, @P_RISK_LEVEL, @P_START_DATE, @P_END_DATE, @P_BUDGET,
             @P_ESTIMATED_HOURS, @P_PROJECT_MANAGER_ID, @P_CREATED_BY);

        SET @P_NEW_PROJECT_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_UPDATE
    @P_PROJECT_ID INT,
    @P_NAME NVARCHAR(200),
    @P_COLOR_HEX CHAR(7),
    @P_COVER_IMAGE_URL NVARCHAR(500) = NULL,
    @P_CLIENT_NAME NVARCHAR(200) = NULL,
    @P_PROJECT_STATUS_ID INT,
    @P_PRIORITY TINYINT,
    @P_RISK_LEVEL TINYINT,
    @P_START_DATE DATE = NULL,
    @P_END_DATE DATE = NULL,
    @P_BUDGET DECIMAL(18,2) = NULL,
    @P_ESTIMATED_HOURS DECIMAL(9,2),
    @P_PROJECT_MANAGER_ID INT,
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Project
        SET Name = @P_NAME,
            ColorHex = @P_COLOR_HEX,
            CoverImageUrl = @P_COVER_IMAGE_URL,
            ClientName = @P_CLIENT_NAME,
            ProjectStatusId = @P_PROJECT_STATUS_ID,
            Priority = @P_PRIORITY,
            RiskLevel = @P_RISK_LEVEL,
            StartDate = @P_START_DATE,
            EndDate = @P_END_DATE,
            Budget = @P_BUDGET,
            EstimatedHours = @P_ESTIMATED_HOURS,
            ProjectManagerId = @P_PROJECT_MANAGER_ID,
            IsActive = @P_IS_ACTIVE,
            ModifiedBy = @P_MODIFIED_BY,
            ModifiedDate = SYSUTCDATETIME()
        WHERE ProjectId = @P_PROJECT_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_DELETE
    @P_PROJECT_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Project
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ProjectId = @P_PROJECT_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- PROJECT MEMBERS
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_GET_MEMBERS
    @P_PROJECT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId AS USER_ID, u.FirstName AS FIRST_NAME, u.LastName AS LAST_NAME, u.Email AS EMAIL,
           u.JobTitle AS JOB_TITLE, u.PhotoUrl AS PHOTO_URL, pm.AddedDate AS ADDED_DATE
    FROM dbo.ProjectMember pm
    JOIN dbo.[User] u ON u.UserId = pm.UserId
    WHERE pm.ProjectId = @P_PROJECT_ID
    ORDER BY u.FirstName, u.LastName;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_ADD_MEMBER
    @P_PROJECT_ID INT,
    @P_USER_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.ProjectMember WHERE ProjectId = @P_PROJECT_ID AND UserId = @P_USER_ID)
        BEGIN
            INSERT INTO dbo.ProjectMember (ProjectId, UserId) VALUES (@P_PROJECT_ID, @P_USER_ID);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_REMOVE_MEMBER
    @P_PROJECT_ID INT,
    @P_USER_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.ProjectMember WHERE ProjectId = @P_PROJECT_ID AND UserId = @P_USER_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- PROJECT TAGS
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_GET_TAGS
    @P_PROJECT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.TagId AS TAG_ID, t.CompanyId AS COMPANY_ID, t.Name AS NAME, t.ColorHex AS COLOR_HEX, t.IsActive AS IS_ACTIVE
    FROM dbo.ProjectTag pt
    JOIN dbo.Tag t ON t.TagId = pt.TagId
    WHERE pt.ProjectId = @P_PROJECT_ID
    ORDER BY t.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_ADD_TAG
    @P_PROJECT_ID INT,
    @P_TAG_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.ProjectTag WHERE ProjectId = @P_PROJECT_ID AND TagId = @P_TAG_ID)
        BEGIN
            INSERT INTO dbo.ProjectTag (ProjectId, TagId) VALUES (@P_PROJECT_ID, @P_TAG_ID);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_REMOVE_TAG
    @P_PROJECT_ID INT,
    @P_TAG_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.ProjectTag WHERE ProjectId = @P_PROJECT_ID AND TagId = @P_TAG_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- PROJECT TEMPLATE
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_TEMPLATE_GET_BY_COMPANY
    @P_COMPANY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProjectTemplateId AS PROJECT_TEMPLATE_ID, CompanyId AS COMPANY_ID, Name AS NAME, Description AS DESCRIPTION,
           DefaultPriority AS DEFAULT_PRIORITY, DefaultEstimatedHours AS DEFAULT_ESTIMATED_HOURS, IsActive AS IS_ACTIVE,
           CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.ProjectTemplate
    WHERE CompanyId = @P_COMPANY_ID AND IsDeleted = 0
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_TEMPLATE_GET_BY_ID
    @P_PROJECT_TEMPLATE_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProjectTemplateId AS PROJECT_TEMPLATE_ID, CompanyId AS COMPANY_ID, Name AS NAME, Description AS DESCRIPTION,
           DefaultPriority AS DEFAULT_PRIORITY, DefaultEstimatedHours AS DEFAULT_ESTIMATED_HOURS, IsActive AS IS_ACTIVE,
           CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.ProjectTemplate
    WHERE ProjectTemplateId = @P_PROJECT_TEMPLATE_ID AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_TEMPLATE_INSERT
    @P_COMPANY_ID INT,
    @P_NAME NVARCHAR(200),
    @P_DESCRIPTION NVARCHAR(500) = NULL,
    @P_DEFAULT_PRIORITY TINYINT,
    @P_DEFAULT_ESTIMATED_HOURS DECIMAL(9,2),
    @P_CREATED_BY INT,
    @P_NEW_PROJECT_TEMPLATE_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.ProjectTemplate (CompanyId, Name, Description, DefaultPriority, DefaultEstimatedHours, CreatedBy)
        VALUES (@P_COMPANY_ID, @P_NAME, @P_DESCRIPTION, @P_DEFAULT_PRIORITY, @P_DEFAULT_ESTIMATED_HOURS, @P_CREATED_BY);

        SET @P_NEW_PROJECT_TEMPLATE_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_TEMPLATE_UPDATE
    @P_PROJECT_TEMPLATE_ID INT,
    @P_NAME NVARCHAR(200),
    @P_DESCRIPTION NVARCHAR(500) = NULL,
    @P_DEFAULT_PRIORITY TINYINT,
    @P_DEFAULT_ESTIMATED_HOURS DECIMAL(9,2),
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.ProjectTemplate
        SET Name = @P_NAME, Description = @P_DESCRIPTION, DefaultPriority = @P_DEFAULT_PRIORITY,
            DefaultEstimatedHours = @P_DEFAULT_ESTIMATED_HOURS, IsActive = @P_IS_ACTIVE,
            ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ProjectTemplateId = @P_PROJECT_TEMPLATE_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PROJECT_TEMPLATE_DELETE
    @P_PROJECT_TEMPLATE_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.ProjectTemplate
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ProjectTemplateId = @P_PROJECT_TEMPLATE_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
