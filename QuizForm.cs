using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CyberBot
{
    public class QuizForm : Form
    {
        private readonly QuizEngine      _quiz;
        private readonly ActivityLog     _log;
        private readonly Action<string>  _onQuizEnd; // callback to post result to chat

        // Controls
        private Panel       pnlHeader;
        private Label       lblTitle;
        private Label       lblProgress;
        private ProgressBar pbProgress;
        private Label       lblScore;
        private Panel       pnlQuestion;
        private Label       lblQNum;
        private Label       lblQuestion;
        private Panel       pnlOptions;
        private Button[]    _optBtns;
        private Button      btnTrue, btnFalse;
        private Panel       pnlFeedback;
        private Label       lblFeedback;
        private Button      btnNext;
        private Panel       pnlResult;
        private Label       lblResult;
        private Button      btnPlayAgain;
        private Button      btnClose;

        // Colours
        static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);
        readonly Color BgDark    = C(12, 22, 42);
        readonly Color BgPanel   = C(16, 32, 62);
        readonly Color BgInput   = C(8, 18, 38);
        readonly Color AccBlue   = C(0, 162, 255);
        readonly Color AccGreen  = C(0, 210, 120);
        readonly Color AccRed    = C(230, 70, 70);
        readonly Color TxtMain   = C(215, 235, 255);
        readonly Color TxtMuted  = C(100, 140, 185);
        readonly Color Border    = C(28, 68, 120);

        public QuizForm(QuizEngine quiz, ActivityLog log, Action<string> onQuizEnd)
        {
            _quiz      = quiz;
            _log       = log;
            _onQuizEnd = onQuizEnd;

            Text            = "Cybersecurity Quiz";
            Size            = new Size(640, 580);
            MinimumSize     = new Size(580, 520);
            BackColor       = BgDark;
            ForeColor       = TxtMain;
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Icon            = SystemIcons.Information;

            BuildUI();
            StartQuiz();
        }

        private void BuildUI()
        {
            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = BgPanel };
            pnlHeader.Paint += (s, e) =>
            {
                using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 69, pnlHeader.Width, 69);
                using (var b = new SolidBrush(AccBlue)) e.Graphics.FillRectangle(b, 0, 0, 5, 70);
            };

            lblTitle = new Label {
                Text      = "🎮  CYBERSECURITY QUIZ",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AccBlue,
                Location  = new Point(18, 12),
                AutoSize  = true
            };
            lblProgress = new Label {
                Text      = "Question 1 of 10",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TxtMuted,
                Location  = new Point(18, 44),
                AutoSize  = true
            };
            lblScore = new Label {
                Text      = "Score: 0",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AccGreen,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize  = true,
                Location  = new Point(520, 44)
            };
            pbProgress = new ProgressBar {
                Style     = ProgressBarStyle.Continuous,
                Height    = 4,
                Location  = new Point(0, 66),
                Width     = 640,
                Minimum   = 0,
                Maximum   = 10,
                Value     = 0,
                BackColor = BgPanel,
                ForeColor = AccBlue
            };

            pnlHeader.Controls.AddRange(new Control[]{ lblTitle, lblProgress, lblScore, pbProgress });

            // Question panel
            pnlQuestion = new Panel {
                Dock      = DockStyle.Top,
                Height    = 140,
                BackColor = BgInput,
                Padding   = new Padding(20, 16, 20, 16)
            };
            pnlQuestion.Paint += (s, e) =>
            {
                using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 139, pnlQuestion.Width, 139);
            };

            lblQNum = new Label {
                Text      = "Q1",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AccBlue,
                Location  = new Point(20, 14),
                AutoSize  = true
            };
            lblQuestion = new Label {
                Text      = "",
                Font      = new Font("Segoe UI", 11f),
                ForeColor = TxtMain,
                Location  = new Point(20, 38),
                Width     = 590,
                Height    = 90,
                AutoSize  = false
            };

            pnlQuestion.Controls.AddRange(new Control[]{ lblQNum, lblQuestion });

            // Options panel
            pnlOptions = new Panel {
                Dock      = DockStyle.Top,
                Height    = 200,
                BackColor = BgDark,
                Padding   = new Padding(20, 10, 20, 10)
            };

            _optBtns = new Button[4];
            string[] letters = { "A", "B", "C", "D" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                _optBtns[i] = MakeOptionBtn(letters[i], "", 20 + i * 44);
                _optBtns[i].Click += (s, e) => SubmitAnswer(idx);
                pnlOptions.Controls.Add(_optBtns[i]);
            }

            btnTrue  = MakeOptionBtn("T", "True",  20);
            btnFalse = MakeOptionBtn("F", "False", 70);
            btnTrue.Click  += (s, e) => SubmitAnswer(0);
            btnFalse.Click += (s, e) => SubmitAnswer(1);
            pnlOptions.Controls.Add(btnTrue);
            pnlOptions.Controls.Add(btnFalse);

            // Feedback panel
            pnlFeedback = new Panel {
                Dock      = DockStyle.Top,
                Height    = 130,
                BackColor = C(8, 30, 60),
                Padding   = new Padding(20, 12, 20, 12),
                Visible   = false
            };
            pnlFeedback.Paint += (s, e) =>
            {
                using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 0, pnlFeedback.Width, 0);
            };

            lblFeedback = new Label {
                Text      = "",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TxtMain,
                Location  = new Point(20, 14),
                Width     = 580,
                Height    = 80,
                AutoSize  = false
            };
            btnNext = new Button {
                Text      = "Next Question  ▶",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = AccBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(20, 96),
                Size      = new Size(145, 28),
                Cursor    = Cursors.Hand
            };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += (s, e) => NextQuestion();
            pnlFeedback.Controls.AddRange(new Control[]{ lblFeedback, btnNext });

            // Result panel (shown at end)
            pnlResult = new Panel {
                Dock      = DockStyle.Fill,
                BackColor = BgDark,
                Visible   = false
            };

            lblResult = new Label {
                Font      = new Font("Segoe UI", 12f),
                ForeColor = TxtMain,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill
            };
            btnPlayAgain = new Button {
                Text      = "🔄  Play Again",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = AccBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(150, 38),
                Cursor    = Cursors.Hand
            };
            btnPlayAgain.FlatAppearance.BorderSize = 0;
            btnPlayAgain.Click += (s, e) => StartQuiz();

            btnClose = new Button {
                Text      = "✖  Close",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = C(80, 24, 24),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(120, 38),
                Cursor    = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            var pnlResultBtns = new FlowLayoutPanel {
                Dock          = DockStyle.Bottom,
                Height        = 60,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor     = BgPanel,
                Padding       = new Padding(16, 10, 16, 0)
            };
            pnlResultBtns.Controls.Add(btnPlayAgain);
            pnlResultBtns.Controls.Add(btnClose);

            pnlResult.Controls.Add(lblResult);
            pnlResult.Controls.Add(pnlResultBtns);

            // Stack panels (note: Dock=Top stacks bottom-up in Controls order)
            Controls.Add(pnlResult);
            Controls.Add(pnlFeedback);
            Controls.Add(pnlOptions);
            Controls.Add(pnlQuestion);
            Controls.Add(pnlHeader);
        }

        private Button MakeOptionBtn(string letter, string text, int top)
        {
            string label = string.IsNullOrEmpty(text) ? letter + ")  " : letter + ")  " + text;
            return new Button {
                Text      = label,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = TxtMain,
                BackColor = BgPanel,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(20, top),
                Size      = new Size(590, 38),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor    = Cursors.Hand,
                Tag       = letter
            };
        }

        // ── Quiz flow ─────────────────────────────────────────────────────────
        private void StartQuiz()
        {
            _quiz.Start();
            _log.Add("Quiz", "Quiz started");
            pnlResult.Visible   = false;
            pnlFeedback.Visible = false;
            pnlOptions.Visible  = true;
            pnlQuestion.Visible = true;
            ShowCurrentQuestion();
        }

        private void ShowCurrentQuestion()
        {
            var q = _quiz.CurrentQuestion;
            if (q == null) return;

            lblQNum.Text      = "Question " + _quiz.CurrentNumber;
            lblQuestion.Text  = q.Question;
            lblProgress.Text  = "Question " + _quiz.CurrentNumber + " of " + _quiz.TotalQuestions;
            lblScore.Text     = "Score: " + _quiz.Score;
            pbProgress.Value  = _quiz.CurrentNumber - 1;
            pnlFeedback.Visible = false;

            // Enable all buttons
            foreach (var b in _optBtns) { b.BackColor = BgPanel; b.Enabled = true; }
            btnTrue.BackColor  = BgPanel; btnTrue.Enabled  = true;
            btnFalse.BackColor = BgPanel; btnFalse.Enabled = true;

            if (q.IsTrueFalse)
            {
                foreach (var b in _optBtns) b.Visible = false;
                btnTrue.Visible  = true;
                btnFalse.Visible = true;
                pnlOptions.Height = 130;
            }
            else
            {
                btnTrue.Visible  = false;
                btnFalse.Visible = false;
                string[] letters = { "A", "B", "C", "D" };
                for (int i = 0; i < 4; i++)
                {
                    _optBtns[i].Text    = letters[i] + ")  " + q.Options[i];
                    _optBtns[i].Visible = true;
                    _optBtns[i].Location = new Point(20, 10 + i * 46);
                }
                pnlOptions.Height = 210;
            }
        }

        private void SubmitAnswer(int guessIndex)
        {
            // Disable all buttons
            foreach (var b in _optBtns) b.Enabled = false;
            btnTrue.Enabled  = false;
            btnFalse.Enabled = false;

            bool correct = _quiz.SubmitAnswer(guessIndex, out string feedback);
            _log.Add("Quiz", "Q" + (_quiz.CurrentNumber - 1) + ": " + (correct ? "correct" : "incorrect"));

            // Highlight chosen button
            Color chosen = correct ? C(0, 80, 40) : C(80, 20, 20);
            {
                // Identify pressed button
                Button pressedBtn = null;
                if (btnTrue.Visible  && guessIndex == 0) pressedBtn = btnTrue;
                if (btnFalse.Visible && guessIndex == 1) pressedBtn = btnFalse;
                if (pressedBtn == null && guessIndex < _optBtns.Length) pressedBtn = _optBtns[guessIndex];
                if (pressedBtn != null) pressedBtn.BackColor = chosen;
            }

            lblFeedback.ForeColor = correct ? AccGreen : AccRed;
            lblFeedback.Text      = (correct ? "✅  " : "❌  ") + feedback;
            pnlFeedback.Visible   = true;

            if (_quiz.State == QuizState.Finished)
            {
                btnNext.Text = "See Results  ▶";
                _log.Add("Quiz", "Completed — score " + _quiz.Score + "/" + _quiz.TotalQuestions);
            }
            else
            {
                btnNext.Text = "Next Question  ▶";
            }
        }

        private void NextQuestion()
        {
            if (_quiz.State == QuizState.Finished)
            {
                ShowResult();
            }
            else
            {
                pnlFeedback.Visible = false;
                ShowCurrentQuestion();
            }
        }

        private void ShowResult()
        {
            pnlOptions.Visible  = false;
            pnlQuestion.Visible = false;
            pnlFeedback.Visible = false;
            pnlResult.Visible   = true;

            string msg = _quiz.FinalMessage();
            lblResult.Text = "🏆  QUIZ COMPLETE!\n\n" + msg;

            // Post to chat
            _onQuizEnd("Quiz completed: " + msg.Split('\n')[0]);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var p = new Pen(Border))
                e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
        }
    }
}
