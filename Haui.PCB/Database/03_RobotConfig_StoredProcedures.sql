/*
    SQL Server — Stored procedures cho RobotConfig.
    Update_RobotConfig_TeachPoint — gọi mỗi khi teach một điểm (chỉ J1–J5, FullState = N'EMPTY').
*/

USE [AGVControlSystem];
GO

-- Suy nhóm vị trí theo quy ước RobotTeachPositions (C#)
CREATE OR ALTER PROCEDURE dbo.Update_RobotConfig_TeachPoint
    @PosName   NVARCHAR(255),
    @J1        NVARCHAR(255),
    @J2        NVARCHAR(255),
    @J3        NVARCHAR(255),
    @J4        NVARCHAR(255),
    @J5        NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM RobotConfig WHERE PosName = @PosName)
    BEGIN
        UPDATE RobotConfig
        SET J1         = @J1,
            J2         = @J2,
            J3         = @J3,
            J4         = @J4,
            J5         = @J5,
            UpdateTime = GETDATE()
        WHERE PosName = @PosName;
    END
END;
GO
-- Lấy toàn bộ vị trí teach
CREATE OR ALTER PROCEDURE dbo.Get_RobotConfig_All
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ID,
        PosName,
        PosGroup,
        J1, J2, J3, J4, J5,
        FullState,
        UpdateTime
    FROM RobotConfig
    ORDER BY
        CASE PosGroup
            WHEN N'Chung' THEN 1
            WHEN N'OK'    THEN 2
            WHEN N'NG'    THEN 3
            ELSE 4
        END,
        PosName;
END;
GO

-- Cập nhật FullState slot (EMPTY / FULL) sau khi đặt hoặc lấy hàng
CREATE OR ALTER PROCEDURE dbo.Update_RobotConfig_FullState
    @PosName    NVARCHAR(255),
    @FullState  NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM RobotConfig WHERE PosName = @PosName)
    BEGIN
        UPDATE RobotConfig
        SET FullState  = @FullState,
            UpdateTime = GETDATE()
        WHERE PosName = @PosName;
    END
END;
GO

-- Lấy một vị trí theo tên
CREATE OR ALTER PROCEDURE dbo.Get_RobotConfig_ByName
    @PosName NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ID,
        PosName,
        PosGroup,
        J1, J2, J3, J4, J5,
        FullState,
        UpdateTime
    FROM RobotConfig
    WHERE PosName = @PosName;
END;
GO
