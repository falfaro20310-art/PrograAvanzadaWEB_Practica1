USE [vitalapp_];
GO

-- =====================================================================
-- 0. VERIFICAR / CORREGIR el FK de Objective
-- =====================================================================

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Objective_IndicatorType')
BEGIN
    ALTER TABLE [dbo].[Objective] DROP CONSTRAINT FK_Objective_IndicatorType;
END
GO

IF OBJECT_ID('dbo.HealthIndicatorType', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Objective_IndicatorType')
BEGIN
    ALTER TABLE [dbo].[Objective]
        ADD CONSTRAINT FK_Objective_IndicatorType
        FOREIGN KEY (IndicatorTypeId) REFERENCES [dbo].[HealthIndicatorType](IndicatorTypeId);
END
GO

-- =====================================================================
-- 1. CREATE - Registrar un objetivo de salud
-- =====================================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserObjective_Create]
    @UserId          INT,
    @IndicatorTypeId INT,
    @Title           NVARCHAR(150),
    @Description     NVARCHAR(500) = NULL,
    @InitialValue    DECIMAL(6,2)  = NULL,
    @ObjectiveValue  DECIMAL(6,2)  = NULL,
    @StartDate       DATETIME2,
    @LimitDate       DATETIME2     = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[HealthIndicatorType] WHERE IndicatorTypeId = @IndicatorTypeId AND DeletedAt IS NULL)
    BEGIN
        RAISERROR('IndicatorType not found or has been deleted.', 16, 1);
        RETURN;
    END

    INSERT INTO [dbo].[Objective]
        (UserId, IndicatorTypeId, Title, Description, InitialValue, ObjectiveValue, StartDate, LimitDate, Status, CreatedAt)
    VALUES
        (@UserId, @IndicatorTypeId, @Title, @Description, @InitialValue, @ObjectiveValue, @StartDate, @LimitDate, 'EnProgreso', SYSUTCDATETIME());

    SELECT SCOPE_IDENTITY() AS NewObjectiveId;
END
GO

-- =====================================================================
-- 2. READ - Consultar objetivos (con filtros opcionales)
-- =====================================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserObjective_Get]
    @ObjectiveId INT = NULL,
    @UserId      INT = NULL,
    @Status      NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        o.ObjectiveId,
        o.UserId,
        o.IndicatorTypeId,
        it.Name AS IndicatorTypeName,
        it.Unit,
        o.Title,
        o.Description,
        o.InitialValue,
        o.ObjectiveValue,
        o.StartDate,
        o.LimitDate,
        o.Status,
        o.CreatedAt,
        o.UpdatedAt,
        (SELECT TOP 1 CurrentValue FROM [dbo].[ObjectiveProgress] p
         WHERE p.ObjectiveId = o.ObjectiveId AND p.DeletedAt IS NULL
         ORDER BY p.Date DESC) AS LastValue,
        (SELECT TOP 1 ComplianceRate FROM [dbo].[ObjectiveProgress] p
         WHERE p.ObjectiveId = o.ObjectiveId AND p.DeletedAt IS NULL
         ORDER BY p.Date DESC) AS ComplianceRate
    FROM [dbo].[Objective] o
    INNER JOIN [dbo].[HealthIndicatorType] it ON it.IndicatorTypeId = o.IndicatorTypeId
    WHERE o.DeletedAt IS NULL
        AND (@ObjectiveId IS NULL OR o.ObjectiveId = @ObjectiveId)
        AND (@UserId      IS NULL OR o.UserId = @UserId)
        AND (@Status      IS NULL OR o.Status = @Status)
    ORDER BY o.StartDate DESC;
END
GO

-- =====================================================================
-- 3. UPDATE - Editar un objetivo
-- =====================================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserObjective_Update]
    @ObjectiveId    INT,
    @Title          NVARCHAR(150),
    @Description    NVARCHAR(500) = NULL,
    @ObjectiveValue DECIMAL(6,2)  = NULL,
    @LimitDate      DATETIME2     = NULL,
    @Status         NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[Objective]
    SET Title          = @Title,
        Description    = @Description,
        ObjectiveValue = @ObjectiveValue,
        LimitDate      = @LimitDate,
        Status         = @Status,
        UpdatedAt      = SYSUTCDATETIME()
    WHERE ObjectiveId = @ObjectiveId
        AND DeletedAt IS NULL;

    IF @@ROWCOUNT = 0
        RAISERROR('Objective not found or already deleted.', 16, 1);
END
GO

-- =====================================================================
-- 4. SOFT DELETE
-- =====================================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserObjective_Delete]
    @ObjectiveId INT,
    @UserId      INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[Objective]
    SET DeletedAt = SYSUTCDATETIME()
    WHERE ObjectiveId = @ObjectiveId
        AND UserId = @UserId
        AND DeletedAt IS NULL;

    IF @@ROWCOUNT = 0
        RAISERROR('Objective not found or already deleted.', 16, 1);
END
GO

-- =====================================================================
-- 5. Registrar avance (progreso) de un objetivo
-- =====================================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserObjective_RegisterProgress]
    @ObjectiveId   INT,
    @Date          DATETIME2,
    @CurrentValue  DECIMAL(6,2)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InitialValue DECIMAL(6,2);
    DECLARE @ObjectiveValue DECIMAL(6,2);
    DECLARE @ComplianceRate DECIMAL(5,2);

    SELECT @InitialValue = InitialValue, @ObjectiveValue = ObjectiveValue
    FROM [dbo].[Objective]
    WHERE ObjectiveId = @ObjectiveId AND DeletedAt IS NULL;

    IF @ObjectiveValue IS NULL OR @InitialValue IS NULL OR @ObjectiveValue = @InitialValue
        SET @ComplianceRate = 0;
    ELSE
        SET @ComplianceRate = ROUND(
            ((@CurrentValue - @InitialValue) / (@ObjectiveValue - @InitialValue)) * 100, 2);

    INSERT INTO [dbo].[ObjectiveProgress] (ObjectiveId, Date, CurrentValue, ComplianceRate, CreatedAt)
    VALUES (@ObjectiveId, @Date, @CurrentValue, @ComplianceRate, SYSUTCDATETIME());

    IF @ComplianceRate >= 100
    BEGIN
        UPDATE [dbo].[Objective]
        SET Status = 'Completado', UpdatedAt = SYSUTCDATETIME()
        WHERE ObjectiveId = @ObjectiveId;
    END

    SELECT SCOPE_IDENTITY() AS NewObjectiveProgressId, @ComplianceRate AS ComplianceRate;
END
GO

-- =====================================================================
-- 6. Consultar el historial de avance de un objetivo
-- =====================================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserObjective_GetProgress]
    @ObjectiveId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ObjectiveProgressId, ObjectiveId, Date, CurrentValue, ComplianceRate, CreatedAt
    FROM [dbo].[ObjectiveProgress]
    WHERE ObjectiveId = @ObjectiveId
        AND DeletedAt IS NULL
    ORDER BY Date ASC;
END
GO