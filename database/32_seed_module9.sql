/*
    Modulo 9 - Seed: permiso de auditoria (solo Admin).
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('AUDIT_VIEW', 'AUDIT', 'Ver la bitacora global de auditoria de la empresa');
GO

INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name = 'Admin'
  AND p.Code = 'AUDIT_VIEW'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
