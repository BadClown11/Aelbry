/*
    Modulo 6 (parte 2) - Seed: permiso de uso del chat empresarial (todos los roles).
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('CHAT_USE', 'CHAT', 'Usar el chat empresarial (canales de proyecto y mensajes directos)');
GO

INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE p.Code = 'CHAT_USE'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
