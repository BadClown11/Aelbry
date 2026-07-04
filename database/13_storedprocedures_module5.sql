/*
    Modulo 5 - Stored Procedures livianos para las vistas operativas dinamicas.
    El tablero Kanban (drag&drop de estado) y el Gantt (arrastre de fechas) necesitan
    actualizar un par de columnas sin reenviar el registro completo de Activity.
*/

USE AelbryDb;
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_UPDATE_STATUS
    @P_ACTIVITY_ID INT,
    @P_STATUS TINYINT,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Activity
        SET Status = @P_STATUS, ModifiedBy = @P_MODIFIED_BY, ModifiedDate = SYSUTCDATETIME()
        WHERE ActivityId = @P_ACTIVITY_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ACTIVITY_UPDATE_DATES
    @P_ACTIVITY_ID INT,
    @P_ESTIMATED_START_DATE DATE = NULL,
    @P_ESTIMATED_END_DATE DATE = NULL,
    @P_MODIFIED_BY INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Activity
        SET EstimatedStartDate = @P_ESTIMATED_START_DATE,
            EstimatedEndDate = @P_ESTIMATED_END_DATE,
            ModifiedBy = @P_MODIFIED_BY,
            ModifiedDate = SYSUTCDATETIME()
        WHERE ActivityId = @P_ACTIVITY_ID AND IsDeleted = 0;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
