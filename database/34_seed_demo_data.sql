/*
    Datos de ejemplo (demo) para ver la app funcionando con contenido real:
    departamentos, equipos, usuarios, etiquetas, proyectos y actividades (WBS)
    para las empresas "Aelbry Demo" y "Foxconn".

    Todos los usuarios nuevos comparten la misma contrasena que el admin
    sembrado en 02_seed_module1.sql: Admin#12345
*/

USE AelbryDb;
GO

DECLARE @PasswordHash NVARCHAR(300) = '$2b$12$Iktm8xbjw333i2YlGUZhQuNJG6F5rUU3TWKu8lhv1Fu.pS60cYis6'; -- Admin#12345
DECLARE @AdminUserId INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'admin@aelbry.local');

--------------------------------------------------------------------------------------------
-- ================================  AELBRY DEMO (CompanyId 1)  =============================
--------------------------------------------------------------------------------------------
DECLARE @C1 INT = (SELECT CompanyId FROM dbo.Company WHERE Name = 'Aelbry Demo');

INSERT INTO dbo.Department (CompanyId, Name, CreatedBy) VALUES
    (@C1, 'Tecnologia', @AdminUserId),
    (@C1, 'Operaciones', @AdminUserId);

DECLARE @C1DeptTech INT = (SELECT DepartmentId FROM dbo.Department WHERE CompanyId = @C1 AND Name = 'Tecnologia');
DECLARE @C1DeptOps  INT = (SELECT DepartmentId FROM dbo.Department WHERE CompanyId = @C1 AND Name = 'Operaciones');

INSERT INTO dbo.Team (DepartmentId, Name, CreatedBy) VALUES
    (@C1DeptTech, 'Desarrollo', @AdminUserId),
    (@C1DeptOps, 'Soporte', @AdminUserId);

DECLARE @C1TeamDev     INT = (SELECT TeamId FROM dbo.Team WHERE DepartmentId = @C1DeptTech AND Name = 'Desarrollo');
DECLARE @C1TeamSupport INT = (SELECT TeamId FROM dbo.Team WHERE DepartmentId = @C1DeptOps AND Name = 'Soporte');

INSERT INTO dbo.[User] (CompanyId, DepartmentId, TeamId, FirstName, LastName, Email, PasswordHash, JobTitle, ProfileColor, CreatedBy) VALUES
    (@C1, @C1DeptTech, @C1TeamDev, 'Maria', 'Lopez',   'maria.lopez@aelbry.local',   @PasswordHash, 'Supervisora de TI',    '#4C6EF5', @AdminUserId),
    (@C1, @C1DeptTech, @C1TeamDev, 'Carlos', 'Ramirez', 'carlos.ramirez@aelbry.local', @PasswordHash, 'Lider de Desarrollo',  '#2F9E44', @AdminUserId),
    (@C1, @C1DeptTech, @C1TeamDev, 'Ana', 'Torres',    'ana.torres@aelbry.local',    @PasswordHash, 'Disenadora UX/UI',     '#F59F00', @AdminUserId),
    (@C1, @C1DeptTech, @C1TeamDev, 'Luis', 'Mendez',    'luis.mendez@aelbry.local',   @PasswordHash, 'Desarrollador Backend','#7048E8', @AdminUserId),
    (@C1, @C1DeptOps, @C1TeamSupport, 'Sofia', 'Castro', 'sofia.castro@aelbry.local', @PasswordHash, 'Analista de Soporte',  '#E03131', @AdminUserId),
    (@C1, @C1DeptOps, @C1TeamSupport, 'Jorge', 'Vega',   'jorge.vega@aelbry.local',   @PasswordHash, 'Analista de Soporte',  '#12B886', @AdminUserId);

DECLARE @C1Maria  INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'maria.lopez@aelbry.local');
DECLARE @C1Carlos INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'carlos.ramirez@aelbry.local');
DECLARE @C1Ana    INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'ana.torres@aelbry.local');
DECLARE @C1Luis   INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'luis.mendez@aelbry.local');
DECLARE @C1Sofia  INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'sofia.castro@aelbry.local');
DECLARE @C1Jorge  INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'jorge.vega@aelbry.local');

INSERT INTO dbo.UserRole (UserId, RoleId, CompanyId)
SELECT u.UserId, r.RoleId, @C1
FROM (VALUES (@C1Maria, 'Supervisor'), (@C1Carlos, 'Lider'), (@C1Ana, 'Empleado'), (@C1Luis, 'Empleado'), (@C1Sofia, 'Empleado'), (@C1Jorge, 'Empleado')) AS u(UserId, RoleName)
JOIN dbo.Role r ON r.Name = u.RoleName;

INSERT INTO dbo.Tag (CompanyId, Name, ColorHex, CreatedBy) VALUES
    (@C1, 'Urgente', '#E03131', @AdminUserId),
    (@C1, 'Backend', '#4C6EF5', @AdminUserId);

DECLARE @C1TagUrgente INT = (SELECT TagId FROM dbo.Tag WHERE CompanyId = @C1 AND Name = 'Urgente');
DECLARE @C1TagBackend INT = (SELECT TagId FROM dbo.Tag WHERE CompanyId = @C1 AND Name = 'Backend');

DECLARE @C1StatusPlan INT = (SELECT ProjectStatusId FROM dbo.ProjectStatus WHERE CompanyId = @C1 AND Name = 'Planeacion');
DECLARE @C1StatusProg INT = (SELECT ProjectStatusId FROM dbo.ProjectStatus WHERE CompanyId = @C1 AND Name = 'En Progreso');

INSERT INTO dbo.Project (CompanyId, Code, Name, ClientName, ProjectStatusId, Priority, StartDate, EndDate, EstimatedHours, ProjectManagerId, CreatedBy) VALUES
    (@C1, 'WEB-01', 'Portal de Clientes', 'Aelbry Demo Corp', @C1StatusProg, 3, '2026-05-01', '2026-08-15', 480, @C1Maria, @AdminUserId),
    (@C1, 'MOB-01', 'App Movil Delivery', 'Aelbry Demo Corp', @C1StatusPlan, 2, '2026-07-01', '2026-11-30', 620, @C1Carlos, @AdminUserId);

DECLARE @C1ProjWeb INT = (SELECT ProjectId FROM dbo.Project WHERE CompanyId = @C1 AND Code = 'WEB-01');
DECLARE @C1ProjMob INT = (SELECT ProjectId FROM dbo.Project WHERE CompanyId = @C1 AND Code = 'MOB-01');

INSERT INTO dbo.ProjectMember (ProjectId, UserId) VALUES
    (@C1ProjWeb, @C1Maria), (@C1ProjWeb, @C1Carlos), (@C1ProjWeb, @C1Ana), (@C1ProjWeb, @C1Luis),
    (@C1ProjMob, @C1Carlos), (@C1ProjMob, @C1Sofia), (@C1ProjMob, @C1Jorge);

INSERT INTO dbo.ProjectTag (ProjectId, TagId) VALUES (@C1ProjWeb, @C1TagBackend);

-- WBS del Portal de Clientes (WEB-01)
INSERT INTO dbo.Activity (ProjectId, Code, Name, Category, Status, Priority, ResponsibleUserId, EstimatedStartDate, EstimatedEndDate, ActualStartDate, ActualEndDate, Weight, EstimatedHours, WorkedHours, ProgressPercentage, Sequence, CreatedBy) VALUES
    (@C1ProjWeb, 'WEB-01-1', 'Diseno UX/UI', 'Diseno', 4, 2, @C1Carlos, '2026-05-01', '2026-05-20', '2026-05-01', '2026-05-18', 2, 80, 76, 100, 1, @AdminUserId),
    (@C1ProjWeb, 'WEB-01-2', 'Desarrollo Backend', 'Backend', 2, 3, @C1Carlos, '2026-05-21', '2026-07-10', '2026-05-21', NULL, 3, 200, 110, 55, 2, @AdminUserId),
    (@C1ProjWeb, 'WEB-01-3', 'QA y Lanzamiento', 'QA', 1, 3, @C1Maria, '2026-07-11', '2026-08-15', NULL, NULL, 2, 120, 0, 0, 3, @AdminUserId);

DECLARE @C1ActDesign  INT = (SELECT ActivityId FROM dbo.Activity WHERE ProjectId = @C1ProjWeb AND Code = 'WEB-01-1');
DECLARE @C1ActBackend INT = (SELECT ActivityId FROM dbo.Activity WHERE ProjectId = @C1ProjWeb AND Code = 'WEB-01-2');

INSERT INTO dbo.Activity (ProjectId, ParentActivityId, Code, Name, Category, Status, Priority, ResponsibleUserId, EstimatedStartDate, EstimatedEndDate, ActualStartDate, ActualEndDate, Weight, EstimatedHours, WorkedHours, ProgressPercentage, Sequence, CreatedBy) VALUES
    (@C1ProjWeb, @C1ActDesign, 'WEB-01-1.1', 'Wireframes', 'Diseno', 4, 2, @C1Ana, '2026-05-01', '2026-05-10', '2026-05-01', '2026-05-09', 1, 30, 28, 100, 1, @AdminUserId),
    (@C1ProjWeb, @C1ActDesign, 'WEB-01-1.2', 'Prototipo interactivo', 'Diseno', 4, 2, @C1Ana, '2026-05-11', '2026-05-20', '2026-05-11', '2026-05-18', 1, 50, 48, 100, 2, @AdminUserId),
    (@C1ProjWeb, @C1ActBackend, 'WEB-01-2.1', 'API de autenticacion', 'Backend', 4, 3, @C1Luis, '2026-05-21', '2026-06-05', '2026-05-21', '2026-06-03', 1, 70, 70, 100, 1, @AdminUserId),
    (@C1ProjWeb, @C1ActBackend, 'WEB-01-2.2', 'API de pagos', 'Backend', 2, 4, @C1Luis, '2026-06-06', '2026-07-10', '2026-06-06', NULL, 2, 130, 40, 30, 2, @AdminUserId);

INSERT INTO dbo.ActivityTag (ActivityId, TagId) VALUES (@C1ActBackend, @C1TagBackend);

-- WBS de la App Movil Delivery (MOB-01)
INSERT INTO dbo.Activity (ProjectId, Code, Name, Category, Status, Priority, ResponsibleUserId, EstimatedStartDate, EstimatedEndDate, ActualStartDate, ActualEndDate, Weight, EstimatedHours, WorkedHours, ProgressPercentage, Sequence, CreatedBy) VALUES
    (@C1ProjMob, 'MOB-01-1', 'Investigacion de mercado', 'Analisis', 4, 2, @C1Sofia, '2026-07-01', '2026-07-15', '2026-07-01', '2026-07-14', 1, 40, 38, 100, 1, @AdminUserId),
    (@C1ProjMob, 'MOB-01-2', 'Definicion de alcance', 'Analisis', 2, 3, @C1Jorge, '2026-07-16', '2026-08-05', '2026-07-16', NULL, 2, 60, 22, 40, 2, @AdminUserId),
    (@C1ProjMob, 'MOB-01-3', 'Desarrollo MVP', 'Backend', 1, 3, @C1Carlos, '2026-08-06', '2026-11-30', NULL, NULL, 3, 400, 0, 0, 3, @AdminUserId);
GO

--------------------------------------------------------------------------------------------
-- ===================================  FOXCONN (CompanyId 2)  ==============================
--------------------------------------------------------------------------------------------
DECLARE @PasswordHash NVARCHAR(300) = '$2b$12$Iktm8xbjw333i2YlGUZhQuNJG6F5rUU3TWKu8lhv1Fu.pS60cYis6'; -- Admin#12345
DECLARE @AdminUserId INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'admin@aelbry.local');
DECLARE @C2 INT = (SELECT CompanyId FROM dbo.Company WHERE Name = 'Foxconn');

INSERT INTO dbo.Department (CompanyId, Name, CreatedBy) VALUES
    (@C2, 'Manufactura', @AdminUserId),
    (@C2, 'TI', @AdminUserId);

DECLARE @C2DeptMfg INT = (SELECT DepartmentId FROM dbo.Department WHERE CompanyId = @C2 AND Name = 'Manufactura');
DECLARE @C2DeptIt  INT = (SELECT DepartmentId FROM dbo.Department WHERE CompanyId = @C2 AND Name = 'TI');

INSERT INTO dbo.Team (DepartmentId, Name, CreatedBy) VALUES
    (@C2DeptMfg, 'Linea de Produccion', @AdminUserId),
    (@C2DeptIt, 'Infraestructura', @AdminUserId);

DECLARE @C2TeamLine INT = (SELECT TeamId FROM dbo.Team WHERE DepartmentId = @C2DeptMfg AND Name = 'Linea de Produccion');
DECLARE @C2TeamInfra INT = (SELECT TeamId FROM dbo.Team WHERE DepartmentId = @C2DeptIt AND Name = 'Infraestructura');

INSERT INTO dbo.[User] (CompanyId, DepartmentId, TeamId, FirstName, LastName, Email, PasswordHash, JobTitle, ProfileColor, CreatedBy) VALUES
    (@C2, @C2DeptIt, @C2TeamInfra, 'Patricia', 'Gomez',  'patricia.gomez@foxconn.local', @PasswordHash, 'Supervisora de TI',        '#4C6EF5', @AdminUserId),
    (@C2, @C2DeptMfg, @C2TeamLine, 'Roberto', 'Diaz',    'roberto.diaz@foxconn.local',   @PasswordHash, 'Lider de Produccion',      '#2F9E44', @AdminUserId),
    (@C2, @C2DeptMfg, @C2TeamLine, 'Elena', 'Morales',   'elena.morales@foxconn.local',  @PasswordHash, 'Analista de Produccion',   '#F59F00', @AdminUserId),
    (@C2, @C2DeptMfg, @C2TeamLine, 'Fernando', 'Ruiz',   'fernando.ruiz@foxconn.local',  @PasswordHash, 'Tecnico de Produccion',    '#7048E8', @AdminUserId),
    (@C2, @C2DeptIt, @C2TeamInfra, 'Claudia', N'Peña',    'claudia.pena@foxconn.local',   @PasswordHash, 'Ingeniera de Infraestructura', '#E03131', @AdminUserId),
    (@C2, @C2DeptIt, @C2TeamInfra, 'Miguel', 'Santos',   'miguel.santos@foxconn.local',  @PasswordHash, 'Administrador de Sistemas','#12B886', @AdminUserId);

DECLARE @C2Patricia INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'patricia.gomez@foxconn.local');
DECLARE @C2Roberto  INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'roberto.diaz@foxconn.local');
DECLARE @C2Elena    INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'elena.morales@foxconn.local');
DECLARE @C2Fernando INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'fernando.ruiz@foxconn.local');
DECLARE @C2Claudia  INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'claudia.pena@foxconn.local');
DECLARE @C2Miguel   INT = (SELECT UserId FROM dbo.[User] WHERE Email = 'miguel.santos@foxconn.local');

INSERT INTO dbo.UserRole (UserId, RoleId, CompanyId)
SELECT u.UserId, r.RoleId, @C2
FROM (VALUES (@C2Patricia, 'Supervisor'), (@C2Roberto, 'Lider'), (@C2Elena, 'Empleado'), (@C2Fernando, 'Empleado'), (@C2Claudia, 'Empleado'), (@C2Miguel, 'Empleado')) AS u(UserId, RoleName)
JOIN dbo.Role r ON r.Name = u.RoleName;

INSERT INTO dbo.Tag (CompanyId, Name, ColorHex, CreatedBy) VALUES
    (@C2, 'Critico', '#E03131', @AdminUserId),
    (@C2, 'Infraestructura', '#4C6EF5', @AdminUserId);

DECLARE @C2TagCritico  INT = (SELECT TagId FROM dbo.Tag WHERE CompanyId = @C2 AND Name = 'Critico');
DECLARE @C2TagInfra    INT = (SELECT TagId FROM dbo.Tag WHERE CompanyId = @C2 AND Name = 'Infraestructura');

-- Foxconn no tenia catalogo de estados de proyecto propio (solo Aelbry Demo lo trajo en el seed base).
INSERT INTO dbo.ProjectStatus (CompanyId, Name, ColorHex, Sequence, IsFinal, CreatedBy) VALUES
    (@C2, 'Planeacion', '#868E96', 1, 0, @AdminUserId),
    (@C2, 'En Progreso', '#4C6EF5', 2, 0, @AdminUserId),
    (@C2, 'En Revision', '#F59F00', 3, 0, @AdminUserId),
    (@C2, 'Completado', '#2F9E44', 4, 1, @AdminUserId),
    (@C2, 'Cancelado', '#E03131', 5, 1, @AdminUserId);

DECLARE @C2StatusPlan INT = (SELECT ProjectStatusId FROM dbo.ProjectStatus WHERE CompanyId = @C2 AND Name = 'Planeacion');
DECLARE @C2StatusProg INT = (SELECT ProjectStatusId FROM dbo.ProjectStatus WHERE CompanyId = @C2 AND Name = 'En Progreso');

INSERT INTO dbo.Project (CompanyId, Code, Name, ClientName, ProjectStatusId, Priority, StartDate, EndDate, EstimatedHours, ProjectManagerId, CreatedBy) VALUES
    (@C2, 'ERP-01', 'Implementacion ERP', 'Foxconn Corporativo', @C2StatusProg, 4, '2026-04-01', '2026-09-30', 900, @C2Patricia, @AdminUserId),
    (@C2, 'AUT-01', 'Automatizacion de Linea 3', 'Foxconn Planta Norte', @C2StatusPlan, 3, '2026-08-01', '2027-01-31', 750, @C2Roberto, @AdminUserId);

DECLARE @C2ProjErp INT = (SELECT ProjectId FROM dbo.Project WHERE CompanyId = @C2 AND Code = 'ERP-01');
DECLARE @C2ProjAut INT = (SELECT ProjectId FROM dbo.Project WHERE CompanyId = @C2 AND Code = 'AUT-01');

INSERT INTO dbo.ProjectMember (ProjectId, UserId) VALUES
    (@C2ProjErp, @C2Patricia), (@C2ProjErp, @C2Claudia), (@C2ProjErp, @C2Miguel),
    (@C2ProjAut, @C2Roberto), (@C2ProjAut, @C2Elena), (@C2ProjAut, @C2Fernando);

INSERT INTO dbo.ProjectTag (ProjectId, TagId) VALUES (@C2ProjErp, @C2TagCritico);

-- WBS de Implementacion ERP (ERP-01)
INSERT INTO dbo.Activity (ProjectId, Code, Name, Category, Status, Priority, ResponsibleUserId, EstimatedStartDate, EstimatedEndDate, ActualStartDate, ActualEndDate, Weight, EstimatedHours, WorkedHours, ProgressPercentage, Sequence, CreatedBy) VALUES
    (@C2ProjErp, 'ERP-01-1', 'Levantamiento de requerimientos', 'Analisis', 4, 3, @C2Patricia, '2026-04-01', '2026-04-20', '2026-04-01', '2026-04-18', 1, 100, 96, 100, 1, @AdminUserId),
    (@C2ProjErp, 'ERP-01-2', 'Configuracion de modulos', 'Infraestructura', 2, 4, @C2Miguel, '2026-04-21', '2026-07-15', '2026-04-21', NULL, 3, 350, 190, 54, 2, @AdminUserId),
    (@C2ProjErp, 'ERP-01-3', 'Migracion de datos y capacitacion', 'Infraestructura', 1, 3, @C2Claudia, '2026-07-16', '2026-09-30', NULL, NULL, 2, 250, 0, 0, 3, @AdminUserId);

DECLARE @C2ActConfig INT = (SELECT ActivityId FROM dbo.Activity WHERE ProjectId = @C2ProjErp AND Code = 'ERP-01-2');

INSERT INTO dbo.Activity (ProjectId, ParentActivityId, Code, Name, Category, Status, Priority, ResponsibleUserId, EstimatedStartDate, EstimatedEndDate, ActualStartDate, ActualEndDate, Weight, EstimatedHours, WorkedHours, ProgressPercentage, Sequence, CreatedBy) VALUES
    (@C2ProjErp, @C2ActConfig, 'ERP-01-2.1', 'Modulo de inventario', 'Infraestructura', 4, 3, @C2Miguel, '2026-04-21', '2026-05-31', '2026-04-21', '2026-05-28', 1, 150, 148, 100, 1, @AdminUserId),
    (@C2ProjErp, @C2ActConfig, 'ERP-01-2.2', 'Modulo de finanzas', 'Infraestructura', 2, 4, @C2Miguel, '2026-06-01', '2026-07-15', '2026-06-01', NULL, 2, 200, 42, 21, 2, @AdminUserId);

INSERT INTO dbo.ActivityTag (ActivityId, TagId) VALUES (@C2ActConfig, @C2TagInfra);

-- WBS de Automatizacion de Linea 3 (AUT-01)
INSERT INTO dbo.Activity (ProjectId, Code, Name, Category, Status, Priority, ResponsibleUserId, EstimatedStartDate, EstimatedEndDate, ActualStartDate, ActualEndDate, Weight, EstimatedHours, WorkedHours, ProgressPercentage, Sequence, CreatedBy) VALUES
    (@C2ProjAut, 'AUT-01-1', 'Diagnostico de linea actual', 'Analisis', 4, 3, @C2Roberto, '2026-08-01', '2026-08-15', '2026-08-01', '2026-08-13', 1, 60, 58, 100, 1, @AdminUserId),
    (@C2ProjAut, 'AUT-01-2', 'Diseno de celda robotica', 'Ingenieria', 1, 3, @C2Elena, '2026-08-16', '2026-10-15', NULL, NULL, 3, 300, 0, 0, 2, @AdminUserId),
    (@C2ProjAut, 'AUT-01-3', 'Instalacion y pruebas', 'Ingenieria', 1, 4, @C2Fernando, '2026-10-16', '2027-01-31', NULL, NULL, 2, 390, 0, 0, 3, @AdminUserId);
GO
