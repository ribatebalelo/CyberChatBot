using System;

namespace CyberBot
{
    public enum CyberTaskStatus { Pending, Completed }

    public class CyberTask
    {
        public int            Id          { get; set; }
        public string         Title       { get; set; }
        public string         Description { get; set; }
        public DateTime       CreatedAt   { get; set; }
        public DateTime?      ReminderDate{ get; set; }
        public CyberTaskStatus Status     { get; set; }

        public string StatusIcon => Status == CyberTaskStatus.Completed ? "✅" : "🔲";

        public string ReminderText =>
            ReminderDate.HasValue
                ? "Reminder: " + ReminderDate.Value.ToString("dd MMM yyyy")
                : "No reminder";
    }

    public class LogEntry
    {
        public DateTime Timestamp   { get; }
        public string   Category    { get; }
        public string   Description { get; }

        public LogEntry(DateTime ts, string cat, string desc)
        {
            Timestamp   = ts;
            Category    = cat;
            Description = desc;
        }
    }

    public class QuizQuestion
    {
        public string   Question    { get; set; }
        public string[] Options     { get; set; }   // null = True/False
        public int      AnswerIndex { get; set; }
        public string   Explanation { get; set; }
        public bool IsTrueFalse => Options == null;
    }
}
