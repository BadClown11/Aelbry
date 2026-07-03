/*
    Modulo 1 - Stored Procedures
    Convencion: toda SP de escritura retorna @OUT_RESULT OUTPUT ('OK' o el mensaje de error
    capturado con ERROR_MESSAGE() en el bloque CATCH). El DAL valida ese valor contra C.OK
    y lanza DataBaseException si difiere.
*/

USE AelbryDb;
GO

--------------------------------------------------------------------------------------------
-- COMPANY
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_COMPANY_GET_ALL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CompanyId AS COMPANY_ID, Name AS NAME, LegalTaxId AS LEGAL_TAX_ID, LogoUrl AS LOGO_URL,
           TimeZone AS TIME_ZONE, IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE,
           ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Company
    WHERE IsDeleted = 0
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_COMPANY_GET_BY_ID
    @P_COMPANY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CompanyId AS COMPANY_ID, Name AS NAME, LegalTaxId AS LEGAL_TAX_ID, LogoUrl AS LOGO_URL,
           TimeZone AS TIME_ZONE, IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE,
           ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Company
    WHERE CompanyId = @P_COMPANY_ID AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_COMPANY_INSERT
    @P_NAME NVARCHAR(200),
    @P_LEGAL_TAX_ID NVARCHAR(50),
    @P_LOGO_URL NVARCHAR(500),
    @P_TIME_ZONE NVARCHAR(100),
    @P_CREATED_BY INT,
    @P_NEW_COMPANY_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Company (Name, LegalTaxId, LogoUrl, TimeZone, CreatedBy)
        VALUES (@P_NAME, @P_LEGAL_TAX_ID, @P_LOGO_URL, ISNULL(@P_TIME_ZONE, 'UTC'), @P_CREATED_BY);

        SET @P_NEW_COMPANY_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_COMPANY_UPDATE
    @P_COMPANY_ID INT,
    @P_NAME NVARCHAR(200),
    @P_LEGAL_TAX_ID NVARCHAR(50),
    @P_LOGO_URL NVARCHAR(500),
    @P_TIME_ZONE NVARCHAR(100),
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Company
        SET Name = @P_NAME,
            LegalTaxId = @P_LEGAL_TAX_ID,
            LogoUrl = @P_LOGO_URL,
            TimeZone = @P_TIME_ZONE,
            IsActive = @P_IS_ACTIVE,
            ModifiedBy = @P_MODIFIED_BY,
            ModifiedDate = SYSUTCDATETIME()
        WHERE CompanyId = @P_COMPANY_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_COMPANY_DELETE
    @P_COMPANY_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Company
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE CompanyId = @P_COMPANY_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- DEPARTMENT
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_DEPARTMENT_GET_BY_COMPANY
    @P_COMPANY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DepartmentId AS DEPARTMENT_ID, CompanyId AS COMPANY_ID, Name AS NAME, IsActive AS IS_ACTIVE,
           CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Department
    WHERE CompanyId = @P_COMPANY_ID AND IsDeleted = 0
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DEPARTMENT_GET_BY_ID
    @P_DEPARTMENT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DepartmentId AS DEPARTMENT_ID, CompanyId AS COMPANY_ID, Name AS NAME, IsActive AS IS_ACTIVE,
           CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Department
    WHERE DepartmentId = @P_DEPARTMENT_ID AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DEPARTMENT_INSERT
    @P_COMPANY_ID INT,
    @P_NAME NVARCHAR(200),
    @P_CREATED_BY INT,
    @P_NEW_DEPARTMENT_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Department (CompanyId, Name, CreatedBy)
        VALUES (@P_COMPANY_ID, @P_NAME, @P_CREATED_BY);

        SET @P_NEW_DEPARTMENT_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DEPARTMENT_UPDATE
    @P_DEPARTMENT_ID INT,
    @P_NAME NVARCHAR(200),
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Department
        SET Name = @P_NAME, IsActive = @P_IS_ACTIVE, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE DepartmentId = @P_DEPARTMENT_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_DEPARTMENT_DELETE
    @P_DEPARTMENT_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Department
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE DepartmentId = @P_DEPARTMENT_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- TEAM
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_TEAM_GET_BY_DEPARTMENT
    @P_DEPARTMENT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TeamId AS TEAM_ID, DepartmentId AS DEPARTMENT_ID, Name AS NAME, IsActive AS IS_ACTIVE,
           CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Team
    WHERE DepartmentId = @P_DEPARTMENT_ID AND IsDeleted = 0
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TEAM_GET_BY_ID
    @P_TEAM_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TeamId AS TEAM_ID, DepartmentId AS DEPARTMENT_ID, Name AS NAME, IsActive AS IS_ACTIVE,
           CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Team
    WHERE TeamId = @P_TEAM_ID AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TEAM_INSERT
    @P_DEPARTMENT_ID INT,
    @P_NAME NVARCHAR(200),
    @P_CREATED_BY INT,
    @P_NEW_TEAM_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Team (DepartmentId, Name, CreatedBy)
        VALUES (@P_DEPARTMENT_ID, @P_NAME, @P_CREATED_BY);

        SET @P_NEW_TEAM_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TEAM_UPDATE
    @P_TEAM_ID INT,
    @P_NAME NVARCHAR(200),
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Team
        SET Name = @P_NAME, IsActive = @P_IS_ACTIVE, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE TeamId = @P_TEAM_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_TEAM_DELETE
    @P_TEAM_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Team
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE TeamId = @P_TEAM_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- PERMISSION (catalogo, solo lectura)
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_PERMISSION_GET_ALL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PermissionId AS PERMISSION_ID, Code AS CODE, Module AS MODULE, Description AS DESCRIPTION
    FROM dbo.Permission
    ORDER BY Module, Code;
END
GO

--------------------------------------------------------------------------------------------
-- ROLE
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_ROLE_GET_ALL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId AS ROLE_ID, Name AS NAME, Description AS DESCRIPTION, IsSystemDefault AS IS_SYSTEM_DEFAULT,
           IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE,
           ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Role
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ROLE_GET_BY_ID
    @P_ROLE_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId AS ROLE_ID, Name AS NAME, Description AS DESCRIPTION, IsSystemDefault AS IS_SYSTEM_DEFAULT,
           IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE,
           ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.Role
    WHERE RoleId = @P_ROLE_ID;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ROLE_INSERT
    @P_NAME NVARCHAR(100),
    @P_DESCRIPTION NVARCHAR(300),
    @P_CREATED_BY INT,
    @P_NEW_ROLE_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Role (Name, Description, IsSystemDefault, CreatedBy)
        VALUES (@P_NAME, @P_DESCRIPTION, 0, @P_CREATED_BY);

        SET @P_NEW_ROLE_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ROLE_UPDATE
    @P_ROLE_ID INT,
    @P_NAME NVARCHAR(100),
    @P_DESCRIPTION NVARCHAR(300),
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Role WHERE RoleId = @P_ROLE_ID AND IsSystemDefault = 1)
        BEGIN
            RAISERROR('No es posible modificar un rol predeterminado del sistema.', 16, 1);
        END

        UPDATE dbo.Role
        SET Name = @P_NAME, Description = @P_DESCRIPTION, IsActive = @P_IS_ACTIVE,
            ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE RoleId = @P_ROLE_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ROLE_DELETE
    @P_ROLE_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Role WHERE RoleId = @P_ROLE_ID AND IsSystemDefault = 1)
        BEGIN
            RAISERROR('No es posible eliminar un rol predeterminado del sistema.', 16, 1);
        END

        UPDATE dbo.Role
        SET IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE RoleId = @P_ROLE_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ROLE_GET_PERMISSIONS
    @P_ROLE_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.PermissionId AS PERMISSION_ID, p.Code AS CODE, p.Module AS MODULE, p.Description AS DESCRIPTION
    FROM dbo.RolePermission rp
    JOIN dbo.Permission p ON p.PermissionId = rp.PermissionId
    WHERE rp.RoleId = @P_ROLE_ID
    ORDER BY p.Module, p.Code;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ROLE_ASSIGN_PERMISSION
    @P_ROLE_ID INT,
    @P_PERMISSION_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.RolePermission WHERE RoleId = @P_ROLE_ID AND PermissionId = @P_PERMISSION_ID)
        BEGIN
            INSERT INTO dbo.RolePermission (RoleId, PermissionId) VALUES (@P_ROLE_ID, @P_PERMISSION_ID);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ROLE_REMOVE_PERMISSION
    @P_ROLE_ID INT,
    @P_PERMISSION_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.RolePermission WHERE RoleId = @P_ROLE_ID AND PermissionId = @P_PERMISSION_ID;
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- [USER]
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_USER_GET_BY_COMPANY
    @P_COMPANY_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId AS USER_ID, CompanyId AS COMPANY_ID, DepartmentId AS DEPARTMENT_ID, TeamId AS TEAM_ID,
           FirstName AS FIRST_NAME, LastName AS LAST_NAME, Email AS EMAIL, JobTitle AS JOB_TITLE,
           PhotoUrl AS PHOTO_URL, TimeZone AS TIME_ZONE, WorkScheduleJson AS WORK_SCHEDULE_JSON,
           ProfileColor AS PROFILE_COLOR, IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY,
           CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.[User]
    WHERE CompanyId = @P_COMPANY_ID AND IsDeleted = 0
    ORDER BY FirstName, LastName;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USER_GET_BY_ID
    @P_USER_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId AS USER_ID, CompanyId AS COMPANY_ID, DepartmentId AS DEPARTMENT_ID, TeamId AS TEAM_ID,
           FirstName AS FIRST_NAME, LastName AS LAST_NAME, Email AS EMAIL, JobTitle AS JOB_TITLE,
           PhotoUrl AS PHOTO_URL, TimeZone AS TIME_ZONE, WorkScheduleJson AS WORK_SCHEDULE_JSON,
           ProfileColor AS PROFILE_COLOR, IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY,
           CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.[User]
    WHERE UserId = @P_USER_ID AND IsDeleted = 0;
END
GO

-- Unico SP de lectura que expone PasswordHash; de uso exclusivo del login (AuthBL).
CREATE OR ALTER PROCEDURE dbo.SP_USER_GET_BY_EMAIL
    @P_EMAIL NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId AS USER_ID, CompanyId AS COMPANY_ID, DepartmentId AS DEPARTMENT_ID, TeamId AS TEAM_ID,
           FirstName AS FIRST_NAME, LastName AS LAST_NAME, Email AS EMAIL, PasswordHash AS PASSWORD_HASH,
           JobTitle AS JOB_TITLE, PhotoUrl AS PHOTO_URL, TimeZone AS TIME_ZONE,
           WorkScheduleJson AS WORK_SCHEDULE_JSON, ProfileColor AS PROFILE_COLOR, IsActive AS IS_ACTIVE,
           CreatedBy AS CREATED_BY, CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.[User]
    WHERE Email = @P_EMAIL AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USER_INSERT
    @P_COMPANY_ID INT,
    @P_DEPARTMENT_ID INT = NULL,
    @P_TEAM_ID INT = NULL,
    @P_FIRST_NAME NVARCHAR(100),
    @P_LAST_NAME NVARCHAR(100),
    @P_EMAIL NVARCHAR(256),
    @P_PASSWORD_HASH NVARCHAR(300),
    @P_JOB_TITLE NVARCHAR(150) = NULL,
    @P_TIME_ZONE NVARCHAR(100),
    @P_WORK_SCHEDULE_JSON NVARCHAR(MAX) = NULL,
    @P_PROFILE_COLOR CHAR(7) = NULL,
    @P_CREATED_BY INT,
    @P_NEW_USER_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.[User] WHERE Email = @P_EMAIL AND IsDeleted = 0)
        BEGIN
            RAISERROR('Ya existe un usuario registrado con ese correo electronico.', 16, 1);
        END

        INSERT INTO dbo.[User]
            (CompanyId, DepartmentId, TeamId, FirstName, LastName, Email, PasswordHash,
             JobTitle, TimeZone, WorkScheduleJson, ProfileColor, CreatedBy)
        VALUES
            (@P_COMPANY_ID, @P_DEPARTMENT_ID, @P_TEAM_ID, @P_FIRST_NAME, @P_LAST_NAME, @P_EMAIL, @P_PASSWORD_HASH,
             @P_JOB_TITLE, ISNULL(@P_TIME_ZONE, 'UTC'), @P_WORK_SCHEDULE_JSON, @P_PROFILE_COLOR, @P_CREATED_BY);

        SET @P_NEW_USER_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USER_UPDATE
    @P_USER_ID INT,
    @P_DEPARTMENT_ID INT = NULL,
    @P_TEAM_ID INT = NULL,
    @P_FIRST_NAME NVARCHAR(100),
    @P_LAST_NAME NVARCHAR(100),
    @P_JOB_TITLE NVARCHAR(150) = NULL,
    @P_PHOTO_URL NVARCHAR(500) = NULL,
    @P_TIME_ZONE NVARCHAR(100),
    @P_WORK_SCHEDULE_JSON NVARCHAR(MAX) = NULL,
    @P_PROFILE_COLOR CHAR(7) = NULL,
    @P_IS_ACTIVE BIT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.[User]
        SET DepartmentId = @P_DEPARTMENT_ID,
            TeamId = @P_TEAM_ID,
            FirstName = @P_FIRST_NAME,
            LastName = @P_LAST_NAME,
            JobTitle = @P_JOB_TITLE,
            PhotoUrl = @P_PHOTO_URL,
            TimeZone = @P_TIME_ZONE,
            WorkScheduleJson = @P_WORK_SCHEDULE_JSON,
            ProfileColor = @P_PROFILE_COLOR,
            IsActive = @P_IS_ACTIVE,
            ModifiedBy = @P_MODIFIED_BY,
            ModifiedDate = SYSUTCDATETIME()
        WHERE UserId = @P_USER_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USER_UPDATE_PASSWORD
    @P_USER_ID INT,
    @P_PASSWORD_HASH NVARCHAR(300),
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.[User]
        SET PasswordHash = @P_PASSWORD_HASH, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE UserId = @P_USER_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USER_DELETE
    @P_USER_ID INT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.[User]
        SET IsDeleted = 1, IsActive = 0, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE UserId = @P_USER_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USER_GET_ROLES
    @P_USER_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.RoleId AS ROLE_ID, r.Name AS NAME, r.Description AS DESCRIPTION,
           r.IsSystemDefault AS IS_SYSTEM_DEFAULT, r.IsActive AS IS_ACTIVE
    FROM dbo.UserRole ur
    JOIN dbo.Role r ON r.RoleId = ur.RoleId
    WHERE ur.UserId = @P_USER_ID
    ORDER BY r.Name;
END
GO

-- Union de todos los permisos otorgados por los roles activos del usuario (RBAC efectivo).
CREATE OR ALTER PROCEDURE dbo.SP_USER_GET_PERMISSIONS
    @P_USER_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT p.PermissionId AS PERMISSION_ID, p.Code AS CODE, p.Module AS MODULE, p.Description AS DESCRIPTION
    FROM dbo.UserRole ur
    JOIN dbo.Role r ON r.RoleId = ur.RoleId AND r.IsActive = 1
    JOIN dbo.RolePermission rp ON rp.RoleId = r.RoleId
    JOIN dbo.Permission p ON p.PermissionId = rp.PermissionId
    WHERE ur.UserId = @P_USER_ID
    ORDER BY p.Module, p.Code;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USER_ASSIGN_ROLE
    @P_USER_ID INT,
    @P_ROLE_ID INT,
    @P_COMPANY_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.UserRole WHERE UserId = @P_USER_ID AND RoleId = @P_ROLE_ID AND CompanyId = @P_COMPANY_ID)
        BEGIN
            INSERT INTO dbo.UserRole (UserId, RoleId, CompanyId) VALUES (@P_USER_ID, @P_ROLE_ID, @P_COMPANY_ID);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USER_REMOVE_ROLE
    @P_USER_ID INT,
    @P_ROLE_ID INT,
    @P_COMPANY_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.UserRole
        WHERE UserId = @P_USER_ID AND RoleId = @P_ROLE_ID AND CompanyId = @P_COMPANY_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- AUTH / REFRESH TOKENS
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_AUTH_SAVE_REFRESH_TOKEN
    @P_USER_ID INT,
    @P_TOKEN NVARCHAR(300),
    @P_EXPIRES_AT DATETIME2,
    @P_CREATED_BY_IP NVARCHAR(64),
    @P_NEW_REFRESH_TOKEN_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.RefreshToken (UserId, Token, ExpiresAt, CreatedByIp)
        VALUES (@P_USER_ID, @P_TOKEN, @P_EXPIRES_AT, @P_CREATED_BY_IP);

        SET @P_NEW_REFRESH_TOKEN_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTH_GET_REFRESH_TOKEN
    @P_TOKEN NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RefreshTokenId AS REFRESH_TOKEN_ID, UserId AS USER_ID, Token AS TOKEN, ExpiresAt AS EXPIRES_AT,
           CreatedAt AS CREATED_AT, CreatedByIp AS CREATED_BY_IP, RevokedAt AS REVOKED_AT,
           ReplacedByToken AS REPLACED_BY_TOKEN
    FROM dbo.RefreshToken
    WHERE Token = @P_TOKEN;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTH_REVOKE_REFRESH_TOKEN
    @P_TOKEN NVARCHAR(300),
    @P_REPLACED_BY_TOKEN NVARCHAR(300) = NULL,
    @P_REVOKED_BY_IP NVARCHAR(64),
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.RefreshToken
        SET RevokedAt = SYSUTCDATETIME(), RevokedByIp = @P_REVOKED_BY_IP, ReplacedByToken = @P_REPLACED_BY_TOKEN
        WHERE Token = @P_TOKEN AND RevokedAt IS NULL;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
