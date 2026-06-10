/*
    SQL Server — Seed RobotConfig từ robot_teach_config.json.
    FullState = N'EMPTY'. Không dùng Gripper.
    Chạy sau 01_RobotConfig_Schema.sql.
*/

USE [AGVControlSystem];
GO

;WITH Seed AS
(
    SELECT *
    FROM (VALUES
        -- PosName, PosGroup, J1,  J2,  J3, J4,  J5
        (N'PickUp', N'Chung', N'20',  N'0',  N'0',  N'0',  N'0'),
        (N'Home',   N'Chung', N'20',  N'20', N'20', N'20', N'-20'),
        (N'Wait',   N'Chung', N'10',  N'10', N'0',  N'10', N'0'),
        (N'OK1',    N'OK',    N'10',  N'0',  N'0',  N'10', N'0'),
        (N'OK2',    N'OK',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'OK3',    N'OK',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'OK4',    N'OK',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'OK5',    N'OK',    N'4',   N'0',  N'0',  N'0',  N'0'),
        (N'OK6',    N'OK',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'NG1',    N'NG',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'NG2',    N'NG',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'NG3',    N'NG',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'NG4',    N'NG',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'NG5',    N'NG',    N'0',   N'0',  N'0',  N'0',  N'0'),
        (N'NG6',    N'NG',    N'0',   N'0',  N'0',  N'0',  N'0')
    ) AS V(PosName, PosGroup, J1, J2, J3, J4, J5)
),
Prepared AS
(
    SELECT
        PosName,
        PosGroup,
        J1,
        J2,
        J3,
        J4,
        J5,
        FullState  = N'EMPTY',
        UpdateTime = GETDATE()
    FROM Seed
)
MERGE RobotConfig AS target
USING Prepared AS source
    ON target.PosName = source.PosName
WHEN MATCHED THEN
    UPDATE SET
        PosGroup   = source.PosGroup,
        J1         = source.J1,
        J2         = source.J2,
        J3         = source.J3,
        J4         = source.J4,
        J5         = source.J5,
        FullState  = source.FullState,
        UpdateTime = source.UpdateTime
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ID, PosName, PosGroup, J1, J2, J3, J4, J5, FullState, UpdateTime)
    VALUES (NEWID(), source.PosName, source.PosGroup, source.J1, source.J2, source.J3,
            source.J4, source.J5, source.FullState, source.UpdateTime);
GO
