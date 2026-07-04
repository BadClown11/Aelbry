/*
    Modulo 8 (parte 1) - Stored Procedures de Reportes & KPIs.
    Un unico SP de lectura, filtrable, que alimenta el reporte semanal, el resumen de KPIs,
    la grafica de avance por usuario, la distribucion por prioridad y el burndown; toda la
    agregacion se hace en C# (ReportBL) sobre este mismo dataset para no duplicar consultas.
*/

USE AelbryDb;
GO

CREATE OR ALTER PROCEDURE dbo.SP_REPORT_GET_ACTIVITIES
    @P_COMPANY_ID INT,
    @P_PROJECT_ID INT = NULL,
    @P_RESPONSIBLE_USER_ID INT = NULL,
    @P_TEAM_ID INT = NULL,
    @P_DEPARTMENT_ID INT = NULL,
    @P_DUE_START_DATE DATE = NULL,
    @P_DUE_END_DATE DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.ActivityId AS ACTIVITY_ID, a.Code AS CODE, a.Name AS NAME, a.Description AS DESCRIPTION,
           a.ProjectId AS PROJECT_ID, p.Name AS PROJECT_NAME,
           a.ResponsibleUserId AS RESPONSIBLE_USER_ID, u.FirstName + ' ' + u.LastName AS RESPONSIBLE_NAME,
           u.TeamId AS TEAM_ID, t.Name AS TEAM_NAME, u.DepartmentId AS DEPARTMENT_ID, dept.Name AS DEPARTMENT_NAME,
           a.Status AS STATUS, a.Priority AS PRIORITY,
           a.EstimatedStartDate AS ESTIMATED_START_DATE, a.EstimatedEndDate AS ESTIMATED_END_DATE,
           a.ActualStartDate AS ACTUAL_START_DATE, a.ActualEndDate AS ACTUAL_END_DATE,
           a.EstimatedHours AS ESTIMATED_HOURS, a.WorkedHours AS WORKED_HOURS, a.ProgressPercentage AS PROGRESS_PERCENTAGE
    FROM dbo.Activity a
    JOIN dbo.Project p ON p.ProjectId = a.ProjectId
    JOIN dbo.[User] u ON u.UserId = a.ResponsibleUserId
    LEFT JOIN dbo.Team t ON t.TeamId = u.TeamId
    LEFT JOIN dbo.Department dept ON dept.DepartmentId = u.DepartmentId
    WHERE p.CompanyId = @P_COMPANY_ID AND a.IsDeleted = 0 AND p.IsDeleted = 0
      AND (@P_PROJECT_ID IS NULL OR a.ProjectId = @P_PROJECT_ID)
      AND (@P_RESPONSIBLE_USER_ID IS NULL OR a.ResponsibleUserId = @P_RESPONSIBLE_USER_ID)
      AND (@P_TEAM_ID IS NULL OR u.TeamId = @P_TEAM_ID)
      AND (@P_DEPARTMENT_ID IS NULL OR u.DepartmentId = @P_DEPARTMENT_ID)
      AND (@P_DUE_START_DATE IS NULL OR a.EstimatedEndDate >= @P_DUE_START_DATE)
      AND (@P_DUE_END_DATE IS NULL OR a.EstimatedEndDate <= @P_DUE_END_DATE)
    ORDER BY a.EstimatedEndDate;
END
GO
