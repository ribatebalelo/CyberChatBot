using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberBot
{
    /// <summary>
    /// Manages CyberTasks in memory AND persists them to MySQL via DatabaseManager.
    /// If the database is unavailable the app continues with in-memory storage only.
    /// </summary>
    public class TaskManager
    {
        private readonly List<CyberTask>  _tasks  = new List<CyberTask>();
        private readonly DatabaseManager  _db;
        private int _nextId = 1;

        public bool DbConnected => _db.IsConnected;
        public string DbStatus  => _db.StatusMsg;

        public TaskManager(DatabaseManager db)
        {
            _db = db;
            LoadFromDb();
        }

        // ── Load existing tasks from DB on startup ────────────────────────────
        private void LoadFromDb()
        {
            if (!_db.IsConnected) return;
            var rows = _db.LoadAllTasks();
            _tasks.AddRange(rows);
            if (_tasks.Any())
                _nextId = _tasks.Max(t => t.Id) + 1;
        }

        // ── Public list ───────────────────────────────────────────────────────
        public IReadOnlyList<CyberTask> AllTasks => _tasks.AsReadOnly();

        // ── Add ───────────────────────────────────────────────────────────────
        public CyberTask Add(string title, string description, DateTime? reminder = null)
        {
            var t = new CyberTask
            {
                Title        = title,
                Description  = description,
                CreatedAt    = DateTime.Now,
                ReminderDate = reminder,
                Status       = CyberTaskStatus.Pending
            };

            // Try to insert into DB first (gets the real auto-increment id)
            int dbId = _db.InsertTask(title, description, reminder);
            t.Id = dbId > 0 ? dbId : _nextId++;

            _tasks.Add(t);
            return t;
        }

        // ── Complete ──────────────────────────────────────────────────────────
        public bool Complete(int id)
        {
            var t = _tasks.FirstOrDefault(x => x.Id == id);
            if (t == null) return false;
            t.Status = CyberTaskStatus.Completed;
            _db.UpdateStatus(id, "Completed");
            return true;
        }

        // ── Delete ────────────────────────────────────────────────────────────
        public bool Delete(int id)
        {
            var t = _tasks.FirstOrDefault(x => x.Id == id);
            if (t == null) return false;
            _tasks.Remove(t);
            _db.DeleteTask(id);
            return true;
        }

        // ── Update reminder ───────────────────────────────────────────────────
        public bool SetReminder(int id, DateTime reminder)
        {
            var t = _tasks.FirstOrDefault(x => x.Id == id);
            if (t == null) return false;
            t.ReminderDate = reminder;
            _db.UpdateReminder(id, reminder);
            return true;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        public CyberTask GetById(int id) => _tasks.FirstOrDefault(x => x.Id == id);
        public List<CyberTask> Pending()   => _tasks.Where(x => x.Status == CyberTaskStatus.Pending).ToList();
        public List<CyberTask> Completed() => _tasks.Where(x => x.Status == CyberTaskStatus.Completed).ToList();

        public List<CyberTask> DueReminders() =>
            _tasks.Where(x =>
                x.Status == CyberTaskStatus.Pending &&
                x.ReminderDate.HasValue &&
                x.ReminderDate.Value.Date <= DateTime.Today).ToList();
    }
}
