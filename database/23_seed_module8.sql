/*
    Modulo 8 (parte 1) - Seed: permiso de reportes/KPIs (solo roles de gestion).
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('REPORTS_VIEW', 'REPORTS', 'Ver reportes, KPIs y graficas de productividad');
GO

INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name IN ('Admin', 'Supervisor', 'Lider')
  AND p.Code = 'REPORTS_VIEW'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
