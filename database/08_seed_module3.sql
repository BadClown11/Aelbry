/*
    Modulo 3 - Seed: permisos granulares y asignacion a roles predeterminados.
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('ACTIVITIES_VIEW', 'ACTIVITIES', 'Ver actividades y subactividades'),
    ('ACTIVITIES_CREATE', 'ACTIVITIES', 'Crear actividades y subactividades'),
    ('ACTIVITIES_EDIT', 'ACTIVITIES', 'Editar actividades'),
    ('ACTIVITIES_DELETE', 'ACTIVITIES', 'Eliminar (dar de baja) actividades'),
    ('ACTIVITY_PARTICIPANTS_MANAGE', 'ACTIVITIES', 'Asignar responsable y participantes a una actividad'),
    ('ACTIVITY_CHECKLISTS_MANAGE', 'ACTIVITIES', 'Administrar los checklists de una actividad'),
    ('ACTIVITY_DEPENDENCIES_MANAGE', 'ACTIVITIES', 'Administrar dependencias/bloqueos entre actividades');
GO

-- Admin: todos los permisos nuevos
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name = 'Admin'
  AND p.Code IN ('ACTIVITIES_VIEW', 'ACTIVITIES_CREATE', 'ACTIVITIES_EDIT', 'ACTIVITIES_DELETE',
                 'ACTIVITY_PARTICIPANTS_MANAGE', 'ACTIVITY_CHECKLISTS_MANAGE', 'ACTIVITY_DEPENDENCIES_MANAGE')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Supervisor: gestion operativa completa de actividades
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code IN ('ACTIVITIES_VIEW', 'ACTIVITIES_CREATE', 'ACTIVITIES_EDIT',
                                    'ACTIVITY_PARTICIPANTS_MANAGE', 'ACTIVITY_CHECKLISTS_MANAGE', 'ACTIVITY_DEPENDENCIES_MANAGE')
WHERE r.Name = 'Supervisor'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Lider: gestiona las actividades de su equipo
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code IN ('ACTIVITIES_VIEW', 'ACTIVITIES_CREATE', 'ACTIVITIES_EDIT',
                                    'ACTIVITY_PARTICIPANTS_MANAGE', 'ACTIVITY_CHECKLISTS_MANAGE')
WHERE r.Name = 'Lider'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Empleado: ve y actualiza su propio checklist de avance
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code IN ('ACTIVITIES_VIEW', 'ACTIVITY_CHECKLISTS_MANAGE')
WHERE r.Name = 'Empleado'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Invitado: solo lectura
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
JOIN dbo.Permission p ON p.Code = 'ACTIVITIES_VIEW'
WHERE r.Name = 'Invitado'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
