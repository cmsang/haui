/*
    SQL Server — RobotConfig, cấu trúc bảng theo thiết kế gốc.
    Database: AGVControlSystem (theo setting.json)
*/

USE [AGVControlSystem];
GO

IF OBJECT_ID(N'dbo.RobotConfig', N'U') IS NULL
BEGIN
    CREATE TABLE RobotConfig
    (
        ID         UNIQUEIDENTIFIER,
        PosName    NVARCHAR(255),
        PosGroup   NVARCHAR(255),
        J1         NVARCHAR(255),
        J2         NVARCHAR(255),
        J3         NVARCHAR(255),
        J4         NVARCHAR(255),
        J5         NVARCHAR(255),
        FullState  NVARCHAR(255),
        UpdateTime DATETIME
    );
END
GO
