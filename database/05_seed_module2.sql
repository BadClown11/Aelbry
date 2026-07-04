/*
    Modulo 2 - Seed: permisos granulares, asignacion a roles predeterminados
    y estados de proyecto por defecto para la empresa demo.
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('PROJECTS_VIEW', 'PROJECTS', 'Ver proyectos'),
    ('PROJECTS_CREATE', 'PROJECTS', 'Crear proyectos'),
    ('PROJECTS_EDIT', 'PROJECTS', 'Editar proyectos'),
    ('PROJECTS_DELETE', 'PROJECTS', 'Eliminar (dar de baja) proyectos'),
    ('PROJECT_MEMBERS_MANAGE', 'PROJECTS', 'Asignar responsable y miembros a un proyecto'),
    ('PROJECT_STATUSES_MANAGE', 'PROJECTS', 'Administrar el catalogo de estados/flujo de trabajo'),
    ('PROJECT_TAGS_MANAGE', 'PROJECTS', 'Administrar el catalogo de etiquetas'),
    ('PROJECT_TEMPLATES_MANAGE', 'PROJECTS', 'Administrar plantillas de proyecto');
GO

-- Admin: todos los permisos nuevos (ya hereda todos via CROSS JOIN en el seed del Modulo 1,
-- pero como esos permisos no existian en ese momento, se agregan explicitamente aqui).
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name = 'Admin'
  AND p.Code IN ('PROJECTS_VIEW', 'PROJECTS_CREATE', 'PROJECTS_EDIT', 'PROJECTS_DELETE',
                 'PROJECT_MEMBERS_MANAGE', 'PROJECT_STATUSES_MANAGE', 'PROJECT_TAGS_MANAGE', 'PROJECT_TEMPLATES_MANAGE')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Supervisor: gestion operativa completa de proyectos
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code IN ('PROJECTS_VIEW', 'PROJECTS_CREATE', 'PROJECTS_EDIT',
                                    'PROJECT_MEMBERS_MANAGE', 'PROJECT_TAGS_MANAGE', 'PROJECT_TEMPLATES_MANAGE')
WHERE r.Name = 'Supervisor'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Lider: ve y edita los proyectos de su equipo, gestiona miembros
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code IN ('PROJECTS_VIEW', 'PROJECTS_EDIT', 'PROJECT_MEMBERS_MANAGE')
WHERE r.Name = 'Lider'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Empleado / Invitado: solo lectura de proyectos
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code = 'PROJECTS_VIEW'
WHERE r.Name IN ('Empleado', 'Invitado')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Estados de proyecto por defecto (flujo de trabajo base) para la empresa demo.
DECLARE @CompanyId INT = (SELECT CompanyId FROM dbo.Company WHERE Name = 'Aelbry Demo');
DECLARE @AdminUserId INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'admin@aelbry.local');

INSERT INTO dbo.ProjectStatus (CompanyId, Name, ColorHex, Sequence, IsFinal, CreatedBy) VALUES
    (@CompanyId, 'Planeacion', '#868E96', 1, 0, @AdminUserId),
    (@CompanyId, 'En Progreso', '#4C6EF5', 2, 0, @AdminUserId),
    (@CompanyId, 'En Revision', '#F59F00', 3, 0, @AdminUserId),
    (@CompanyId, 'Completado', '#2F9E44', 4, 1, @AdminUserId),
    (@CompanyId, 'Cancelado', '#E03131', 5, 1, @AdminUserId);
GO
