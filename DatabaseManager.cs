using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CyberBot
{
    public class DatabaseManager
    {
        private const string INSTANCE = @"(localdb)\MSSQLLocalDB";
        private const string DB_NAME  = "CyberBotDB";

        private static readonly string MasterConn =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=10;";

        private static readonly string AppConn =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CyberBotDB;Integrated Security=True;Connect Timeout=10;";

        public bool   IsConnected { get; private set; } = false;
        public string StatusMsg   { get; private set; } = "Not initialised";

        // ── Init ──────────────────────────────────────────────────────────────
        public void Init()
        {
            try
            {
                EnsureDatabaseExists();
                EnsureTableExists();
                IsConnected = true;
                StatusMsg   = "SQL Server LocalDB — CyberBotDB connected";
            }
            catch (Exception ex)
            {
                IsConnected = false;
                StatusMsg   = "DB offline: " + ex.Message;
            }
        }

        // ── Create DB ─────────────────────────────────────────────────────────
        private void EnsureDatabaseExists()
        {
            using (var conn = new SqlConnection(MasterConn))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "IF DB_ID(N'CyberBotDB') IS NULL CREATE DATABASE [CyberBotDB]", conn))
                    cmd.ExecuteNonQuery();
            }
        }

        // ── Create Table ──────────────────────────────────────────────────────
        private void EnsureTableExists()
        {
            using (var conn = new SqlConnection(AppConn))
            {
                conn.Open();
                string sql = @"
                    IF OBJECT_ID(N'dbo.Tasks', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Tasks (
                            Id          INT           IDENTITY(1,1)  NOT NULL PRIMARY KEY,
                            Title       NVARCHAR(200)                NOT NULL,
                            Description NVARCHAR(MAX)                NOT NULL,
                            CreatedAt   DATETIME                     NOT NULL,
                            Reminder    DATE                             NULL,
                            Status      NVARCHAR(20)  DEFAULT 'Pending' NOT NULL
                        )
                    END";
                using (var cmd = new SqlCommand(sql, conn))
                    cmd.ExecuteNonQuery();
            }
        }

        // ── INSERT ────────────────────────────────────────────────────────────
        public int InsertTask(string title, string description, DateTime? reminder)
        {
            if (!IsConnected) return -1;
            try
            {
                using (var conn = new SqlConnection(AppConn))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO dbo.Tasks (Title, Description, CreatedAt, Reminder, Status)
                        VALUES (@title, @desc, @created, @reminder, 'Pending');
                        SELECT SCOPE_IDENTITY();";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@title",   SqlDbType.NVarChar, 200).Value = title;
                        cmd.Parameters.Add("@desc",    SqlDbType.NVarChar, -1 ).Value = description;
                        cmd.Parameters.Add("@created", SqlDbType.DateTime     ).Value = DateTime.Now;
                        cmd.Parameters.Add("@reminder",SqlDbType.Date         ).Value =
                            reminder.HasValue ? (object)reminder.Value.Date : DBNull.Value;
                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value
                               ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex) { Log("InsertTask", ex); return -1; }
        }

        // ── SELECT ALL ────────────────────────────────────────────────────────
        public List<CyberTask> LoadAllTasks()
        {
            var list = new List<CyberTask>();
            if (!IsConnected) return list;
            try
            {
                using (var conn = new SqlConnection(AppConn))
                {
                    conn.Open();
                    string sql = @"SELECT Id, Title, Description, CreatedAt, Reminder, Status
                                   FROM dbo.Tasks ORDER BY Id";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new CyberTask
                            {
                                Id           = rdr.GetInt32(0),
                                Title        = rdr.GetString(1),
                                Description  = rdr.GetString(2),
                                CreatedAt    = rdr.GetDateTime(3),
                                ReminderDate = rdr.IsDBNull(4)
                                               ? (DateTime?)null
                                               : rdr.GetDateTime(4),
                                Status       = rdr.GetString(5) == "Completed"
                                               ? CyberTaskStatus.Completed
                                               : CyberTaskStatus.Pending
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { Log("LoadAllTasks", ex); }
            return list;
        }

        // ── UPDATE status ─────────────────────────────────────────────────────
        public bool UpdateStatus(int id, string status)
        {
            if (!IsConnected) return false;
            try
            {
                using (var conn = new SqlConnection(AppConn))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Tasks SET Status=@s WHERE Id=@id", conn))
                    {
                        cmd.Parameters.Add("@s",  SqlDbType.NVarChar, 20).Value = status;
                        cmd.Parameters.Add("@id", SqlDbType.Int          ).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex) { Log("UpdateStatus", ex); return false; }
        }

        // ── UPDATE reminder ───────────────────────────────────────────────────
        public bool UpdateReminder(int id, DateTime? reminder)
        {
            if (!IsConnected) return false;
            try
            {
                using (var conn = new SqlConnection(AppConn))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Tasks SET Reminder=@r WHERE Id=@id", conn))
                    {
                        cmd.Parameters.Add("@r",  SqlDbType.Date).Value =
                            reminder.HasValue ? (object)reminder.Value.Date : DBNull.Value;
                        cmd.Parameters.Add("@id", SqlDbType.Int ).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex) { Log("UpdateReminder", ex); return false; }
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public bool DeleteTask(int id)
        {
            if (!IsConnected) return false;
            try
            {
                using (var conn = new SqlConnection(AppConn))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "DELETE FROM dbo.Tasks WHERE Id=@id", conn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex) { Log("DeleteTask", ex); return false; }
        }

        private static void Log(string m, Exception ex) =>
            System.Diagnostics.Debug.WriteLine("[DB:" + m + "] " + ex.Message);
    }
}
