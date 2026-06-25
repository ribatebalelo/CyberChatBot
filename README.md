# 🛡️ Cybersecurity Awareness Chatbot — Part 3

![Build Status](https://github.com/YOUR_USERNAME/CyberBot/actions/workflows/build.yml/badge.svg)
![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET%20Framework-4.8-purple)
![Language](https://img.shields.io/badge/language-C%23-green)
![Database](https://img.shields.io/badge/database-SQL%20Server%20LocalDB-red)

A Windows Forms cybersecurity awareness chatbot built in C# (.NET Framework 4.8) with natural language processing simulation, a task assistant backed by SQL Server, an interactive quiz, and a full activity log.

---

## 📸 Screenshots

> Main chat window with header action buttons

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  CYBERSECURITY AWARENESS CHATBOT          [📋 TASKS]  [🎮 QUIZ]  [📜 LOG]   │
│  Keyword recognition • Sentiment detection • Memory • Task assistant • Quiz  │
│  ● SECURE SESSION                                                            │
├──────────────────────────────────────────────────────────────────────────────┤
│  Quick: [password] [phishing] [scam] [privacy] [malware] [ransomware] ...   │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  🤖 CYBER-BOT                                                                │
│  ──────────────────────────────────────────────────────                      │
│  Welcome! I am CYBER-BOT, your cybersecurity advisor.                        │
│  Use the 📋 TASKS button to manage tasks, 🎮 QUIZ to                         │
│  test your knowledge, and 📜 LOG to view activity.                           │
│                                                                              │
│                                                          YOU  👤             │
│                                              ─────────────────────────────  │
│                                              What is phishing?               │
│                                                                              │
├──────────────────────────────────────────────────────────────────────────────┤
│  Type your message here...                                      [ SEND ► ]  │
├──────────────────────────────────────────────────────────────────────────────┤
│  DB: SQL Server LocalDB connected                      13:45:22 | 25 Jun 2026│
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## ✨ Features

### 💬 Part 1 & 2 — Core Chatbot
- **Keyword recognition** — detects cybersecurity topics in natural language
- **Sentiment detection** — recognises emotions (worried, confused, curious, etc.) and responds appropriately
- **Memory** — remembers your name and favourite topic across the session
- **Dynamic responses** — multiple response variants per topic, randomly selected
- **Follow-up tips** — type "tell me more" to get additional tips on the current topic
- **Greeting audio** — plays `greeting.wav` on startup

### 📋 Part 3 — Task Assistant
- Add cybersecurity tasks with title, description and optional reminder date
- View all tasks in a dedicated Task Manager window (ListView)
- Mark tasks as completed or delete them
- Due reminders fire automatically as chat bubbles
- All tasks persisted to **SQL Server LocalDB** — survives app restarts

### 🎮 Part 3 — Cybersecurity Quiz
- **12 questions** in the bank (8 multiple choice + 4 true/false)
- **10 random questions** served per game
- Immediate feedback after every answer with explanation
- Score tracking with motivational final message
- Dedicated popup quiz window with clickable answer buttons

### 🧠 Part 3 — NLP Simulation
- Understands many natural phrasings of the same command
- `"add task - Enable 2FA"` / `"create task: Enable 2FA"` / `"new task Enable 2FA"` — all work
- `"remind me in 7 days to update my password"` — parsed automatically
- `"complete task 1"` / `"mark task 1 done"` / `"task 1 done"` — all work
- Auto-generates task descriptions from common keywords (2FA, password, backup, VPN...)

### 📜 Part 3 — Activity Log
- Records every significant action with timestamp and category
- Categories: `Session`, `Tasks`, `Reminder`, `Quiz`, `NLP`, `Sentiment`, `Memory`, `Log`
- View via the **📜 LOG** button or by typing `"show activity log"`
- Shows last 10 actions; total count displayed

---

## 🗂️ Project Structure

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

## 🔄 CI/CD — GitHub Actions

Every push to `master` automatically builds the project.

The workflow (`.github/workflows/build.yml`):
1. Checks out the code on a `windows-latest` runner
2. Sets up MSBuild
3. Restores and builds `CyberBot.sln` in Release mode
4. Uploads `bin\Release\` as a downloadable artifact

To download the compiled build:
**GitHub → Actions → Latest run → Artifacts → CyberBot-Build**

---

## 🏫 Academic Information

| Detail | Value |
|--------|-------|
| Subject | Application Development |
| Part | 3 of 3 (Final POE) |
| Framework | .NET Framework 4.8 |
| UI | Windows Forms |
| Database | SQL Server LocalDB |
| Language | C# 7.3 |

---

## 📝 License

This project was created for academic purposes.
