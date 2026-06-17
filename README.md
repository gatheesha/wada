# Wada - A freelance companion app 

A  desktop application built for freelancers to manage **clients, projects, milestones, tasks, and finances** — all in one place.


## Features

- **Client Management** – Add, edit, search clients
- **Project Tracking** – Full project lifecycle with start date, duration, and status
- **Milestones & Tasks** – Hierarchical structure with deadlines and completion tracking
- **Finance Module** – Track earnings, expenses, and pending amounts with summaries
- **Invoice Generation** – Beautiful PDF invoices using QuestPDF
- **Dashboard** – Live earnings chart + upcoming tasks + active projects
- **Reports** – Interactive earnings/expenses trend charts with time filters
- **Real-time Countdowns** – Deadline tracking for projects and tasks


## Technologies

- **.NET 10** (Windows Desktop)
- **WPF** + **MahApps.Metro** (Modern UI)
- **SQLite** (via Microsoft.Data.Sqlite)
- **LiveCharts.Wpf** – Interactive charts
- **QuestPDF** – Professional PDF invoice generation


## Prerequisites

- **Windows 10 / 11**
- **.NET 10 SDK** (or later)
- Visual Studio


## Setup Instructions

### Clone / Download the Project

```bash
git clone https://github.com/gatheesha/wada.git
cd wada
```

### Build the Project

- Open `wada.sln` in Visual Studio
- Build → Rebuild Solution (`Ctrl+Shift+B`)
- Visual studio will download the required nuget packages automatically.

### Run the Application

Press `F5` or click the Run button.

The app will automatically create:
- `AppDatabase.db` (SQLite database)
- `profile.json` (your freelancer details)


## Project Structure

```
wada/
├── Data/              # DatabaseContext + ProfileStore
├── Models/            # Data models
├── ViewModels/        # MVVM logic
├── Views/             # XAML views
├── Dialogs/           # Modal dialogs
├── Services/          # Invoice PDF service
├── Converters/        # Value converters
└── MainWindow.xaml    # Navigation
```


## Database

- SQLite file: `AppDatabase.db` (created automatically in the executable folder)
- All data (projects, clients, tasks, finances) is persisted locally
- No internet or cloud required


## How to Use

1. **Dashboard** – Overview with charts and upcoming work
2. **Projects** – Manage projects, milestones, and tasks
3. **Clients** – Maintain client database
4. **Earnings** – Record income and expenses
5. **Invoices** – Fill profile → Select project → Generate PDF
6. **Reports** – View earnings trends over time


**Made for freelancers by freelancers <3.**