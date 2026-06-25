using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace CyberBot
{
    public class MainForm : Form
    {
        // ── Engine ────────────────────────────────────────────────────────────
        private readonly DatabaseManager _db     = new DatabaseManager();
        private readonly ChatbotEngine  _engine;
        private readonly QuizEngine     _quiz   = new QuizEngine();
        private bool _nameCollected = false;

        // ── Controls ──────────────────────────────────────────────────────────
        private Panel          pnlHeader;
        private Label          lblTitle;
        private Label          lblSub;
        private Label          lblOnline;
        private Panel          pnlMemBar;
        private Label          lblMem;
        private Panel          pnlTopics;
        private Panel          pnlChat;
        private Panel          pnlMessages;    // scrollable inner panel
        private VScrollBar     vsb;
        private Panel          pnlInput;
        private TextBox        txtInput;
        private Button         btnSend;
        private StatusStrip    statusBar;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblClock;

        // ── Layout ────────────────────────────────────────────────────────────
        private int _chatY = 10;

        // ── Colours ───────────────────────────────────────────────────────────
        static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);
        readonly Color BgApp    = C(12, 22, 42);
        readonly Color BgHeader = C(16, 32, 62);
        readonly Color BgInput  = C(8, 18, 38);
        readonly Color BgMsg    = C(10, 20, 40);
        readonly Color BgBot    = C(16, 40, 78);
        readonly Color BgUser   = C(0, 72, 144);
        readonly Color BgTopics = C(10, 20, 45);
        readonly Color BgMem    = C(6, 14, 30);
        readonly Color AccBlue  = C(0, 162, 255);
        readonly Color AccGreen = C(0, 210, 120);
        readonly Color TxtMain  = C(215, 235, 255);
        readonly Color TxtMuted = C(100, 140, 185);
        readonly Color Border   = C(28, 68, 120);

        // ── Fonts ─────────────────────────────────────────────────────────────
        readonly Font FTitle  = new Font("Segoe UI", 15f, FontStyle.Bold);
        readonly Font FSub    = new Font("Segoe UI", 8.5f);
        readonly Font FMem    = new Font("Segoe UI", 8f);
        readonly Font FChat   = new Font("Segoe UI", 10f);
        readonly Font FLabel  = new Font("Segoe UI", 8f, FontStyle.Bold);
        readonly Font FInput  = new Font("Segoe UI", 10f);
        readonly Font FBtn    = new Font("Segoe UI", 9f, FontStyle.Bold);
        readonly Font FTopic  = new Font("Segoe UI", 8.5f, FontStyle.Bold);

        // ── Quick topics ──────────────────────────────────────────────────────
        static readonly string[] Topics = {
            "password","phishing","scam","privacy",
            "malware","ransomware","vpn","two-factor","firewall","social engineering"
        };
        static readonly Color[] TopicClr = {
            C(0,90,165), C(0,110,85), C(140,55,0),   C(88,0,128),
            C(140,24,24),C(0,100,130),C(70,70,0),     C(0,72,90),
            C(60,60,120),C(100,50,0)
        };

        // ── ASCII art ─────────────────────────────────────────────────────────
        static readonly string[] Ascii = {
            "  .::.  .:.  .::.  .::.  .::.  .::.  .:.  .::.  .::.  ",
            ".........................................................",
            "....................::............................:.......",
            "..........:=+**+=-:  .-=+*##*+=:.  .:=+**+=-:.........",
            "........:+#@@@@@@@@*=*@@@@@@@@@@@@*=*@@@@@@@@@@#=......",
            "......:+@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@+:...",
            "....:*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*:.",
            "...:*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*:",
            "...+@@@@@@@*=-----=*@@@@@@@@@@@*=-----=*@@@@@@@@@@@@@+",
            "...*@@@@@@@-  .::.  *@@@@@@@@@*  .::.  -@@@@@@@@@@@@@*",
            "...#@@@@@@@+  :@@:  +@@@@@@@@@+  :@@:  +@@@@@@@@@@@@@#",
            "...@@@@@@@@*  :@@:  *@@@@@@@@@*  :@@:  *@@@@@@@@@@@@@@",
            "...#@@@@@@@+  :@@:  +@@@@@@@@@+  :@@:  +@@@@@@@@@@@@@#",
            "...*@@@@@@@:  :@@:  :@@@@@@@@@:  :@@:  :@@@@@@@@@@@@@*",
            "...+@@@@@@@+=-+@@+=-+@@@@@@@@@+=-+@@+=-+@@@@@@@@@@@@@+",
            "...:*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*:.",
            ".....*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*:...",
            ".......:+#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@#=:......",
            "...........:=+*##@@@@@@@@@@@@@@@@@@@@@@##*+=:...........",
            "...............:--==+***########***+==--:...............",
            ".........................................................",
        };

        public MainForm()
        {
            _db.Init();
            _engine = new ChatbotEngine(_db);
            Build();
            PlayGreeting();
            ShowWelcome();

            var clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => {
                lblClock.Text = DateTime.Now.ToString("HH:mm:ss  |  dd MMM yyyy");
                CheckDueReminders();
            };
            clockTimer.Start();

            // Show DB connection status in status bar
            SetStatus(_db.IsConnected
                ? "DB: " + _db.StatusMsg
                : "DB offline — running in-memory mode", 
                _db.IsConnected ? AccGreen : C(255,160,50));
        }

        private void PlayGreeting()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (!File.Exists(path)) return;
                new Thread(() => {
                    try { new SoundPlayer(path).PlaySync(); } catch { }
                }) { IsBackground = true }.Start();
            }
            catch { }
        }

        // ═════════════════════════════════════════════════════════════════════
        // BUILD
        // ═════════════════════════════════════════════════════════════════════
        private void Build()
        {
            Text          = "Cybersecurity Awareness Chatbot — Part 3";
            Size          = new Size(1000, 700);
            MinimumSize   = new Size(800, 600);
            BackColor     = BgApp;
            ForeColor     = TxtMain;
            StartPosition = FormStartPosition.CenterScreen;
            Icon          = SystemIcons.Shield;

            BuildHeader();
            BuildMemBar();
            BuildTopicsBar();
            BuildChatArea();
            BuildInputBar();
            BuildStatusBar();

            statusBar.Dock = DockStyle.Bottom;
            pnlInput.Dock  = DockStyle.Bottom;
            pnlChat.Dock   = DockStyle.Fill;
            pnlTopics.Dock = DockStyle.Top;
            pnlMemBar.Dock = DockStyle.Top;
            pnlHeader.Dock = DockStyle.Top;

            Controls.Add(pnlChat);
            Controls.Add(pnlTopics);
            Controls.Add(pnlMemBar);
            Controls.Add(pnlHeader);
            Controls.Add(pnlInput);
            Controls.Add(statusBar);
        }

        // ── Header ────────────────────────────────────────────────────────────
        private void BuildHeader()
        {
            pnlHeader = new Panel { Height = 86, BackColor = BgHeader };
            pnlHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var b = new SolidBrush(AccBlue)) g.FillRectangle(b, 0, 0, 5, pnlHeader.Height);
                using (var p = new Pen(Border)) g.DrawLine(p, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            lblTitle = new Label {
                Text      = "CYBERSECURITY AWARENESS CHATBOT",
                Font      = FTitle,
                ForeColor = AccBlue,
                Location  = new Point(18, 12),
                AutoSize  = true
            };
            lblSub = new Label {
                Text      = "Keyword recognition  •  Sentiment detection  •  Memory  •  Task assistant  •  Quiz  •  Activity log",
                Font      = FSub,
                ForeColor = TxtMuted,
                Location  = new Point(20, 48),
                AutoSize  = true
            };
            lblOnline = new Label {
                Text      = "●  SECURE SESSION",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AccGreen,
                Location  = new Point(20, 66),
                AutoSize  = true
            };
            var blink = new System.Windows.Forms.Timer { Interval = 850 };
            blink.Tick += (s, e) => lblOnline.ForeColor =
                lblOnline.ForeColor == AccGreen ? BgHeader : AccGreen;
            blink.Start();

            // Three large action buttons right-anchored in the header
            var btnHdrTasks = MakeHeaderBtn("📋  TASKS",  C(0, 130, 90));
            var btnHdrQuiz  = MakeHeaderBtn("🎮  QUIZ",   C(160, 80, 0));
            var btnHdrLog   = MakeHeaderBtn("📜  LOG",    C(70, 0, 140));

            btnHdrTasks.Click += (s, e) => OpenTaskForm();
            btnHdrQuiz.Click  += (s, e) => OpenQuizForm();
            btnHdrLog.Click   += (s, e) => { txtInput.Text = "show activity log"; DoSend(); };

            pnlHeader.Resize += (s, e) =>
            {
                btnHdrLog.Location   = new Point(pnlHeader.Width - 136, 18);
                btnHdrQuiz.Location  = new Point(pnlHeader.Width - 266, 18);
                btnHdrTasks.Location = new Point(pnlHeader.Width - 396, 18);
            };

            pnlHeader.Controls.AddRange(new Control[]{ lblTitle, lblSub, lblOnline, btnHdrTasks, btnHdrQuiz, btnHdrLog });
        }

        private Button MakeHeaderBtn(string text, Color bg)
        {
            var b = new Button {
                Text      = text,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(120, 50),
                Cursor    = Cursors.Hand,
                Location  = new Point(0, 18)
            };
            b.FlatAppearance.BorderSize        = 0;
            b.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(
                    Math.Min(bg.R + 40, 255),
                    Math.Min(bg.G + 40, 255),
                    Math.Min(bg.B + 40, 255));
            return b;
        }

        // ── Memory bar ────────────────────────────────────────────────────────
        private void BuildMemBar()
        {
            pnlMemBar = new Panel { Height = 26, BackColor = BgMem, Visible = false };
            pnlMemBar.Paint += (s, e) =>
            {
                using (var p = new Pen(Border)) {
                    e.Graphics.DrawLine(p, 0, 0, pnlMemBar.Width, 0);
                    e.Graphics.DrawLine(p, 0, 25, pnlMemBar.Width, 25);
                }
            };

            lblMem = new Label {
                Text      = "🧠  MEMORY:",
                Font      = FMem,
                ForeColor = TxtMuted,
                Location  = new Point(10, 6),
                AutoSize  = true
            };
            pnlMemBar.Controls.Add(lblMem);
        }

        // ── Topics bar ────────────────────────────────────────────────────────
        private void BuildTopicsBar()
        {
            pnlTopics = new Panel { Height = 46, BackColor = BgTopics };
            pnlTopics.Paint += (s, e) =>
            {
                using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 45, pnlTopics.Width, 45);
            };

            var flow = new FlowLayoutPanel {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Padding       = new Padding(6, 6, 6, 0),
                AutoScroll    = true,
                BackColor     = BgTopics
            };

            var lbl = new Label { Text = "Quick:", Font = FMem, ForeColor = TxtMuted, AutoSize = true, Margin = new Padding(2, 6, 6, 0) };
            flow.Controls.Add(lbl);

            for (int i = 0; i < Topics.Length; i++)
            {
                string t = Topics[i]; Color clr = TopicClr[i];
                var b = QuickBtn(t, clr, () => { txtInput.Text = "What is " + t + "?"; DoSend(); });
                flow.Controls.Add(b);
            }

            // Separator
            flow.Controls.Add(new Label { Text = "|", Font = FMem, ForeColor = TxtMuted, AutoSize = true, Margin = new Padding(4, 6, 4, 0) });

            // Part 3 action buttons
            var bTasks = QuickBtn("📋  TASKS",  C(0,80,60),  OpenTaskForm);
            var bQuiz  = QuickBtn("🎮  QUIZ",   C(100,50,0), OpenQuizForm);
            var bLog   = QuickBtn("📜  LOG",    C(50,0,90),  () => { txtInput.Text = "show activity log"; DoSend(); });

            flow.Controls.Add(bTasks);
            flow.Controls.Add(bQuiz);
            flow.Controls.Add(bLog);

            pnlTopics.Controls.Add(flow);
        }

        private Button QuickBtn(string text, Color bg, Action onClick)
        {
            var b = new Button {
                Text      = text,
                Font      = FTopic,
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Height    = 28,
                AutoSize  = true,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(2, 0, 2, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => onClick();
            return b;
        }

        // ── Chat area ─────────────────────────────────────────────────────────
        private void BuildChatArea()
        {
            pnlChat = new Panel { BackColor = BgMsg };

            pnlMessages = new Panel { BackColor = BgMsg, Left = 0, Top = 0 };

            vsb = new VScrollBar { Dock = DockStyle.Right, Minimum = 0, Maximum = 0, Value = 0 };
            vsb.Scroll += (s, e) => pnlMessages.Top = -vsb.Value;

            pnlChat.Controls.Add(pnlMessages);
            pnlChat.Controls.Add(vsb);

            pnlChat.Resize += (s, e) =>
            {
                pnlMessages.Width = pnlChat.Width - vsb.Width;
                UpdateScrollbar();
            };
        }

        // ── Input bar ─────────────────────────────────────────────────────────
        private void BuildInputBar()
        {
            pnlInput = new Panel { Height = 54, BackColor = BgInput };
            pnlInput.Paint += (s, e) =>
            {
                using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 0, pnlInput.Width, 0);
            };

            txtInput = new TextBox {
                Font        = FInput,
                BackColor   = BgHeader,
                ForeColor   = TxtMain,
                BorderStyle = BorderStyle.FixedSingle,
                Location    = new Point(12, 12),
                Height      = 30,
                Anchor      = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            txtInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DoSend(); } };

            btnSend = new Button {
                Text      = "SEND  ►",
                Font      = FBtn,
                BackColor = AccBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(100, 30),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                Cursor    = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (s, e) => DoSend();

            pnlInput.Controls.AddRange(new Control[]{ txtInput, btnSend });
            pnlInput.Resize += (s, e) =>
            {
                txtInput.Width = pnlInput.Width - 126;
                btnSend.Left   = pnlInput.Width - 112;
                btnSend.Top    = 12;
            };
        }

        // ── Status bar ────────────────────────────────────────────────────────
        private void BuildStatusBar()
        {
            statusBar = new StatusStrip { BackColor = C(6, 12, 28), SizingGrip = false };
            lblStatus = new ToolStripStatusLabel("Ready") { Font = FSub, ForeColor = TxtMuted };
            var spring = new ToolStripStatusLabel { Spring = true };
            lblClock  = new ToolStripStatusLabel(DateTime.Now.ToString("HH:mm:ss")) { Font = FSub, ForeColor = C(45,75,115) };
            statusBar.Items.AddRange(new ToolStripItem[]{ lblStatus, spring, lblClock });
        }

        // ═════════════════════════════════════════════════════════════════════
        // WELCOME
        // ═════════════════════════════════════════════════════════════════════
        private void ShowWelcome()
        {
            int w = Math.Max(400, pnlMessages.Width - 28);

            var box = new Panel { BackColor = C(8, 18, 38), Left = 14, Top = _chatY, Width = w };
            box.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var p = new Pen(AccBlue, 1))
                using (var path = Rounded(new Rectangle(0, 0, box.Width - 1, box.Height - 1), 8))
                    e.Graphics.DrawPath(p, path);
            };

            int y = 10;
            var hdr = new Label { Text = "  CYBERSECURITY AWARENESS CHATBOT  —  PART 3", Font = new Font("Consolas", 9f, FontStyle.Bold), ForeColor = AccBlue, AutoSize = true, Location = new Point(10, y) };
            box.Controls.Add(hdr); y += hdr.PreferredHeight + 4;

            var sep = new Panel { BackColor = AccBlue, Left = 10, Top = y, Height = 1, Width = w - 20 };
            box.Controls.Add(sep); y += 6;

            var art = new Label { Text = string.Join("\r\n", Ascii), Font = new Font("Consolas", 7f), ForeColor = C(0, 120, 200), AutoSize = true, Location = new Point(10, y) };
            box.Controls.Add(art); y += art.PreferredHeight + 6;

            var sep2 = new Panel { BackColor = Border, Left = 10, Top = y, Height = 1, Width = w - 20 };
            box.Controls.Add(sep2); y += 6;

            var sub = new Label { Text = "  Tasks  •  Quiz  •  NLP  •  Activity Log  |  Keywords  •  Sentiment  •  Memory", Font = new Font("Consolas", 7.5f), ForeColor = TxtMuted, AutoSize = true, Location = new Point(10, y) };
            box.Controls.Add(sub); y += sub.PreferredHeight + 10;

            box.Height = y;
            pnlMessages.Controls.Add(box);
            _chatY += y + 10;
            UpdateScrollbar();

            PostBot("Welcome! I am CYBER-BOT, your cybersecurity advisor.\n\nUse the 📋 TASKS button to manage tasks, 🎮 QUIZ to test your knowledge, and 📜 LOG to view activity.\n\nFirst, what is your name?");
            SetStatus("Waiting for your name...", AccBlue);
        }

        // ═════════════════════════════════════════════════════════════════════
        // SEND
        // ═════════════════════════════════════════════════════════════════════
        private void DoSend()
        {
            string raw = txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(raw)) return;
            txtInput.Clear();
            txtInput.Focus();

            // Name collection on first message
            if (!_nameCollected)
            {
                string name = raw.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries)[0];
                name = char.ToUpper(name[0]) + (name.Length > 1 ? name.Substring(1).ToLower() : "");
                _engine.UserName    = name;
                _nameCollected      = true;
                pnlMemBar.Visible   = true;
                UpdateMemBar();
                _engine.Log.Add("Session", "Started for " + name);
                PostUser(raw);
                PostBot(_engine.GetResponse("hello"));
                SetStatus("Session active — " + name, AccGreen);
                return;
            }

            string low = raw.ToLower().Trim();
            if (low == "exit" || low == "quit" || low == "bye" || low == "goodbye")
            {
                PostUser(raw);
                string farewell = "Thank you" + (_engine.UserName != "" ? ", " + _engine.UserName : "") +
                                  " for using the Cybersecurity Awareness Chatbot! Stay safe online. 🛡️";
                PostBot(farewell);
                _engine.Log.Add("Session", "Ended by user");
                SetStatus("Session ended.", C(210, 70, 70));
                btnSend.Enabled  = false;
                txtInput.Enabled = false;
                return;
            }

            PostUser(raw);
            string response = _engine.GetResponse(raw);
            PostBot(response);
            UpdateMemBar();
            SetStatus("Response delivered.", AccGreen);
        }

        // ═════════════════════════════════════════════════════════════════════
        // OPEN FORMS
        // ═════════════════════════════════════════════════════════════════════
        private void OpenTaskForm()
        {
            _engine.Log.Add("Tasks", "Task Manager opened");
            var form = new TaskForm(_engine.Tasks, _engine.Log, msg =>
            {
                PostBot(msg);
                SetStatus(msg, AccGreen);
            });
            form.ShowDialog(this);
            SetStatus("Task Manager closed.", TxtMuted);
        }

        private void OpenQuizForm()
        {
            _engine.Log.Add("Quiz", "Quiz window opened");
            var form = new QuizForm(_quiz, _engine.Log, result =>
            {
                PostBot(result);
                SetStatus("Quiz finished!", AccGreen);
            });
            form.ShowDialog(this);
        }

        // ═════════════════════════════════════════════════════════════════════
        // BUBBLE FACTORY
        // ═════════════════════════════════════════════════════════════════════
        private void PostBot(string text)
        {
            int margin = 14;
            int maxW   = pnlMessages.Width - margin * 2 - 10;
            if (maxW < 100) maxW = 400;

            var bubble = new Panel { BackColor = BgBot, Left = margin, Top = _chatY, Width = maxW };
            bubble.Paint += (s, e) => PaintBubble(bubble, e, BgBot, AccBlue, false);

            var lbl = new Label { Text = "🤖  CYBER-BOT", Font = FLabel, ForeColor = AccBlue, AutoSize = true, Location = new Point(14, 10) };
            bubble.Controls.Add(lbl);
            var div = new Panel { BackColor = Border, Height = 1, Left = 14, Top = 28, Width = maxW - 28 };
            bubble.Controls.Add(div);

            var rtb = BuildRtb(text, TxtMain, BgBot, maxW - 28);
            rtb.Location = new Point(14, 36);
            bubble.Controls.Add(rtb);
            bubble.Height = rtb.Top + rtb.Height + 14;

            pnlMessages.Controls.Add(bubble);
            _chatY += bubble.Height + 8;
            pnlMessages.Height = Math.Max(pnlChat.Height, _chatY + 10);
            UpdateScrollbar();
            ScrollBottom();
        }

        private void PostUser(string text)
        {
            int margin = 14;
            int maxW   = (int)(pnlMessages.Width * 0.65);
            if (maxW < 100) maxW = 320;
            int left   = pnlMessages.Width - maxW - margin;

            var bubble = new Panel { BackColor = BgUser, Left = left, Top = _chatY, Width = maxW };
            bubble.Paint += (s, e) => PaintBubble(bubble, e, BgUser, C(0, 120, 210), true);

            var lbl = new Label { Text = "YOU  👤", Font = FLabel, ForeColor = AccGreen, AutoSize = true, Location = new Point(14, 10) };
            bubble.Controls.Add(lbl);
            var div = new Panel { BackColor = C(0, 100, 180), Height = 1, Left = 14, Top = 28, Width = maxW - 28 };
            bubble.Controls.Add(div);

            var rtb = BuildRtb(text, C(230, 245, 255), BgUser, maxW - 28);
            rtb.Location = new Point(14, 36);
            bubble.Controls.Add(rtb);
            bubble.Height = rtb.Top + rtb.Height + 14;

            pnlMessages.Controls.Add(bubble);
            _chatY += bubble.Height + 8;
            pnlMessages.Height = Math.Max(pnlChat.Height, _chatY + 10);
            UpdateScrollbar();
            ScrollBottom();
        }

        private RichTextBox BuildRtb(string text, Color fg, Color bg, int width)
        {
            var rtb = new RichTextBox {
                Text        = text,
                Font        = FChat,
                ForeColor   = fg,
                BackColor   = bg,
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.None,
                WordWrap    = true,
                DetectUrls  = false,
                Width       = width,
                Height      = 10
            };
            // Measure height
            pnlMessages.Controls.Add(rtb);
            var sz = rtb.GetPreferredSize(new Size(width, 0));
            pnlMessages.Controls.Remove(rtb);
            rtb.Height = Math.Max(sz.Height + 8, 22);
            return rtb;
        }

        // ── Rounded bubble painting ───────────────────────────────────────────
        private void PaintBubble(Panel p, PaintEventArgs e, Color bg, Color border, bool right)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using (var b = new SolidBrush(bg))
            using (var path = Rounded(r, 10))
                g.FillPath(b, path);
            using (var pen = new Pen(border, 1))
            using (var path = Rounded(r, 10))
                g.DrawPath(pen, path);
            using (var b = new SolidBrush(border))
            {
                if (!right) g.FillRectangle(b, 0, 10, 4, p.Height - 20);
                else        g.FillRectangle(b, p.Width - 4, 10, 4, p.Height - 20);
            }
        }

        private static GraphicsPath Rounded(Rectangle r, int rad)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X,             r.Y,              rad*2, rad*2, 180, 90);
            path.AddArc(r.Right-rad*2,   r.Y,              rad*2, rad*2, 270, 90);
            path.AddArc(r.Right-rad*2,   r.Bottom-rad*2,   rad*2, rad*2,   0, 90);
            path.AddArc(r.X,             r.Bottom-rad*2,   rad*2, rad*2,  90, 90);
            path.CloseFigure();
            return path;
        }

        // ── Scrollbar ─────────────────────────────────────────────────────────
        private void UpdateScrollbar()
        {
            int content = _chatY + 10;
            int visible = pnlChat.Height;
            if (content <= visible) { vsb.Visible = false; pnlMessages.Top = 0; }
            else
            {
                vsb.Visible  = true;
                vsb.Maximum  = content - visible + vsb.LargeChange;
                pnlMessages.Width = pnlChat.Width - vsb.Width;
            }
        }

        private void ScrollBottom()
        {
            if (!vsb.Visible) return;
            int max = Math.Max(0, _chatY + 10 - pnlChat.Height);
            vsb.Value      = Math.Min(max, vsb.Maximum - vsb.LargeChange + 1);
            pnlMessages.Top = -vsb.Value;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void UpdateMemBar()
        {
            string n = _engine.UserName   != "" ? "Name: " + _engine.UserName : "";
            string f = _engine.FavTopic   != "" ? "  |  Interest: " + _engine.FavTopic : "";
            lblMem.Text      = "🧠  MEMORY:   " + n + f;
            lblMem.ForeColor = AccBlue;
        }

        private void SetStatus(string msg, Color color)
        {
            if (statusBar.InvokeRequired)
                statusBar.Invoke(new Action(() => SetStatus(msg, color)));
            else { lblStatus.Text = msg; lblStatus.ForeColor = color; }
        }

        private void CheckDueReminders()
        {
            foreach (var task in _engine.Tasks.DueReminders())
            {
                task.ReminderDate = null; // fire once
                PostBot("⏰ Reminder: '" + task.Title + "' is due today!\n" + task.Description);
                _engine.Log.Add("Reminder", "Fired: " + task.Title);
            }
        }
    }
}
