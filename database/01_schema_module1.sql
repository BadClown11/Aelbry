/*
    Modulo 1 - Arquitectura Base & Gestion de Identidad Empresarial
    Motor: SQL Server
    Contenido: tablas, llaves foraneas, restricciones unicas e indices.
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.RefreshToken', 'U') IS NOT NULL DROP TABLE dbo.RefreshToken;
IF OBJECT_ID('dbo.UserRole', 'U') IS NOT NULL DROP TABLE dbo.UserRole;
IF OBJECT_ID('dbo.RolePermission', 'U') IS NOT NULL DROP TABLE dbo.RolePermission;
IF OBJECT_ID('dbo.Permission', 'U') IS NOT NULL DROP TABLE dbo.Permission;
IF OBJECT_ID('dbo.Role', 'U') IS NOT NULL DROP TABLE dbo.Role;
IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL DROP TABLE dbo.[User];
IF OBJECT_ID('dbo.Team', 'U') IS NOT NULL DROP TABLE dbo.Team;
IF OBJECT_ID('dbo.Department', 'U') IS NOT NULL DROP TABLE dbo.Department;
IF OBJECT_ID('dbo.Company', 'U') IS NOT NULL DROP TABLE dbo.Company;
GO

CREATE TABLE dbo.Company
(
    CompanyId     INT IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(200)     NOT NULL,
    LegalTaxId    NVARCHAR(50)      NULL,
    LogoUrl       NVARCHAR(500)     NULL,
    TimeZone      NVARCHAR(100)     NOT NULL CONSTRAINT DF_Company_TimeZone DEFAULT ('UTC'),
    IsActive      BIT               NOT NULL CONSTRAINT DF_Company_IsActive DEFAULT (1),
    CreatedBy     INT               NOT NULL,
    CreatedDate   DATETIME2         NOT NULL CONSTRAINT DF_Company_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy    INT               NULL,
    ModifiedDate  DATETIME2         NULL,
    IsDeleted     BIT               NOT NULL CONSTRAINT DF_Company_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Company PRIMARY KEY CLUSTERED (CompanyId),
    CONSTRAINT UQ_Company_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.Department
(
    DepartmentId  INT IDENTITY(1,1) NOT NULL,
    CompanyId     INT               NOT NULL,
    Name          NVARCHAR(200)     NOT NULL,
    IsActive      BIT               NOT NULL CONSTRAINT DF_Department_IsActive DEFAULT (1),
    CreatedBy     INT               NOT NULL,
    CreatedDate   DATETIME2         NOT NULL CONSTRAINT DF_Department_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy    INT               NULL,
    ModifiedDate  DATETIME2         NULL,
    IsDeleted     BIT               NOT NULL CONSTRAINT DF_Department_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Department PRIMARY KEY CLUSTERED (DepartmentId),
    CONSTRAINT FK_Department_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT UQ_Department_Company_Name UNIQUE (CompanyId, Name)
);
GO

CREATE TABLE dbo.Team
(
    TeamId        INT IDENTITY(1,1) NOT NULL,
    DepartmentId  INT               NOT NULL,
    Name          NVARCHAR(200)     NOT NULL,
    IsActive      BIT               NOT NULL CONSTRAINT DF_Team_IsActive DEFAULT (1),
    CreatedBy     INT               NOT NULL,
    CreatedDate   DATETIME2         NOT NULL CONSTRAINT DF_Team_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy    INT               NULL,
    ModifiedDate  DATETIME2         NULL,
    IsDeleted     BIT               NOT NULL CONSTRAINT DF_Team_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Team PRIMARY KEY CLUSTERED (TeamId),
    CONSTRAINT FK_Team_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Department (DepartmentId),
    CONSTRAINT UQ_Team_Department_Name UNIQUE (DepartmentId, Name)
);
GO

CREATE TABLE dbo.Role
(
    RoleId           INT IDENTITY(1,1) NOT NULL,
    Name             NVARCHAR(100)     NOT NULL,
    Description      NVARCHAR(300)     NULL,
    IsSystemDefault  BIT               NOT NULL CONSTRAINT DF_Role_IsSystemDefault DEFAULT (0),
    IsActive         BIT               NOT NULL CONSTRAINT DF_Role_IsActive DEFAULT (1),
    CreatedBy        INT               NULL,
    CreatedDate      DATETIME2         NOT NULL CONSTRAINT DF_Role_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy       INT               NULL,
    ModifiedDate     DATETIME2         NULL,
    CONSTRAINT PK_Role PRIMARY KEY CLUSTERED (RoleId),
    CONSTRAINT UQ_Role_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.Permission
(
    PermissionId  INT IDENTITY(1,1) NOT NULL,
    Code          NVARCHAR(100)     NOT NULL,
    Module        NVARCHAR(100)     NOT NULL,
    Description   NVARCHAR(300)     NULL,
    CONSTRAINT PK_Permission PRIMARY KEY CLUSTERED (PermissionId),
    CONSTRAINT UQ_Permission_Code UNIQUE (Code)
);
GO

CREATE TABLE dbo.RolePermission
(
    RoleId        INT NOT NULL,
    PermissionId  INT NOT NULL,
    CONSTRAINT PK_RolePermission PRIMARY KEY CLUSTERED (RoleId, PermissionId),
    CONSTRAINT FK_RolePermission_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId),
    CONSTRAINT FK_RolePermission_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.Permission (PermissionId)
);
GO

CREATE TABLE dbo.[User]
(
    UserId             INT IDENTITY(1,1) NOT NULL,
    CompanyId          INT               NOT NULL,
    DepartmentId       INT               NULL,
    TeamId             INT               NULL,
    FirstName          NVARCHAR(100)     NOT NULL,
    LastName           NVARCHAR(100)     NOT NULL,
    Email              NVARCHAR(256)     NOT NULL,
    PasswordHash       NVARCHAR(300)     NOT NULL,
    JobTitle           NVARCHAR(150)     NULL,
    PhotoUrl           NVARCHAR(500)     NULL,
    TimeZone           NVARCHAR(100)     NOT NULL CONSTRAINT DF_User_TimeZone DEFAULT ('UTC'),
    WorkScheduleJson   NVARCHAR(MAX)     NULL,
    ProfileColor       CHAR(7)           NULL,
    IsActive           BIT               NOT NULL CONSTRAINT DF_User_IsActive DEFAULT (1),
    CreatedBy          INT               NOT NULL,
    CreatedDate        DATETIME2         NOT NULL CONSTRAINT DF_User_CreatedDate DEFAULT (SYSUTCDATETIME()),
    ModifiedBy         INT               NULL,
    ModifiedDate       DATETIME2         NULL,
    IsDeleted          BIT               NOT NULL CONSTRAINT DF_User_IsDeleted DEFAULT (0),
    CONSTRAINT PK_User PRIMARY KEY CLUSTERED (UserId),
    CONSTRAINT FK_User_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT FK_User_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Department (DepartmentId),
    CONSTRAINT FK_User_Team FOREIGN KEY (TeamId) REFERENCES dbo.Team (TeamId),
    CONSTRAINT UQ_User_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.UserRole
(
    UserId     INT NOT NULL,
    RoleId     INT NOT NULL,
    CompanyId  INT NOT NULL,
    CONSTRAINT PK_UserRole PRIMARY KEY CLUSTERED (UserId, RoleId, CompanyId),
    CONSTRAINT FK_UserRole_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId),
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId),
    CONSTRAINT FK_UserRole_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId)
);
GO

CREATE TABLE dbo.RefreshToken
(
    RefreshTokenId    INT IDENTITY(1,1) NOT NULL,
    UserId            INT               NOT NULL,
    Token             NVARCHAR(300)     NOT NULL,
    ExpiresAt         DATETIME2         NOT NULL,
    CreatedAt         DATETIME2         NOT NULL CONSTRAINT DF_RefreshToken_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedByIp       NVARCHAR(64)      NULL,
    RevokedAt         DATETIME2         NULL,
    RevokedByIp       NVARCHAR(64)      NULL,
    ReplacedByToken   NVARCHAR(300)     NULL,
    CONSTRAINT PK_RefreshToken PRIMARY KEY CLUSTERED (RefreshTokenId),
    CONSTRAINT FK_RefreshToken_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId),
    CONSTRAINT UQ_RefreshToken_Token UNIQUE (Token)
);
GO

CREATE NONCLUSTERED INDEX IX_Department_CompanyId ON dbo.Department (CompanyId);
CREATE NONCLUSTERED INDEX IX_Team_DepartmentId ON dbo.Team (DepartmentId);
CREATE NONCLUSTERED INDEX IX_User_CompanyId ON dbo.[User] (CompanyId);
CREATE NONCLUSTERED INDEX IX_User_DepartmentId ON dbo.[User] (DepartmentId);
CREATE NONCLUSTERED INDEX IX_User_TeamId ON dbo.[User] (TeamId);
CREATE NONCLUSTERED INDEX IX_UserRole_UserId ON dbo.UserRole (UserId);
CREATE NONCLUSTERED INDEX IX_RolePermission_PermissionId ON dbo.RolePermission (PermissionId);
CREATE NONCLUSTERED INDEX IX_RefreshToken_UserId ON dbo.RefreshToken (UserId);
GO
