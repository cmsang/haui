using System.Data;
using System.Globalization;
using DL.PCBDetect.Entities;
using Microsoft.Data.SqlClient;

namespace DL.PCBDetect;

/// <summary>
/// Truy cập SQL Server — bảng RobotConfig qua stored procedures.
/// </summary>
public class RobotConfigRepository : IRobotConfigRepository
{
    public List<RobotConfigEntity> GetAll(string connectionString)
    {
        using var conn = new SqlConnection(connectionString);
        using var cmd = new SqlCommand("dbo.Get_RobotConfig_All", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var result = new List<RobotConfigEntity>();
        while (reader.Read())
        {
            result.Add(new RobotConfigEntity
            {
                PosName = reader.GetString(reader.GetOrdinal("PosName")),
                PosGroup = reader.GetString(reader.GetOrdinal("PosGroup")),
                J1 = ReadString(reader, "J1"),
                J2 = ReadString(reader, "J2"),
                J3 = ReadString(reader, "J3"),
                J4 = ReadString(reader, "J4"),
                J5 = ReadString(reader, "J5"),
                FullState = ReadString(reader, "FullState"),
                UpdateTime = reader.GetDateTime(reader.GetOrdinal("UpdateTime"))
            });
        }

        return result;
    }

    public void UpdateTeachPoint(
        string connectionString,
        string posName,
        string j1,
        string j2,
        string j3,
        string j4,
        string j5)
    {
        using var conn = new SqlConnection(connectionString);
        using var cmd = new SqlCommand("dbo.Update_RobotConfig_TeachPoint", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@PosName", posName);
        cmd.Parameters.AddWithValue("@J1", j1);
        cmd.Parameters.AddWithValue("@J2", j2);
        cmd.Parameters.AddWithValue("@J3", j3);
        cmd.Parameters.AddWithValue("@J4", j4);
        cmd.Parameters.AddWithValue("@J5", j5);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void UpdateFullState(string connectionString, string posName, string fullState)
    {
        using var conn = new SqlConnection(connectionString);
        using var cmd = new SqlCommand("dbo.Update_RobotConfig_FullState", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@PosName", posName);
        cmd.Parameters.AddWithValue("@FullState", fullState);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    public static string FormatAngle(double angle)
        => angle.ToString("0.##", CultureInfo.InvariantCulture);

    public static double ParseAngle(string text)
        => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
}
