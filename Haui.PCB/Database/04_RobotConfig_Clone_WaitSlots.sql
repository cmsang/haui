/*
    SQL Server — Clone OK1–4 / NG1–4 thành Wait OK1–4 / Wait NG1–4.
    Sao chép J1–J5 từ slot gốc làm giá trị khởi tạo; operator teach lại từng điểm wait trên màn Teaching.
    Chỉ INSERT khi chưa có — không ghi đè điểm đã teach.
    Chạy sau 01 → 02 (hoặc trên DB đã có OK/NG).

    Database: AGVControlSystem (theo setting.json)
*/

USE [AGVControlSystem];
GO

;WITH Slots AS
(
    SELECT PosName, J1, J2, J3, J4, J5
    FROM RobotConfig
    WHERE PosName IN (N'OK1', N'OK2', N'OK3', N'OK4', N'NG1', N'NG2', N'NG3', N'NG4')
),
Cloned AS
(
    SELECT
        WaitName   = N'Wait ' + s.PosName,
        PosGroup   = N'Chung',
        J1         = s.J1,
        J2         = s.J2,
        J3         = s.J3,
        J4         = s.J4,
        J5         = s.J5,
        FullState  = N'EMPTY',
        UpdateTime = GETDATE()
    FROM Slots s
)
MERGE RobotConfig AS target
USING Cloned AS source
    ON target.PosName = source.WaitName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ID, PosName, PosGroup, J1, J2, J3, J4, J5, FullState, UpdateTime)
    VALUES (NEWID(), source.WaitName, source.PosGroup, source.J1, source.J2, source.J3,
            source.J4, source.J5, source.FullState, source.UpdateTime);
GO

-- Wait OK / Wait NG cũ (một điểm chung) không còn dùng trong chu trình
DELETE FROM RobotConfig WHERE PosName IN (N'Wait OK', N'Wait NG');
GO
