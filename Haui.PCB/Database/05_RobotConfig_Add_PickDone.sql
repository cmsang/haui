/*
    SQL Server — Thêm Pick Done (clone từ PickUp làm giá trị khởi tạo).
    Operator teach lại trên màn Teaching để tránh va đập sau khi gắp.
    Chỉ INSERT khi chưa có — không ghi đè điểm đã teach.

    Database: AGVControlSystem (theo setting.json)
*/

USE [AGVControlSystem];
GO

;WITH Source AS
(
    SELECT J1, J2, J3, J4, J5
    FROM RobotConfig
    WHERE PosName = N'PickUp'
),
Prepared AS
(
    SELECT
        PosName    = N'Pick Done',
        PosGroup   = N'Chung',
        J1         = s.J1,
        J2         = s.J2,
        J3         = s.J3,
        J4         = s.J4,
        J5         = s.J5,
        FullState  = N'EMPTY',
        UpdateTime = GETDATE()
    FROM Source s
)
MERGE RobotConfig AS target
USING Prepared AS source
    ON target.PosName = source.PosName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ID, PosName, PosGroup, J1, J2, J3, J4, J5, FullState, UpdateTime)
    VALUES (NEWID(), source.PosName, source.PosGroup, source.J1, source.J2, source.J3,
            source.J4, source.J5, source.FullState, source.UpdateTime);
GO
