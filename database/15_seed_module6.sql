/*
    Modulo 6 (parte 1) - Seed: permisos de Time Tracking.
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('TIME_ENTRIES_MANAGE', 'TIME_TRACKING', 'Registrar y editar el propio tiempo trabajado (cronometro y manual)'),
    ('TIME_ENTRIES_VIEW', 'TIME_TRACKING', 'Ver el tiempo registrado por otros usuarios');
GO

-- Todos los roles operativos pueden registrar su propio tiempo
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name IN ('Admin', 'Supervisor', 'Lider', 'Empleado')
  AND p.Code = 'TIME_ENTRIES_MANAGE'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Solo roles de gestion pueden ver el tiempo de otros (reportes de equipo)
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name IN ('Admin', 'Supervisor', 'Lider')
  AND p.Code = 'TIME_ENTRIES_VIEW'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
