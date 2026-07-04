/*
    Modulo 7 (parte 2) - Seed: permiso de notificaciones (todos los roles, es basico).
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('NOTIFICATIONS_USE', 'NOTIFICATIONS', 'Ver y gestionar las propias notificaciones');
GO

INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE p.Code = 'NOTIFICATIONS_USE'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
