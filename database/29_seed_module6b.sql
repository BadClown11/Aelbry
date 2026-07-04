/*
    Modulo 6 (parte 3) - Seed: permisos de Documentos y Archivos.
*/

USE AelbryDb;
GO

INSERT INTO dbo.Permission (Code, Module, Description) VALUES
    ('DOCUMENTS_VIEW', 'DOCUMENTS', 'Ver documentos del proyecto'),
    ('DOCUMENTS_MANAGE', 'DOCUMENTS', 'Crear y editar documentos (nuevas versiones)'),
    ('FILES_VIEW', 'DOCUMENTS', 'Ver y descargar archivos adjuntos'),
    ('FILES_MANAGE', 'DOCUMENTS', 'Subir, organizar en carpetas y eliminar archivos adjuntos');
GO

-- Admin y Supervisor administran todo
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name IN ('Admin', 'Supervisor')
  AND p.Code IN ('DOCUMENTS_VIEW', 'DOCUMENTS_MANAGE', 'FILES_VIEW', 'FILES_MANAGE')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Lider y Empleado pueden ver y colaborar (crear/editar documentos, subir archivos)
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name IN ('Lider', 'Empleado')
  AND p.Code IN ('DOCUMENTS_VIEW', 'DOCUMENTS_MANAGE', 'FILES_VIEW', 'FILES_MANAGE')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO

-- Invitado: solo lectura
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Role r
CROSS JOIN dbo.Permission p
WHERE r.Name = 'Invitado'
  AND p.Code IN ('DOCUMENTS_VIEW', 'FILES_VIEW')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermission rp WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId);
GO
