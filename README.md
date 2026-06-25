# 🛡️ Cybersecurity Awareness Chatbot — Part 3

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET%20Framework-4.8-purple)
![Language](https://img.shields.io/badge/language-C%23-green)
![Database](https://img.shields.io/badge/database-SQL%20Server%20LocalDB-red)

A Windows Forms cybersecurity awareness chatbot built in C# (.NET Framework 4.8) with natural language processing simulation, a task assistant backed by SQL Server, an interactive quiz, and a full activity log.

---

## Screenshots

### Main Chat Window
> Features the TASKS, QUIZ and LOG buttons in the header, quick-topic buttons, and the chat interface. The status bar confirms the SQL Server LocalDB connection.

<img width="986" height="695" alt="Screenshot 2026-06-25 173902" src="https://github.com/user-attachments/assets/a7bdffeb-ea0a-491f-84ac-f9e8ef0973ac" />


---

### 📋 Task Manager
> Dedicated task manager popup — add tasks with an optional reminder date, view all tasks in a list, and mark them complete or delete them. Tasks are saved to SQL Server LocalDB.

![Task Manager](screenshots/task_manager.png)

---

### 🎮 Cybersecurity Quiz
> 10-question quiz with multiple-choice and True/False questions. Immediate feedback shown after each answer with an explanation. Progress bar tracks current question.

![Cybersecurity Quiz](screenshots/quiz.png)

---

## ✨ Features

### 💬 Part 1 & 2 — Core Chatbot
- **Keyword recognition** — detects cybersecurity topics in natural language
- **Sentiment detection** — recognises emotions (worried, confused, curious, etc.) and responds appropriately
- **Memory** — remembers your name and favourite topic across the session
- **Dynamic responses** — multiple response variants per topic, randomly selected
- **Follow-up tips** — type "tell me more" to get additional tips on the current topic
- **Greeting audio** — plays `greeting.wav` on startup

### Part 3 — Task Assistant
- Add cybersecurity tasks with title, description and optional reminder date
- View all tasks in a dedicated Task Manager window (ListView)
- Mark tasks as completed or delete them
- Due reminders fire automatically as chat bubbles
- All tasks persisted to **SQL Server LocalDB** — survives app restarts

### Part 3 — Cybersecurity Quiz
- **12 questions** in the bank (8 multiple choice + 4 true/false)
- **10 random questions** served per game
- Immediate feedback after every answer with explanation
- Score tracking with motivational final message
- Dedicated popup quiz window with clickable answer buttons

### Part 3 — NLP Simulation
- Understands many natural phrasings of the same command
- `"add task - Enable 2FA"` / `"create task: Enable 2FA"` / `"new task Enable 2FA"` — all work
- `"remind me in 7 days to update my password"` — parsed automatically
- `"complete task 1"` / `"mark task 1 done"` / `"task 1 done"` — all work
- Auto-generates task descriptions from common keywords (2FA, password, backup, VPN...)

### Part 3 — Activity Log
- Records every significant action with timestamp and category
- Categories: `Session`, `Tasks`, `Reminder`, `Quiz`, `NLP`, `Sentiment`, `Memory`, `Log`
- View via the **📜 LOG** button or by typing `"show activity log"`
- Shows last 10 actions; total count displayed

---

## Project Structure

```
CyberBot/
│
├── .github/
│   └── workflows/
│       └── build.yml          # GitHub Actions CI pipeline
│
├── Properties/
│   └── AssemblyInfo.cs        # Assembly metadata
│
├── Models.cs                  # CyberTask, LogEntry, QuizQuestion classes
├── ActivityLog.cs             # Timestamped activity log
├── DatabaseManager.cs         # SQL Server LocalDB integration
├── TaskManager.cs             # Task CRUD — memory + database
├── QuizEngine.cs              # 12-question quiz logic
├── ChatbotEngine.cs           # NLP, keywords, sentiment, memory
├── QuizForm.cs                # Dedicated quiz popup window
├── TaskForm.cs                # Dedicated task manager window
├── MainForm.cs                # Main chat window
├── Program.cs                 # Entry point
│
├── greeting.wav               # Startup greeting audio
├── CyberBot.csproj            # Project file
├── CyberBot.sln               # Solution file
└── README.md                  # This file
```

---

## 🗄️ Database

Uses **SQL Server LocalDB** — built into Visual Studio, no separate install needed.

The app automatically creates the database and table on first run:

```sql
-- Auto-created on startup
CREATE DATABASE CyberBotDB;

CREATE TABLE dbo.Tasks (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(200)     NOT NULL,
    Description NVARCHAR(MAX)     NOT NULL,
    CreatedAt   DATETIME          NOT NULL,
    Reminder    DATE              NULL,
    Status      NVARCHAR(20)      NOT NULL DEFAULT 'Pending'
);
```

To view data in Visual Studio:
**View → SQL Server Object Explorer → (localdb)\MSSQLLocalDB → CyberBotDB → Tables → dbo.Tasks → Right-click → View Data**

---

## 🚀 Getting Started

### Prerequisites
- Windows 10 / 11
- Visual Studio 2019 or 2022
- .NET Framework 4.8 (included with Windows)
- SQL Server LocalDB (included with Visual Studio)

### Run Locally

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/CyberBot.git
cd CyberBot
```

1. Open **`CyberBot.sln`** in Visual Studio
2. Press **`Ctrl + Shift + B`** to build
3. Press **`F5`** to run

### Change Database Instance

If your LocalDB instance name differs, open `DatabaseManager.cs` and update:

```csharp
private const string INSTANCE = @"(localdb)\MSSQLLocalDB";  // change if needed
```

Common alternatives:
- `(localdb)\ProjectsV13` — Visual Studio 2017
- `.\SQLEXPRESS` — SQL Server Express

---

## 🤖 Supported Topics

| Topic | Example question |
|-------|-----------------|
| Passwords | "How do I create a strong password?" |
| Phishing | "What is phishing?" |
| Scams | "How do I spot an online scam?" |
| Privacy | "How do I protect my privacy?" |
| Malware | "What is malware?" |
| Ransomware | "How does ransomware work?" |
| VPN | "Do I need a VPN?" |
| Two-factor auth | "What is 2FA?" |
| Data breaches | "What should I do after a data breach?" |
| Firewalls | "What does a firewall do?" |
| Social engineering | "What is social engineering?" |

---

## 💬 Example Commands

```
# Task management
"add task - Enable two-factor authentication"
"remind me in 7 days to update my password"
"show tasks"
"complete task 1"
"delete task 2"

# Quiz
Click 🎮 QUIZ button

# Activity log
"show activity log"
"what have you done"

# Chat
"What is phishing?"
"tell me more"
"My name is John"
"I am interested in passwords"
```

---

## Academic Information

| Detail | Value |
|--------|-------|
| Subject | Application Development |
| Part | 3 of 3 (Final POE) |
| Framework | .NET Framework 4.8 |
| UI | Windows Forms |
| Database | SQL Server LocalDB |
| Language | C# 7.3 |

---

## License

This project was created for academic purposes.

---

## References

Alshaikh, M., Maynard, S.B., Ahmad, A. and Chang, S. (2018) 'Information security policy compliance: A systematic mapping and review', *Australasian Journal of Information Systems*, 22, pp. 1–24. Available at: https://doi.org/10.3127/ajis.v22i0.1395 (Accessed: 25 June 2026).

Anti-Phishing Working Group (APWG) (2023) *Phishing activity trends report: 4th quarter 2022*. Available at: https://apwg.org/trendsreports/ (Accessed: 25 June 2026).

Cialdini, R.B. (2021) *Influence: The psychology of persuasion*. Revised edn. New York: Harper Business.

Furnell, S. and Clarke, N. (2012) 'Power to the people? The evolving recognition of human aspects of security', *Computers and Security*, 31(8), pp. 983–988. Available at: https://doi.org/10.1016/j.cose.2012.08.004 (Accessed: 25 June 2026).

GitHub (2024) *GitHub Actions documentation*. Available at: https://docs.github.com/en/actions (Accessed: 25 June 2026).

Hunt, T. (2024) *Have I Been Pwned: Check if your email has been compromised in a data breach*. Available at: https://haveibeenpwned.com (Accessed: 25 June 2026).

Microsoft (2022) *System.Data.SqlClient namespace*. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient (Accessed: 25 June 2026).

Microsoft (2023a) *SQL Server Express LocalDB*. Available at: https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb (Accessed: 25 June 2026).

Microsoft (2023b) *Windows Forms overview (.NET Framework)*. Available at: https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview (Accessed: 25 June 2026).

Microsoft (2024a) *MSBuild reference*. Available at: https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-reference (Accessed: 25 June 2026).

Microsoft (2024b) *C# language reference*. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/ (Accessed: 25 June 2026).

Microsoft (2024c) *System.Text.RegularExpressions namespace*. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions (Accessed: 25 June 2026).

National Institute of Standards and Technology (NIST) (2023) *Cybersecurity framework version 2.0*. Gaithersburg, MD: NIST. Available at: https://doi.org/10.6028/NIST.CSWP.29 (Accessed: 25 June 2026).

Proofpoint (2023) *2023 state of the phish report*. Available at: https://www.proofpoint.com/us/resources/threat-reports/state-of-phish (Accessed: 25 June 2026).

Salahdine, F. and Kaabouch, N. (2019) 'Social engineering attacks: A survey', *Future Internet*, 11(4), p. 89. Available at: https://doi.org/10.3390/fi11040089 (Accessed: 25 June 2026).

Stallings, W. and Brown, L. (2018) *Computer security: Principles and practice*. 4th edn. Harlow: Pearson Education.

Symantec (2023) *Internet security threat report*. Available at: https://www.broadcom.com/support/security-center (Accessed: 25 June 2026).

Verizon (2023) *2023 data breach investigations report*. Available at: https://www.verizon.com/business/resources/reports/dbir/ (Accessed: 25 June 2026).

Von Solms, R. and Van Niekerk, J. (2013) 'From information security to cyber security', *Computers and Security*, 38, pp. 97–102. Available at: https://doi.org/10.1016/j.cose.2013.04.004 (Accessed: 25 June 2026).

Whitman, M.E. and Mattord, H.J. (2021) *Principles of information security*. 6th edn. Boston: Cengage Learning.
