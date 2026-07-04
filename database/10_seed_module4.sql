/*
    Modulo 4 (parte 1) - Seed: permiso de uso del asistente IA.
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('AI_ASSISTANT_USE', 'AI_ASSISTANT', 'Usar el asistente IA para sugerir y crear proyectos/actividades');
GO

-- Admin, Supervisor y Lider pueden usar el asistente IA
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name IN ('Admin', 'Supervisor', 'Lider')
  AND p.Code = 'AI_ASSISTANT_USE'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
