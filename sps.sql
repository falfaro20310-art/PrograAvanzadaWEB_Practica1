-- UserHealthIndicatorMeasure Stored Procedures...

-- =========================================================
-- 1. CREATE
-- =========================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserHealthIndicatorMeasure_Create]
    @UserId          INT,
    @IndicatorTypeId INT,
    @Value           DECIMAL(6,2),
    @SecondaryValue  DECIMAL(6,2),
    @MeasureDate     DATETIME2,
    @Notes           NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MinNormal DECIMAL(6,2);
    DECLARE @MaxNormal DECIMAL(6,2);
    DECLARE @IsAbnormal BIT;

    SELECT
        @MinNormal = MinNormalValue,
        @MaxNormal = MaxNormalValue
    FROM [dbo].[IndicatorType]
    WHERE IndicatorTypeId = @IndicatorTypeId
      AND DeletedAt IS NULL;

    IF @MinNormal IS NULL
    BEGIN
        RAISERROR('IndicatorType not found or has been deleted.', 16, 1);
        RETURN;
    END

    -- Determine if the value is outside the normal range
    SET @IsAbnormal = CASE
        WHEN @Value < @MinNormal OR @Value > @MaxNormal THEN 1
        ELSE 0
    END;

    INSERT INTO [dbo].[UserHealthIndicatorMeasure]
        (UserId, IndicatorTypeId, Value, SecondaryValue, MeasureDate, Notes, IsAbnormal, CreatedAt)
    VALUES
        (@UserId, @IndicatorTypeId, @Value, @SecondaryValue, @MeasureDate, @Notes, @IsAbnormal, SYSUTCDATETIME());

    SELECT SCOPE_IDENTITY() AS NewMeasureId;
END
GO


-- =========================================================
-- 2. READ
-- =========================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserHealthIndicatorMeasure_Get]
    @MeasureId       INT           = NULL,
    @UserId          INT           = NULL,
    @IndicatorTypeId INT           = NULL,
    @DateFrom        DATETIME2     = NULL,
    @DateTo          DATETIME2     = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.MeasureId,
        m.UserId,
        m.IndicatorTypeId,
        it.Name AS IndicatorTypeName,
        it.Unit,
        m.Value,
        m.SecondaryValue,
        m.MeasureDate,
        m.Notes,
        m.IsAbnormal,
        m.CreatedAt,
        m.UpdatedAt,
        m.DeletedAt
    FROM [dbo].[UserHealthIndicatorMeasure] m
    INNER JOIN [dbo].[IndicatorType] it
        ON it.IndicatorTypeId = m.IndicatorTypeId
    WHERE (@MeasureId       IS NULL OR m.MeasureId = @MeasureId)
      AND (@UserId          IS NULL OR m.UserId = @UserId)
      AND (@IndicatorTypeId IS NULL OR m.IndicatorTypeId = @IndicatorTypeId)
      AND (@DateFrom        IS NULL OR m.MeasureDate >= @DateFrom)
      AND (@DateTo          IS NULL OR m.MeasureDate <= @DateTo)
    ORDER BY m.MeasureDate DESC;
END
GO


-- =========================================================
-- 3. UPDATE
-- =========================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserHealthIndicatorMeasure_Update]
    @MeasureId       INT,
    @Value           DECIMAL(6,2),
    @SecondaryValue  DECIMAL(6,2),
    @MeasureDate     DATETIME2,
    @Notes           NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IndicatorTypeId INT;
    DECLARE @MinNormal DECIMAL(6,2);
    DECLARE @MaxNormal DECIMAL(6,2);
    DECLARE @IsAbnormal BIT;

    SELECT @IndicatorTypeId = IndicatorTypeId
    FROM [dbo].[UserHealthIndicatorMeasure]
    WHERE MeasureId = @MeasureId
      AND DeletedAt IS NULL;

    IF @IndicatorTypeId IS NULL
    BEGIN
        RAISERROR('Measure not found or has been deleted.', 16, 1);
        RETURN;
    END

    SELECT
        @MinNormal = MinNormalValue,
        @MaxNormal = MaxNormalValue
    FROM [dbo].[IndicatorType]
    WHERE IndicatorTypeId = @IndicatorTypeId;

    SET @IsAbnormal = CASE
        WHEN @Value < @MinNormal OR @Value > @MaxNormal THEN 1
        ELSE 0
    END;

    UPDATE [dbo].[UserHealthIndicatorMeasure]
    SET
        Value          = @Value,
        SecondaryValue = @SecondaryValue,
        MeasureDate    = @MeasureDate,
        Notes          = @Notes,
        IsAbnormal     = @IsAbnormal,
        UpdatedAt      = SYSUTCDATETIME()
    WHERE MeasureId = @MeasureId;
END
GO


-- =========================================================
-- 4. SOFT DELETE
-- =========================================================
CREATE OR ALTER PROCEDURE [dbo].[usp_UserHealthIndicatorMeasure_Delete]
    @MeasureId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[UserHealthIndicatorMeasure]
    SET DeletedAt = SYSUTCDATETIME()
    WHERE MeasureId = @MeasureId
      AND DeletedAt IS NULL;

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Measure not found or already deleted.', 16, 1);
    END
END
GO