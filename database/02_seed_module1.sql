/*
    Modulo 1 - Seed inicial: catalogo de permisos, roles predeterminados,
    empresa demo y usuario administrador inicial.

    Usuario:    admin@aelbry.local
    Contrasena: Admin#12345   (hash BCrypt, factor de costo 12 -- cambiarla en el primer login)
*/

USE AelbryDb;
GO

-- Catalogo de permisos granulares del Modulo 1 (mas modulos agregaran su propio catalogo)
INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('COMPANY_MANAGE', 'IDENTITY', 'Administrar datos de la empresa'),
    ('DEPARTMENTS_MANAGE', 'IDENTITY', 'Administrar departamentos'),
    ('TEAMS_MANAGE', 'IDENTITY', 'Administrar equipos'),
    ('USERS_VIEW', 'IDENTITY', 'Ver usuarios'),
    ('USERS_CREATE', 'IDENTITY', 'Crear usuarios'),
    ('USERS_EDIT', 'IDENTITY', 'Editar usuarios'),
    ('USERS_DELETE', 'IDENTITY', 'Eliminar (dar de baja) usuarios'),
    ('ROLES_MANAGE', 'IDENTITY', 'Administrar roles, permisos y asignaciones');
GO

-- Roles predeterminados del sistema
INSERT INTO dbo.Role (Name, Description, IsSystemDefault, IsActive) VALUES
    ('Admin', 'Acceso total a la plataforma', 1, 1),
    ('Supervisor', 'Gestiona departamentos, equipos y usuarios de su area', 1, 1),
    ('Lider', 'Lidera equipos y coordina actividades', 1, 1),
    ('Empleado', 'Usuario operativo estandar', 1, 1),
    ('Invitado', 'Acceso de solo lectura limitado', 1, 1);
GO

-- Admin: todos los permisos
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name = 'Admin';
GO

-- Supervisor: gestion operativa, sin administracion de roles
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code IN ('DEPARTMENTS_MANAGE', 'TEAMS_MANAGE', 'USERS_VIEW', 'USERS_CREATE', 'USERS_EDIT')
WHERE r.Name = 'Supervisor';
GO

-- Lider: gestion de su equipo
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code IN ('TEAMS_MANAGE', 'USERS_VIEW')
WHERE r.Name = 'Lider';
GO

-- Empleado / Invitado: solo lectura de usuarios
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code = 'USERS_VIEW'
WHERE r.Name IN ('Empleado', 'Invitado');
GO

-- Empresa demo
INSERT INTO dbo.Company (Name, LegalTaxId, TimeZone, IsActive, CreatedBy)
VALUES ('Aelbry Demo', 'DEMO-000000', 'America/Mexico_City', 1, 0);
GO

DECLARE @CompanyId INT = (SELECT CompanyId FROM dbo.Company WHERE Name = 'Aelbry Demo');
DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Role WHERE Name = 'Admin');

INSERT INTO dbo.[User]
    (CompanyId, FirstName, LastName, Email, PasswordHash, JobTitle, TimeZone, ProfileColor, IsActive, CreatedBy)
VALUES
    (@CompanyId, 'Administrador', 'General', 'admin@aelbry.local',
     '$2b$12$Iktm8xbjw333i2YlGUZhQuNJG6F5rUU3TWKu8lhv1Fu.pS60cYis6',
     'Administrador de Plataforma', 'America/Mexico_City', '#4C6EF5', 1, 0);

DECLARE @AdminUserId INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'admin@aelbry.local');

INSERT INTO dbo.UserRole (UserId, RoleId, CompanyId) VALUES (@AdminUserId, @AdminRoleId, @CompanyId);
GO
