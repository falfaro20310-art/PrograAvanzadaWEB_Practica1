-- 1. Blood Pressure
INSERT INTO [dbo].[HealthIndicatorType] (Name, Unit, MinNormalValue, MaxNormalValue, Gender)
VALUES ('Blood Pressure', 'mmHg', 90.00, 120.00, 0);

-- 2. Blood Glucose
INSERT INTO [dbo].[HealthIndicatorType] (Name, Unit, MinNormalValue, MaxNormalValue, Gender)
VALUES ('Blood Glucose', 'mg/dL', 70.00, 99.00, 0);

-- 3. Mood (self-reported scale)
INSERT INTO [dbo].[HealthIndicatorType] (Name, Unit, MinNormalValue, MaxNormalValue, Gender)
VALUES ('Mood', 'scale 1-5', 3.00, 5.00, 0);

-- 4. Sleep Hours
INSERT INTO [dbo].[HealthIndicatorType] (Name, Unit, MinNormalValue, MaxNormalValue, Gender)
VALUES ('Sleep Hours', 'hours', 6.00, 9.00, 0);

-- 5. Weight
INSERT INTO [dbo].[HealthIndicatorType] (Name, Unit, MinNormalValue, MaxNormalValue, Gender)
VALUES ('Weight', 'kg', 50.00, 90.00, 0);