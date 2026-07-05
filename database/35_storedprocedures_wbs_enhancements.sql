-- Modulo 3 (WBS): usuarios de un equipo especifico, para restringir a quien
-- puede asignarse como participante de una actividad raiz cuando quien la
-- edita es un Empleado normal (solo su propio equipo, incluyendo a su lider,
-- ya que el lider pertenece al mismo TeamId).
CREATE OR ALTER PROCEDURE dbo.SP_USER_GET_BY_TEAM
    @P_TEAM_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId AS USER_ID, CompanyId AS COMPANY_ID, DepartmentId AS DEPARTMENT_ID, TeamId AS TEAM_ID,
           FirstName AS FIRST_NAME, LastName AS LAST_NAME, Email AS EMAIL, JobTitle AS JOB_TITLE,
           PhotoUrl AS PHOTO_URL, TimeZone AS TIME_ZONE, WorkScheduleJson AS WORK_SCHEDULE_JSON,
           ProfileColor AS PROFILE_COLOR, IsActive AS IS_ACTIVE, CreatedBy AS CREATED_BY,
           CreatedDate AS CREATED_DATE, ModifiedBy AS MODIFIED_BY, ModifiedDate AS MODIFIED_DATE
    FROM dbo.[User]
    WHERE TeamId = @P_TEAM_ID AND IsDeleted = 0
    ORDER BY FirstName, LastName;
END
GO
