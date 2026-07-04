/*
    Modulo 7 (parte 1) - Seed: permisos del motor de automatizaciones.
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('AUTOMATION_RULES_VIEW', 'AUTOMATION', 'Ver reglas de automatizacion y su bitacora de ejecucion'),
    ('AUTOMATION_RULES_MANAGE', 'AUTOMATION', 'Crear, editar y eliminar reglas de automatizacion');
GO

-- Admin y Supervisor administran reglas
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name IN ('Admin', 'Supervisor')
  AND p.Code IN ('AUTOMATION_RULES_VIEW', 'AUTOMATION_RULES_MANAGE')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Lider solo puede ver (para entender por que cambio algo automaticamente)
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name = 'Lider'
  AND p.Code = 'AUTOMATION_RULES_VIEW'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
