using Haui.GarlicDetector.Common;
using Haui.GarlicDetector.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Haui.GarlicDetector.Services
{
    public class DatabaseService
    {
        private static string ConnectionString => AppSettings.Instance.ConnectionString;

        private static SqlConnection conn = new SqlConnection(ConnectionString);
        private static SqlCommand cmd = new SqlCommand();
        private static SqlDataAdapter da;

        public static void SaveGarlicRegionHistory(GarlicRegion garlicRegion)
        {
            try
            {
                string Stored = "Insert_DetectHistory";
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = Stored;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@GarlicType", garlicRegion.FinalLabel.Value);
                cmd.Parameters.AddWithValue("@UpdateTime", garlicRegion.DetectedAt);
                cmd.Parameters.AddWithValue("@UpdateBy", "System");
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
        }

        /// <summary>
        /// Lấy số lượng phát hiện theo ngày (0h00 - 23h59:59).
        /// </summary>
        /// <param name="date">Ngày cần lấy dữ liệu. Null = hôm nay.</param>
        /// <param name="big">Số lượng tỏi to</param>
        /// <param name="small">Số lượng tỏi nhỏ</param>
        /// <param name="error">Số lượng tỏi hỏng</param>
        public static void GetDetectCount(out int big, out int small, out int error, DateTime? date = null)
        {
            big = 0;
            small = 0;
            error = 0;

            try
            {
                // Lấy ngày hiện tại nếu không truyền vào
                DateTime targetDate = date ?? DateTime.Today;

                // Từ 0h00:00.000 đến 23h59:59.999 của ngày
                DateTime from = targetDate.Date; // 0h00:00
                DateTime to = targetDate.Date.AddDays(1).AddTicks(-1); // 23h59:59.999

                string Stored = "Get_Count_Detect";
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = Stored;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@StartTime", from);
                cmd.Parameters.AddWithValue("@EndTime", to);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        big = reader.IsDBNull(reader.GetOrdinal("Big")) ? 0 : reader.GetInt32(reader.GetOrdinal("Big"));
                        small = reader.IsDBNull(reader.GetOrdinal("Small")) ? 0 : reader.GetInt32(reader.GetOrdinal("Small"));
                        error = reader.IsDBNull(reader.GetOrdinal("Error")) ? 0 : reader.GetInt32(reader.GetOrdinal("Error"));
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy số lượng theo ngày: {ex}");
            }
        }

        // Thêm lấy history theo thời gian ra table
        public static DataTable GetDetectHistoryByTime(DateTime from, DateTime to)
        {
            DataTable dt = new DataTable();
            try
            {
                string Stored = "Get_DetectHistoryByTime";
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = Stored;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@StartTime", from);
                cmd.Parameters.AddWithValue("@EndTime", to);
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy lịch sử theo thời gian: {ex}");
            }
            return dt;
        }

        // ─── Helper Methods ──────────────────────────────────────────────────

        /// <summary>
        /// Lấy số lượng phát hiện hôm nay (0h - 23h59).
        /// </summary>
        public static void GetTodayDetectCount(out int big, out int small, out int error)
            => GetDetectCount(out big, out small, out error, DateTime.Today);

        /// <summary>
        /// Lấy số lượng phát hiện hôm qua (0h - 23h59).
        /// </summary>
        public static void GetYesterdayDetectCount(out int big, out int small, out int error)
            => GetDetectCount(out big, out small, out error, DateTime.Today.AddDays(-1));

        /// <summary>
        /// Lấy lịch sử phát hiện hôm nay (0h - 23h59).
        /// </summary>
        public static DataTable GetTodayHistory()
        {
            var today = DateTime.Today;
            return GetDetectHistoryByTime(today, today.AddDays(1).AddTicks(-1));
        }

        /// <summary>
        /// Lấy lịch sử phát hiện hôm qua (0h - 23h59).
        /// </summary>
        public static DataTable GetYesterdayHistory()
        {
            var yesterday = DateTime.Today.AddDays(-1);
            return GetDetectHistoryByTime(yesterday, yesterday.AddDays(1).AddTicks(-1));
        }

        /// <summary>
        /// Lấy lịch sử phát hiện tuần này (Chủ Nhật - Thứ Bảy).
        /// </summary>
        public static DataTable GetThisWeekHistory()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);
            return GetDetectHistoryByTime(startOfWeek, endOfWeek);
        }

        /// <summary>
        /// Lấy lịch sử phát hiện tháng này.
        /// </summary>
        public static DataTable GetThisMonthHistory()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
            return GetDetectHistoryByTime(startOfMonth, endOfMonth);
        }
    }
}
