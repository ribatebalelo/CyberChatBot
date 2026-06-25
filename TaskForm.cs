using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CyberBot
{
    public class TaskForm : Form
    {
        private readonly TaskManager    _tasks;
        private readonly ActivityLog    _log;
        private readonly Action<string> _onAction; // post result to chat

        // Controls
        private Panel       pnlHeader;
        private Label       lblTitle;
        private Label       lblCount;
        private ListView    lstTasks;
        private Panel       pnlAdd;
        private TextBox     txtTitle;
        private TextBox     txtDesc;
        private DateTimePicker dtpReminder;
        private CheckBox    chkReminder;
        private Button      btnAdd;
        private Panel       pnlButtons;
        private Button      btnComplete;
        private Button      btnDelete;
        private Button      btnRefresh;
        private Button      btnClose;

        static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);
        readonly Color BgDark   = C(12, 22, 42);
        readonly Color BgPanel  = C(16, 32, 62);
        readonly Color BgInput  = C(8, 18, 38);
        readonly Color AccBlue  = C(0, 162, 255);
        readonly Color AccGreen = C(0, 210, 120);
        readonly Color AccRed   = C(230, 70, 70);
        readonly Color TxtMain  = C(215, 235, 255);
        readonly Color TxtMuted = C(100, 140, 185);
        readonly Color Border   = C(28, 68, 120);

        public TaskForm(TaskManager tasks, ActivityLog log, Action<string> onAction)
        {
            _tasks    = tasks;
            _log      = log;
            _onAction = onAction;

            Text            = "Cybersecurity Task Manager";
            Size            = new Size(740, 620);
            MinimumSize     = new Size(660, 540);
            BackColor       = BgDark;
            ForeColor       = TxtMain;
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            Icon            = SystemIcons.Shield;

            BuildUI();
            RefreshList();
        }

        private void BuildUI()
        {
            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = BgPanel };
            pnlHeader.Paint += (s, e) =>
            {
                using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 61, pnlHeader.Width, 61);
                using (var b = new SolidBrush(AccGreen)) e.Graphics.FillRectangle(b, 0, 0, 5, 62);
            };

            lblTitle = new Label {
                Text      = "📋  TASK MANAGER",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AccGreen,
                Location  = new Point(18, 10),
                AutoSize  = true
            };
            lblCount = new Label {
                Text      = "0 tasks",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TxtMuted,
                Location  = new Point(18, 40),
                AutoSize  = true
            };
            pnlHeader.Controls.AddRange(new Control[]{ lblTitle, lblCount });

            // Add-task panel
            pnlAdd = new Panel { Dock = DockStyle.Top, Height = 165, BackColor = BgInput, Padding = new Padding(14, 10, 14, 10) };
            pnlAdd.Paint += (s, e) =>
            {
                using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 164, pnlAdd.Width, 164);
            };

            var lblNewTask = new Label { Text = "Add New Task", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AccBlue, Location = new Point(14, 8), AutoSize = true };

            var lblTitleL  = new Label { Text = "Title:", Font = new Font("Segoe UI", 8.5f), ForeColor = TxtMuted, Location = new Point(14, 30), AutoSize = true };
            txtTitle = new TextBox { Font = new Font("Segoe UI", 9.5f), BackColor = BgPanel, ForeColor = TxtMain, BorderStyle = BorderStyle.FixedSingle, Location = new Point(14, 48), Width = 480, TabIndex = 0 };

            var lblDescL = new Label { Text = "Description (optional):", Font = new Font("Segoe UI", 8.5f), ForeColor = TxtMuted, Location = new Point(14, 74), AutoSize = true };
            txtDesc = new TextBox { Font = new Font("Segoe UI", 9f), BackColor = BgPanel, ForeColor = TxtMain, BorderStyle = BorderStyle.FixedSingle, Location = new Point(14, 91), Width = 480, TabIndex = 1 };

            chkReminder = new CheckBox { Text = "Set reminder:", Font = new Font("Segoe UI", 8.5f), ForeColor = TxtMain, Location = new Point(14, 122), AutoSize = true, TabIndex = 2 };
            dtpReminder = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9f), Location = new Point(116, 120), Width = 150, MinDate = DateTime.Today, Value = DateTime.Today.AddDays(7), Enabled = false, TabIndex = 3 };
            chkReminder.CheckedChanged += (s, e) => dtpReminder.Enabled = chkReminder.Checked;

            btnAdd = new Button {
                Text      = "➕  Add Task",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = AccBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(510, 44),
                Size      = new Size(120, 70),
                Cursor    = Cursors.Hand,
                TabIndex  = 4
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += OnAddTask;

            pnlAdd.Controls.AddRange(new Control[]{ lblNewTask, lblTitleL, txtTitle, lblDescL, txtDesc, chkReminder, dtpReminder, btnAdd });

            // Task list
            lstTasks = new ListView {
                Dock          = DockStyle.Fill,
                BackColor     = BgInput,
                ForeColor     = TxtMain,
                Font          = new Font("Segoe UI", 9.5f),
                FullRowSelect = true,
                GridLines     = false,
                BorderStyle   = BorderStyle.None,
                View          = View.Details,
                MultiSelect   = false,
                HeaderStyle   = ColumnHeaderStyle.Nonclickable
            };
            lstTasks.Columns.Add("",    26,  HorizontalAlignment.Center);
            lstTasks.Columns.Add("#",   36,  HorizontalAlignment.Center);
            lstTasks.Columns.Add("Title",       200, HorizontalAlignment.Left);
            lstTasks.Columns.Add("Description", 240, HorizontalAlignment.Left);
            lstTasks.Columns.Add("Reminder",    120, HorizontalAlignment.Left);
            lstTasks.Columns.Add("Status",       70, HorizontalAlignment.Center);

            // Button bar
            pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = BgPanel };
            pnlButtons.Paint += (s, e) => {
                using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 0, pnlButtons.Width, 0);
            };

            btnComplete = MakeBtn("✅  Complete", AccGreen,  10);
            btnDelete   = MakeBtn("🗑  Delete",   AccRed,   150);
            btnRefresh  = MakeBtn("🔄  Refresh",  C(60,60,100), 290);
            btnClose    = MakeBtn("✖  Close",    C(60,40,40), 430);

            btnComplete.Click += OnComplete;
            btnDelete.Click   += OnDelete;
            btnRefresh.Click  += (s, e) => RefreshList();
            btnClose.Click    += (s, e) => Close();

            pnlButtons.Controls.AddRange(new Control[]{ btnComplete, btnDelete, btnRefresh, btnClose });

            Controls.Add(lstTasks);
            Controls.Add(pnlButtons);
            Controls.Add(pnlAdd);
            Controls.Add(pnlHeader);
        }

        private Button MakeBtn(string text, Color bg, int left)
        {
            var btn = new Button {
                Text      = text,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(left, 8),
                Size      = new Size(130, 34),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ── Actions ───────────────────────────────────────────────────────────
        private void OnAddTask(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a task title.", "Missing Title", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string desc = txtDesc.Text.Trim();
            if (string.IsNullOrEmpty(desc)) desc = AutoDesc(title);

            DateTime? reminder = chkReminder.Checked ? (DateTime?)dtpReminder.Value.Date : null;

            var task = _tasks.Add(title, desc, reminder);
            string taskTitle = task.Title;
            _log.Add("Tasks", "Added: " + title + (reminder.HasValue ? " (reminder: " + reminder.Value.ToString("dd MMM yyyy") + ")" : ""));

            string msg = "Task added: '" + title + "'." + (reminder.HasValue ? " Reminder set for " + reminder.Value.ToString("dd MMM yyyy") + "." : "");
            _onAction(msg);

            txtTitle.Clear();
            txtDesc.Clear();
            chkReminder.Checked = false;
            RefreshList();
        }

        private void OnComplete(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItems.Count == 0) { MessageBox.Show("Select a task first."); return; }
            int id = (int)lstTasks.SelectedItems[0].Tag;
            if (_tasks.Complete(id))
            {
                _log.Add("Tasks", "Completed #" + id);
                _onAction("Task #" + id + " marked as completed!");
                RefreshList();
            }
        }

        private void OnDelete(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItems.Count == 0) { MessageBox.Show("Select a task first."); return; }
            int id    = (int)lstTasks.SelectedItems[0].Tag;
            string nm = lstTasks.SelectedItems[0].SubItems[2].Text;
            if (MessageBox.Show("Delete task '" + nm + "'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _tasks.Delete(id);
            _log.Add("Tasks", "Deleted #" + id + ": " + nm);
            _onAction("Task #" + id + " deleted.");
            RefreshList();
        }

        public void RefreshList()
        {
            lstTasks.Items.Clear();
            foreach (var t in _tasks.AllTasks)
            {
                var item = new ListViewItem(t.StatusIcon);
                item.Tag = t.Id;
                item.SubItems.Add(t.Id.ToString());
                item.SubItems.Add(t.Title);
                item.SubItems.Add(t.Description.Length > 50 ? t.Description.Substring(0, 47) + "..." : t.Description);
                item.SubItems.Add(t.ReminderDate.HasValue ? t.ReminderDate.Value.ToString("dd MMM yyyy") : "—");
                item.SubItems.Add(t.Status.ToString());
                item.BackColor = t.Status == CyberTaskStatus.Completed ? C(8, 40, 20) : BgInput;
                item.ForeColor = t.Status == CyberTaskStatus.Completed ? C(80, 180, 100) : TxtMain;
                lstTasks.Items.Add(item);
            }
            int total     = _tasks.AllTasks.Count;
            int pending   = _tasks.Pending().Count;
            int completed = _tasks.Completed().Count;
            lblCount.Text = total + " task" + (total == 1 ? "" : "s") +
                            "  —  " + pending + " pending,  " + completed + " completed";
        }

        private static string AutoDesc(string title)
        {
            string t = title.ToLower();
            if (t.Contains("2fa") || t.Contains("two-factor")) return "Enable two-factor authentication to add an extra security layer.";
            if (t.Contains("password"))  return "Update your password to a strong, unique passphrase of at least 12 characters.";
            if (t.Contains("backup"))    return "Create a secure offline backup following the 3-2-1 rule.";
            if (t.Contains("vpn"))       return "Set up a trusted VPN to encrypt your connection on public networks.";
            if (t.Contains("antivirus")) return "Install or update antivirus software and run a full system scan.";
            if (t.Contains("update"))    return "Apply the latest security updates to your operating system.";
            if (t.Contains("privacy"))   return "Review account privacy settings to ensure your data is protected.";
            return "Complete this cybersecurity action: " + title + ".";
        }
    }
}
