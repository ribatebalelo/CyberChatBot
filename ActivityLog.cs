using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberBot
{
    public class ActivityLog
    {
        private readonly List<LogEntry> _entries = new List<LogEntry>();

        public void Add(string category, string description)
        {
            _entries.Add(new LogEntry(DateTime.Now, category, description));
        }

        public List<LogEntry> Recent(int count = 10)
        {
            return _entries.AsEnumerable().Reverse().Take(count).ToList();
        }

        public int TotalCount => _entries.Count;

        public string FormatRecent(int count = 10)
        {
            var recent = Recent(count);
            if (!recent.Any()) return "No activity recorded yet.";

            var sb = new StringBuilder();
            sb.AppendLine("Here is a summary of recent actions:\n");
            for (int i = 0; i < recent.Count; i++)
            {
                var e = recent[i];
                sb.AppendLine(string.Format("  {0}. [{1}]  {2}: {3}",
                    i + 1, e.Timestamp.ToString("HH:mm:ss"), e.Category, e.Description));
            }
            sb.AppendLine(string.Format("\nTotal actions recorded: {0}", _entries.Count));
            return sb.ToString().TrimEnd();
        }
    }
}
