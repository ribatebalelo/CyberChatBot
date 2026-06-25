using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CyberBot
{
    public class ChatbotEngine
    {
        // ── Public sub-systems ────────────────────────────────────────────────
        public readonly TaskManager  Tasks;
        public readonly ActivityLog  Log   = new ActivityLog();

        public ChatbotEngine(DatabaseManager db)
        {
            Tasks = new TaskManager(db);
        }

        // ── Memory ────────────────────────────────────────────────────────────
        private readonly Dictionary<string, string> _memory =
            new Dictionary<string, string>();

        private string _lastTopic = null;
        private readonly Random _rng = new Random();

        public string UserName
        {
            get { return _memory.ContainsKey("name") ? _memory["name"] : ""; }
            set { _memory["name"] = value; }
        }
        public string FavTopic
        {
            get { return _memory.ContainsKey("fav") ? _memory["fav"] : ""; }
            set { _memory["fav"] = value; }
        }

        // ── Keyword responses ─────────────────────────────────────────────────
        private readonly Dictionary<string, List<string>> _tips =
            new Dictionary<string, List<string>>
        {
            ["phishing"] = new List<string> {
                "Phishing is when cybercriminals send fake emails pretending to be a trusted source to trick you into revealing passwords or personal data. Always hover over links before clicking to check the real destination URL.",
                "Phishing attacks create a false sense of urgency with messages like 'Your account will be closed!' Legitimate organisations never pressure you this way. Verify through official channels before acting.",
                "Types of phishing include: Email phishing (mass fake emails), Spear phishing (targeted attacks using your personal details), Smishing (fake SMS), and Vishing (phone call scams). Always verify the sender independently.",
                "To protect yourself: never click links in unsolicited emails, type URLs directly into your browser, check for HTTPS, use anti-phishing extensions, and report suspicious emails to your IT department."
            },
            ["password"] = new List<string> {
                "A strong password is at least 12 characters and mixes uppercase, lowercase, numbers and symbols. Never use personal information like your name, birthday or pet's name.",
                "Never reuse the same password across multiple accounts. If one site is breached, attackers try that password everywhere — a technique called credential stuffing.",
                "A password manager like Bitwarden (free), 1Password or Dashlane generates and securely stores unique passwords for all your accounts. You only need to remember one master password.",
                "Enable two-factor authentication (2FA) on all accounts in addition to a strong password. Even if your password is stolen, an attacker cannot log in without your second factor."
            },
            ["scam"] = new List<string> {
                "Online scams include lottery scams, tech support scams, romance scams, and investment fraud. The common thread is a request for money or personal information from someone you have not verified.",
                "If you receive an unexpected call or message asking for money or personal details — stop. Hang up and contact the organisation directly using a phone number from their official website.",
                "Romance scammers build trust over weeks before asking for money with a sudden 'crisis'. Always verify identities through a live video call and never send money to someone you have not met in person.",
                "Report scams to your national cybercrime unit. In South Africa, report to SABRIC or the SAPS Commercial Crime Unit. Your report helps protect others."
            },
            ["privacy"] = new List<string> {
                "Review privacy settings on all your social media accounts. Set posts to 'Friends only', disable location sharing, and never publicly display your phone number or home address.",
                "Use a VPN whenever you connect to public Wi-Fi. A VPN encrypts your entire internet connection so anyone on that network cannot intercept your data.",
                "Audit which apps have access to your camera, microphone, location and contacts. Go to Settings > Apps > Permissions and revoke any permissions the app does not genuinely need.",
                "Use a privacy-focused browser like Firefox or Brave with the DuckDuckGo search engine. These tools do not track your searches or sell your data to advertisers."
            },
            ["malware"] = new List<string> {
                "Malware includes viruses, spyware, adware and trojans. It can steal data, encrypt files, log keystrokes, or turn your device into part of a criminal network.",
                "Keep your operating system and all applications updated. Software updates contain security patches that close the vulnerabilities malware exploits. Enable automatic updates wherever possible.",
                "Only download software from official sources — the developer's website, Microsoft Store, Apple App Store, or Google Play. Cracked versions of paid software almost always contain hidden malware.",
                "Install reputable antivirus software such as Windows Defender (free and built into Windows), Malwarebytes, or ESET. Run full system scans weekly."
            },
            ["ransomware"] = new List<string> {
                "Ransomware encrypts all files on your device and demands payment — usually cryptocurrency — to restore access. Prevention is the only reliable defence.",
                "Follow the 3-2-1 backup rule: keep 3 copies of your data, on 2 different media types, with 1 copy stored completely offline. Ransomware cannot encrypt a backup it cannot reach.",
                "Ransomware almost always arrives through phishing emails with malicious attachments. Never open an email attachment you were not expecting, even if it appears to come from someone you know.",
                "If hit by ransomware: disconnect from the internet immediately, do not pay the ransom, report it to law enforcement, and restore from a clean backup."
            },
            ["vpn"] = new List<string> {
                "A VPN creates an encrypted tunnel between your device and the internet, hiding your traffic from your internet provider and hackers on the same network. Essential on public Wi-Fi.",
                "When choosing a VPN, look for: a no-logs policy independently audited, AES-256 encryption, a kill switch, and a reputable company. Good options: ProtonVPN, Mullvad, ExpressVPN.",
                "A VPN protects your internet traffic but does not make you fully anonymous. Websites can still identify you through login accounts and browser fingerprinting."
            },
            ["two-factor"] = new List<string> {
                "Two-factor authentication (2FA) requires two separate proofs of identity: something you know (password) and something you have (a one-time code). Even if your password is stolen, attackers cannot log in without the second factor.",
                "Use an authenticator app like Google Authenticator, Microsoft Authenticator, or Authy instead of SMS-based 2FA. SMS codes can be intercepted through SIM-swapping attacks.",
                "When you enable 2FA, store the backup codes in a secure offline location. If you lose your phone, backup codes are the only way to regain access."
            },
            ["data breach"] = new List<string> {
                "A data breach occurs when unauthorised persons access a company's systems and steal user data. Breaches happen regularly, and your data from old accounts is often at risk.",
                "Visit haveibeenpwned.com and enter your email address. This free service checks your email against hundreds of known breach databases and tells you exactly what was exposed.",
                "If your data has been breached: change the affected password immediately, enable 2FA, monitor bank statements for suspicious activity, and consider placing a fraud alert with your bank."
            },
            ["firewall"] = new List<string> {
                "A firewall monitors all network traffic and blocks traffic that looks suspicious or unauthorised, acting as a gatekeeper between your device and the internet.",
                "Make sure your built-in firewall is switched on. On Windows: Settings > Windows Security > Firewall. On macOS: System Preferences > Security & Privacy > Firewall.",
                "Your router also has a built-in firewall. Log into your router admin panel (usually 192.168.0.1) and ensure the firewall is enabled. Change the default admin password if you have not done so."
            },
            ["social engineering"] = new List<string> {
                "Social engineering manipulates people into giving up confidential information. Unlike hacking, it exploits human psychology — trust, fear, and urgency — rather than technical vulnerabilities.",
                "Common tactics: pretexting (false scenario), baiting (infected USB drives), quid pro quo (offering something for information), and tailgating (following authorised people through secure doors).",
                "Defend yourself by always verifying a person's identity independently before sharing any information. If someone claims to be from IT or your bank, hang up and call back on the official number."
            }
        };

        // ── Sentiment openers ─────────────────────────────────────────────────
        private readonly Dictionary<string, string> _sentiment =
            new Dictionary<string, string>
        {
            ["worried"]     = "It is understandable to feel worried — cybersecurity threats are very real. But knowledge is your best protection, and you are already taking the right step by asking.",
            ["scared"]      = "Your concern shows you take your safety seriously. Let me give you some practical steps that will help you feel more in control.",
            ["confused"]    = "Cybersecurity can feel overwhelming at first — no worries at all. Let me explain this in clear, simple terms.",
            ["frustrated"]  = "I completely understand the frustration. Security issues are stressful. Let us work through this together, one step at a time.",
            ["curious"]     = "That curiosity is exactly the right attitude! Staying informed is one of the most powerful things you can do for your digital safety.",
            ["anxious"]     = "Take a breath — you are already doing the right thing by seeking information. Here is what you need to know.",
            ["overwhelmed"] = "It can feel like a lot to take in at once. Let us focus on the most important step first and build from there."
        };

        private readonly List<string> _greetings = new List<string> {
            "hello","hi","hey","good morning","good afternoon","good evening","howdy","greetings"
        };
        private readonly List<string> _followUps = new List<string> {
            "tell me more","more","another tip","explain more","go on",
            "continue","elaborate","another one","give me more","more tips","what else"
        };

        // ═════════════════════════════════════════════════════════════════════
        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type a message so I can help you.";

            string raw = input.Trim();
            string low = raw.ToLower();

            // 1. Activity log
            if (Contains(low, "show activity log","activity log","what have you done",
                              "show log","recent actions","show history","what actions"))
            {
                Log.Add("Log", "User viewed activity log");
                return Log.FormatRecent(10);
            }

            // 2. Task commands (NLP)
            string taskResp = HandleTaskNlp(low, raw);
            if (taskResp != null) return taskResp;

            // 3. Greeting
            if (_greetings.Any(g => WholeWord(low, g)))
            {
                string n = UserName != "" ? ", " + UserName : "";
                return "Hello" + n + "! I am CYBER-BOT, your personal cybersecurity advisor.\n\n" +
                       "I can help you with:\n" +
                       "  Passwords  |  Phishing  |  Scams  |  Privacy\n" +
                       "  Malware    |  Ransomware |  VPN   |  Two-factor authentication\n" +
                       "  Data breaches  |  Firewalls  |  Social engineering\n\n" +
                       "Part 3 features:\n" +
                       "  Click TASKS to manage your cybersecurity task list\n" +
                       "  Click QUIZ to test your knowledge with a 8-question quiz\n" +
                       "  Type 'show activity log' to see everything I have done\n\n" +
                       "What would you like to know about?";
            }

            // 4. Help
            if (low == "help" || Contains(low, "what can you do","list topics","show topics"))
            {
                return "TOPICS I CAN HELP WITH:\n\n" +
                       "  passwords, phishing, scam, privacy, malware,\n" +
                       "  ransomware, vpn, two-factor, data breach,\n" +
                       "  firewall, social engineering\n\n" +
                       "COMMANDS:\n" +
                       "  'show activity log'  — view recent actions\n" +
                       "  Click TASKS button  — manage cybersecurity tasks\n" +
                       "  Click QUIZ button   — start the cybersecurity quiz\n\n" +
                       "Just type naturally — I understand many ways of asking!";
            }

            // 5. How are you
            if (Contains(low, "how are you","how r u","how do you do"))
            {
                string n = UserName != "" ? ", " + UserName : "";
                return "I am running perfectly" + n + " and ready to help keep you safe online! What cybersecurity topic would you like to explore?";
            }

            // 6. Name capture
            foreach (string prefix in new[] { "my name is ","i am ","i'm ","call me " })
            {
                int idx = low.IndexOf(prefix);
                if (idx >= 0)
                {
                    string after = low.Substring(idx + prefix.Length).Trim();
                    string name  = after.Split(new[]{' ','.','!',','}, StringSplitOptions.RemoveEmptyEntries)
                                        .FirstOrDefault() ?? "";
                    if (name.Length > 1)
                    {
                        name = char.ToUpper(name[0]) + name.Substring(1);
                        UserName = name;
                        Log.Add("Memory", "Name remembered: " + name);
                        return "Great to meet you, " + name + "! I will remember your name. What cybersecurity question can I help you with?";
                    }
                }
            }

            // 7. Interest capture
            foreach (string phrase in new[]{"i'm interested in ","i am interested in ","i want to learn about ","tell me about "})
            {
                if (low.Contains(phrase))
                {
                    string after = low.Substring(low.IndexOf(phrase) + phrase.Length).Trim().TrimEnd(new char[]{'.'}) ;
                    string kw    = _tips.Keys.FirstOrDefault(k => after.Contains(k));
                    if (kw != null)
                    {
                        FavTopic   = kw;
                        _lastTopic = kw;
                        Log.Add("Memory", "Interest: " + kw);
                        string n = UserName != "" ? ", " + UserName : "";
                        return "Great" + n + "! I will remember that you are interested in " + kw + ".\n\n" + RandomTip(kw);
                    }
                }
            }

            // 8. Follow-up
            if (_followUps.Any(f => low.Contains(f)))
            {
                if (_lastTopic != null && _tips.ContainsKey(_lastTopic))
                {
                    Log.Add("NLP", "Follow-up on: " + _lastTopic);
                    return "Here is another tip on " + _lastTopic + ":\n\n" + RandomTip(_lastTopic);
                }
                return "Please ask a cybersecurity question first and I will give you more tips on that topic!";
            }

            // 9. Sentiment
            foreach (var kvp in _sentiment)
            {
                if (low.Contains(kvp.Key))
                {
                    Log.Add("Sentiment", "Detected: " + kvp.Key);
                    string kw = DetectKeyword(low) ?? _lastTopic;
                    if (kw != null) { _lastTopic = kw; return kvp.Value + "\n\n" + RandomTip(kw); }
                    return kvp.Value + "\n\nFeel free to ask about any cybersecurity topic — I am here to help!";
                }
            }

            // 10. Keyword
            string keyword = DetectKeyword(low);
            if (keyword != null)
            {
                _lastTopic = keyword;
                Log.Add("NLP", "Keyword: " + keyword);
                string prefix2 = (FavTopic == keyword && FavTopic != "")
                    ? "As someone interested in " + keyword + ", here is a tip:\n\n"
                    : "";
                return prefix2 + RandomTip(keyword);
            }

            // 11. Default
            Log.Add("NLP", "Unrecognised: " + raw.Substring(0, Math.Min(30, raw.Length)));
            return "I am not quite sure I understood that. Could you rephrase?\n\n" +
                   "Try asking about: passwords, phishing, scams, privacy, malware,\n" +
                   "ransomware, vpn, two-factor, data breaches, firewalls, or social engineering.\n\n" +
                   "Type 'help' for the full list of commands.";
        }

        // ═════════════════════════════════════════════════════════════════════
        // TASK NLP
        // ═════════════════════════════════════════════════════════════════════
        public string HandleTaskNlp(string low, string raw)
        {
            // VIEW
            if (Contains(low,"show tasks","list tasks","my tasks","view tasks",
                             "all tasks","task list","pending tasks","show my tasks"))
            {
                Log.Add("Tasks", "User viewed task list");
                return FormatTaskList();
            }

            // COMPLETE
            string completeId = MatchFirst(low, new[]{
                @"complete task\s+(\d+)", @"mark task\s+(\d+)\s+done",
                @"task\s+(\d+)\s+done",  @"done task\s+(\d+)",
                @"finish task\s+(\d+)"
            });
            if (completeId != null && int.TryParse(completeId, out int cid))
            {
                
                if (Tasks.Complete(cid)) { Log.Add("Tasks", "Completed #" + cid); return "Task #" + cid + " marked as completed! Well done on staying cyber-safe."; }
                return "Task #" + cid + " not found. Type 'show tasks' to see your list.";
            }

            // DELETE
            string deleteId = MatchFirst(low, new[]{
                @"delete task\s+(\d+)", @"remove task\s+(\d+)", @"cancel task\s+(\d+)"
            });
            if (deleteId != null && int.TryParse(deleteId, out int did))
            {
                if (Tasks.Delete(did)) { Log.Add("Tasks", "Deleted #" + did); return "Task #" + did + " has been deleted."; }
                return "Task #" + did + " not found. Type 'show tasks' to see your list.";
            }

            // REMIND ME IN N DAYS
            var remMatch = Regex.Match(low, @"remind me in (\d+) days?\s+(?:to\s+)?(.+)");
            if (remMatch.Success)
            {
                int days = int.Parse(remMatch.Groups[1].Value);
                string title = Cap(remMatch.Groups[2].Value.Trim().TrimEnd(new char[]{'.'}) );
                DateTime rem = DateTime.Today.AddDays(days);
                string desc  = AutoDesc(title);
                Tasks.Add(title, desc, rem);
                Log.Add("Tasks", "Added with reminder: " + title + " in " + days + " days");
                return "Task added: '" + title + "'\nReminder set for " + rem.ToString("dd MMM yyyy") +
                       " (" + days + " day" + (days == 1 ? "" : "s") + " from now).\n\n" + desc;
            }

            // ADD TASK
            string addTitle = MatchFirst(low, new[]{
                @"add task[-:\s]+(.+)",
                @"create task[-:\s]+(.+)",
                @"new task[-:\s]+(.+)",
                @"add a task\s+(?:to\s+)?(.+)"
            });
            if (addTitle != null)
            {
                string title = Cap(addTitle.TrimEnd(new char[]{'.'}) );
                string desc  = AutoDesc(title);
                Tasks.Add(title, desc);
                Log.Add("Tasks", "Added: " + title);
                return "Task added!\n\nTitle: " + title + "\nDescription: " + desc +
                       "\n\nWould you like a reminder? Type: 'remind me in N days to " + title + "'";
            }

            return null;
        }

        private string FormatTaskList()
        {
            var tasks = Tasks.AllTasks.ToList();
            if (!tasks.Any()) return "You have no tasks yet. Use the TASKS button or type 'add task <title>' to create one.";
            var sb = new StringBuilder("Your Cybersecurity Tasks:\n\n");
            foreach (var t in tasks)
            {
                sb.AppendLine(t.StatusIcon + " #" + t.Id + "  " + t.Title);
                sb.AppendLine("       " + t.Description);
                sb.AppendLine("       " + t.ReminderText);
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════════════════
        private string DetectKeyword(string low) =>
            _tips.Keys.FirstOrDefault(k => low.Contains(k));

        private string RandomTip(string topic) =>
            _tips[topic][_rng.Next(_tips[topic].Count)];

        private static bool Contains(string input, params string[] phrases) =>
            phrases.Any(p => input.Contains(p));

        private static bool WholeWord(string input, string word)
        {
            int idx = input.IndexOf(word);
            if (idx < 0) return false;
            bool before = idx == 0 || !char.IsLetter(input[idx - 1]);
            bool after  = idx + word.Length >= input.Length || !char.IsLetter(input[idx + word.Length]);
            return before && after;
        }

        private static string MatchFirst(string input, string[] patterns)
        {
            foreach (var pat in patterns)
            {
                var m = Regex.Match(input, pat);
                if (m.Success && m.Groups.Count > 1) return m.Groups[1].Value.Trim();
            }
            return null;
        }

        private static string Cap(string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);

        private static string AutoDesc(string title)
        {
            string t = title.ToLower();
            if (t.Contains("2fa") || t.Contains("two-factor") || t.Contains("two factor"))
                return "Enable two-factor authentication to add an extra security layer to your accounts.";
            if (t.Contains("password"))
                return "Update your password to a strong, unique passphrase of at least 12 characters.";
            if (t.Contains("privacy") || t.Contains("settings"))
                return "Review account privacy settings to ensure your personal data is protected.";
            if (t.Contains("backup"))
                return "Create a secure offline backup following the 3-2-1 rule.";
            if (t.Contains("update") || t.Contains("patch"))
                return "Apply the latest security updates to your operating system and applications.";
            if (t.Contains("antivirus") || t.Contains("malware"))
                return "Install or update antivirus software and run a full system scan.";
            if (t.Contains("vpn"))
                return "Set up a trusted VPN to encrypt your internet connection on public networks.";
            if (t.Contains("firewall"))
                return "Enable and configure your system firewall to block unauthorised network access.";
            return "Complete this cybersecurity action: " + title + ".";
        }
    }
}
