using System;
using System.Collections.Generic;

namespace CyberBot
{
    public enum QuizState { Idle, InProgress, Finished }

    public class QuizEngine
    {
        // ── 12 questions: 8 multiple-choice + 4 true/false ───────────────────
        private static readonly List<QuizQuestion> Bank = new List<QuizQuestion>
        {
            new QuizQuestion {
                Question    = "What should you do if you receive an email asking for your password?",
                Options     = new[]{"Reply with your password","Delete the email","Report it as phishing","Ignore it"},
                AnswerIndex = 2,
                Explanation = "Reporting phishing emails helps security teams protect everyone and prevents the attacker reusing the same lure."
            },
            new QuizQuestion {
                Question    = "Which method of two-factor authentication (2FA) is most secure?",
                Options     = new[]{"SMS text message","Email one-time code","Authenticator app (TOTP)","Security question"},
                AnswerIndex = 2,
                Explanation = "Authenticator apps generate codes locally and cannot be intercepted via SIM-swapping, unlike SMS codes."
            },
            new QuizQuestion {
                Question    = "What does 'HTTPS' in a website address tell you?",
                Options     = new[]{"The site is fast","Your connection is encrypted","The site is government-owned","The site has no ads"},
                AnswerIndex = 1,
                Explanation = "HTTPS means the connection between your browser and the site is encrypted, protecting data in transit."
            },
            new QuizQuestion {
                Question    = "What is ransomware?",
                Options     = new[]{"Software that monitors browsing","Malware that encrypts files and demands payment","A type of firewall","An email spam filter"},
                AnswerIndex = 1,
                Explanation = "Ransomware encrypts your files and demands cryptocurrency to restore access. Prevention and offline backups are your best defence."
            },
            new QuizQuestion {
                Question    = "What does the 3-2-1 backup rule mean?",
                Options     = new[]{"3 passwords, 2 accounts, 1 email","3 copies, 2 media types, 1 offsite","3 firewalls, 2 VPNs, 1 antivirus","Back up every 3-2-1 days"},
                AnswerIndex = 1,
                Explanation = "Keep 3 copies of data, on 2 different media types, with 1 copy stored offsite or offline — critical protection against ransomware."
            },
            new QuizQuestion {
                Question    = "What is social engineering in cybersecurity?",
                Options     = new[]{"Hacking through software bugs","Manipulating people to reveal information","Building social media bots","Engineering social platforms"},
                AnswerIndex = 1,
                Explanation = "Social engineering exploits human psychology — trust, fear, urgency — rather than technical vulnerabilities."
            },
            new QuizQuestion {
                Question    = "Which of the following is NOT a sign of a phishing email?",
                Options     = new[]{"Urgent language ('Act now!')","Sender email does not match the brand","Email from a verified colleague's address","Link that hovers to a different URL"},
                AnswerIndex = 2,
                Explanation = "A verified email from a known colleague is generally safe. The other options are classic phishing red flags."
            },
            new QuizQuestion {
                Question    = "What website lets you check if your email was exposed in a data breach?",
                Options     = new[]{"google.com","haveibeenpwned.com","breachcheck.gov","securityscan.net"},
                AnswerIndex = 1,
                Explanation = "haveibeenpwned.com (by Troy Hunt) is a free, trusted service that checks your email against hundreds of breach databases."
            },
            // True/False questions
            new QuizQuestion {
                Question    = "TRUE or FALSE: Using the same password for multiple accounts is safe if the password is very long.",
                Options     = null,
                AnswerIndex = 1,  // False
                Explanation = "FALSE. Password reuse is dangerous — if one site is breached, attackers try that password everywhere (credential stuffing)."
            },
            new QuizQuestion {
                Question    = "TRUE or FALSE: A VPN makes you completely anonymous on the internet.",
                Options     = null,
                AnswerIndex = 1,  // False
                Explanation = "FALSE. A VPN hides your IP and encrypts traffic, but websites can still identify you via login accounts and browser fingerprinting."
            },
            new QuizQuestion {
                Question    = "TRUE or FALSE: You should click 'Unsubscribe' in spam emails to stop receiving them.",
                Options     = null,
                AnswerIndex = 1,  // False
                Explanation = "FALSE. Clicking 'Unsubscribe' in spam confirms your email is active, often resulting in MORE spam. Mark it as spam instead."
            },
            new QuizQuestion {
                Question    = "TRUE or FALSE: Public Wi-Fi is safe for online banking if the website uses HTTPS.",
                Options     = null,
                AnswerIndex = 1,  // False
                Explanation = "FALSE. Public Wi-Fi exposes you to rogue hotspots and man-in-the-middle attacks. Use a VPN on public networks."
            }
        };

        // ── State ─────────────────────────────────────────────────────────────
        private List<QuizQuestion> _active;
        private int  _current;
        private int  _score;

        public QuizState State    { get; private set; } = QuizState.Idle;
        public int CurrentNumber  => _current + 1;
        public int TotalQuestions => _active != null ? _active.Count : 0;
        public int Score          => _score;

        public QuizQuestion CurrentQuestion =>
            (_active != null && _current < _active.Count) ? _active[_current] : null;

        private readonly Random _rng = new Random();

        // ── Start ─────────────────────────────────────────────────────────────
        public void Start()
        {
            var pool = new List<QuizQuestion>(Bank);
            // Fisher-Yates shuffle
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
            }
            _active  = pool.GetRange(0, Math.Min(10, pool.Count));
            _current = 0;
            _score   = 0;
            State    = QuizState.InProgress;
        }

        // ── Submit answer; returns (correct, feedback) ────────────────────────
        public bool SubmitAnswer(int guessIndex, out string feedback)
        {
            var q       = _active[_current];
            bool correct = (guessIndex == q.AnswerIndex);
            if (correct) _score++;

            string correctLabel = q.IsTrueFalse
                ? (q.AnswerIndex == 0 ? "True" : "False")
                : ((char)('A' + q.AnswerIndex)) + ") " + q.Options[q.AnswerIndex];

            feedback = correct
                ? "CORRECT!  " + q.Explanation
                : "INCORRECT.  The correct answer was: " + correctLabel + "\n\n" + q.Explanation;

            _current++;
            if (_current >= _active.Count) State = QuizState.Finished;
            return correct;
        }

        public string FinalMessage()
        {
            double pct = (double)_score / _active.Count * 100;
            string grade = pct >= 80 ? "Excellent — you are a cybersecurity pro!"
                         : pct >= 50 ? "Good effort! Keep learning to sharpen your skills."
                         : "Keep learning — every bit of knowledge keeps you safer online.";
            return string.Format("You scored {0} / {1}  ({2:0}%)\n\n{3}",
                _score, _active.Count, pct, grade);
        }
    }
}
