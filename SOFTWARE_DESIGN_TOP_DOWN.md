# Success Planner MCP Software Design

## Purpose

This document defines the top-down software design for Success Planner MCP from application startup through each primary user workflow and application shutdown. It is written as a build guide: each component includes coding responsibilities and testing expectations.

Success Planner MCP is a solo, local-first personal success planner. The interface should remain simple enough to drive by mouse or touch, while the internal system manages tasks, focus sessions, movement, review, local storage, and Microsoft integrations.

## Design Goals

- Start quickly and show the Home screen without requiring sync to finish.
- Keep all daily actions point-and-click.
- Store local changes first, then sync in the background.
- Make every user action recoverable if sync fails.
- Keep Microsoft-specific details out of the main user interface.
- Test each component in isolation before connecting it to the full app.
- Support future phone companion sync without redesigning the core model.

## Top-Level Runtime Flow

```mermaid
flowchart TD
    A["App Launch"] --> B["Bootstrap"]
    B --> C["Load Settings"]
    C --> D["Open Local Database"]
    D --> E["Start Services"]
    E --> F["Show Home Screen"]
    F --> G["User Chooses Action"]
    G --> H["Capture"]
    G --> I["Today"]
    G --> J["Plan"]
    G --> K["Start Work"]
    G --> L["Done"]
    G --> M["Move"]
    G --> N["Review"]
    G --> O["Find"]
    G --> P["Settings"]
    H --> Q["Save Local Change"]
    I --> Q
    J --> Q
    K --> Q
    L --> Q
    M --> Q
    N --> Q
    O --> F
    P --> Q
    Q --> R["Queue Sync"]
    R --> S["Return to Screen"]
    S --> F
    F --> T["App Close Requested"]
    T --> U["Flush Local Saves"]
    U --> V["Persist Sync Queue"]
    V --> W["Stop Services"]
    W --> X["Close Database"]
    X --> Y["Exit"]
```

## Recommended Solution Structure

```text
SuccessPlannerMCP/
  src/
    SuccessPlanner.App/              WPF desktop app
    SuccessPlanner.Core/             domain objects and business rules
    SuccessPlanner.Application/      services and use cases
    SuccessPlanner.Infrastructure/   SQLite, settings, logging
    SuccessPlanner.Integrations/     Graph, Project COM, import/export
    SuccessPlanner.Phone/            future .NET MAUI companion

  tests/
    SuccessPlanner.Core.Tests/
    SuccessPlanner.Application.Tests/
    SuccessPlanner.Infrastructure.Tests/
    SuccessPlanner.Integrations.Tests/
    SuccessPlanner.App.Tests/

  docs/
    architecture/
    test-plans/
```

For the current repository, documents can remain at the root until the first code scaffold is created.

## Application Startup Design

### Startup Sequence

```mermaid
sequenceDiagram
    participant User
    participant App
    participant Bootstrapper
    participant Settings
    participant Database
    participant Services
    participant Home
    participant Sync

    User->>App: Open Success Planner MCP
    App->>Bootstrapper: Start()
    Bootstrapper->>Settings: Load or create defaults
    Bootstrapper->>Database: Open SQLite database
    Bootstrapper->>Services: Register services
    Bootstrapper->>Sync: Start background sync worker
    Bootstrapper->>Home: Show Home screen
    Home-->>User: Ready
```

### Components To Code

```text
AppBootstrapper
  LoadSettings()
  OpenDatabase()
  RegisterServices()
  StartBackgroundWorkers()
  ShowHome()

SettingsService
  Load()
  Save()
  CreateDefaults()

DatabaseService
  Open()
  Migrate()
  HealthCheck()
  Close()

AppShellViewModel
  CurrentScreen
  NavigateHome()
  NavigateTo(screen)
  ShowStatus(message)
```

### Startup Tests

- App creates default settings when no settings file exists.
- App opens an existing settings file without changing unrelated values.
- Database migration runs only when needed.
- App still opens Home screen if Microsoft sync is unavailable.
- Home screen shows local ready status within a short startup window.
- Startup failure shows a simple recovery message and writes a detailed log.

## Home Screen Design

The Home screen is the main control panel.

### User Controls

- Capture
- Today
- Plan
- Start
- Done
- Move
- Review
- Find
- Settings

### Components To Code

```text
HomeView
HomeViewModel
HomeCommand
NavigationService
StatusBadgeViewModel
```

### Home Tests

- Each tile navigates to the correct screen.
- Tile labels remain simple and nontechnical.
- Status badge can show Ready, Working, Syncing, Offline, and Needs Attention.
- Keyboard focus does not break mouse-first behavior.
- Layout remains usable at expected desktop window sizes.

## Capture Workflow

Capture is for quick ideas, tasks, project steps, reminders, or movement plans.

```mermaid
flowchart TD
    A["Click Capture"] --> B["Enter what it is"]
    B --> C["Choose date hint"]
    C --> D["Choose destination or Let MCP Choose"]
    D --> E["Save"]
    E --> F["Create TaskItem"]
    F --> G["Store locally"]
    G --> H["Queue sync"]
    H --> I["Show success"]
    I --> J["Return Home or Capture Another"]
```

### Components To Code

```text
CaptureView
CaptureViewModel
TaskService.CaptureTask()
PlanningService.ChooseDestination()
TaskRepository.Add()
SyncService.QueueChange()
```

### Capture Tests

- Empty title cannot be saved.
- A captured task can be saved with no date.
- Date buttons create correct date hints.
- Let MCP Choose assigns a reasonable default destination.
- Save writes the task to SQLite before any sync attempt.
- Failed sync does not lose the captured task.
- Capture Another clears the form and keeps the user on Capture.

## Today Workflow

Today shows a small, manageable set of important actions.

```mermaid
flowchart TD
    A["Click Today"] --> B["Load today's task cards"]
    B --> C["Select a task"]
    C --> D{"Choose action"}
    D --> E["Start"]
    D --> F["Done"]
    D --> G["Snooze"]
    D --> H["Add Note"]
    E --> I["Create focus session"]
    F --> J["Complete task"]
    G --> K["Update due date"]
    H --> L["Append note"]
    I --> M["Save local change"]
    J --> M
    K --> M
    L --> M
    M --> N["Queue sync"]
```

### Components To Code

```text
TodayView
TodayViewModel
TaskCardViewModel
TaskService.GetTodayTasks()
TaskService.CompleteTask()
TaskService.SnoozeTask()
TaskService.AddNote()
FocusService.StartSession()
```

### Today Tests

- Today loads only tasks that belong in the today view.
- Completed tasks disappear or move to Done feedback based on setting.
- Snooze moves the task to the selected date.
- Add Note preserves existing notes.
- Start creates a focus session with the selected task.
- The view stays useful with zero tasks, one task, and many tasks.

## Plan Workflow

Plan converts loose items into small, realistic next actions.

```mermaid
flowchart TD
    A["Click Plan"] --> B["Show unplanned inbox"]
    B --> C["Pick item"]
    C --> D["Split into smaller action if needed"]
    D --> E["Set priority"]
    E --> F["Set date"]
    F --> G["Set project"]
    G --> H["Choose minimum win"]
    H --> I["Save plan"]
    I --> J["Queue sync"]
```

### Components To Code

```text
PlanView
PlanViewModel
InboxItemViewModel
PlanningService.AssignPriority()
PlanningService.AssignDate()
PlanningService.AssignProject()
TaskService.SplitIntoTinySteps()
PlanningService.ChooseRealisticGoal()
```

### Plan Tests

- Unplanned inbox shows tasks without enough planning data.
- Split Into Tiny Steps creates child tasks or a checklist without losing the original.
- Minimum Win can be smaller than the stretch goal.
- Project assignment updates both task and project views.
- Planning changes are saved locally and queued for sync.

## Start Work Workflow

Start Work removes decision fatigue and encourages short focus sessions.

```mermaid
flowchart TD
    A["Click Start"] --> B["Suggest best next action"]
    B --> C["Pick 10, 15, or 20 minutes"]
    C --> D["Start timer"]
    D --> E{"Session result"}
    E --> F["Done"]
    E --> G["Pause"]
    E --> H["Blocked"]
    E --> I["Need break"]
    F --> J["Record small win"]
    G --> K["Save paused session"]
    H --> L["Mark blocked and ask for next tiny step"]
    I --> M["Suggest break"]
    J --> N["Suggest rotate project or continue"]
    K --> N
    L --> N
    M --> N
```

### Components To Code

```text
StartWorkView
StartWorkViewModel
FocusTimerViewModel
FocusService.SuggestNextSmallStep()
FocusService.StartSession()
FocusService.PauseSession()
FocusService.CompleteSession()
FocusService.SuggestBreak()
PlanningService.RotateProjectsAfterWin()
ReviewService.GetSmallWins()
```

### Start Work Tests

- Suggested next action is selected from available planned tasks.
- User can override the suggestion.
- Timer can start, pause, resume, and complete.
- 20 minutes is the default session length.
- Done records a small win.
- Blocked status does not mark the task complete.
- After completion, the app can suggest rotating to another project.

## Done Workflow

Done is a fast completion path.

```mermaid
flowchart TD
    A["Click Done"] --> B["Show recent and active tasks"]
    B --> C["Select task"]
    C --> D["Confirm Done"]
    D --> E["Set completed date"]
    E --> F["Record win"]
    F --> G["Queue sync"]
    G --> H["Show simple success feedback"]
```

### Components To Code

```text
DoneView
DoneViewModel
TaskService.CompleteTask()
ReviewService.RecordWin()
SyncService.QueueChange()
```

### Done Tests

- Completing a task sets status and completed date.
- Completing a task twice is harmless.
- Completion is stored locally before sync.
- Completion feedback is brief and encouraging.
- The completed task appears in Review.

## Move Workflow

Move makes physical activity a first-class part of the success system.

```mermaid
flowchart TD
    A["Click Move"] --> B["Choose Walk / Workout / Stretch"]
    B --> C["Choose Now or Schedule"]
    C --> D["Choose mind occupier"]
    D --> E["Optionally mark with spouse"]
    E --> F["Start or save movement task"]
    F --> G["Record movement plan"]
    G --> H["Queue sync"]
    H --> I["Done or Return Home"]
```

### Components To Code

```text
MoveView
MoveViewModel
MovementService.ScheduleWalk()
MovementService.ScheduleWorkout()
MovementService.SuggestMindOccupier()
MovementService.CompleteMovement()
TaskService.CaptureTask()
```

### Move Tests

- Walk, workout, and stretch create valid physical activity tasks.
- Schedule creates a future task with a date/time.
- Start Now creates an active movement session.
- Mind occupier selection is saved in notes or metadata.
- Movement completion appears in Review as a success win.

## Review Workflow

Review helps the user learn and adjust without turning the app into a performance judge.

```mermaid
flowchart TD
    A["Click Review"] --> B["Load week summary"]
    B --> C["Show small wins"]
    B --> D["Show stuck items"]
    B --> E["Show needs decision"]
    C --> F["Choose next week focus"]
    D --> G["Break stuck item smaller"]
    E --> H["Plan or snooze"]
    F --> I["Save review"]
    G --> I
    H --> I
```

### Components To Code

```text
ReviewView
ReviewViewModel
ReviewService.GetWeeklySummary()
ReviewService.GetSmallWins()
ReviewService.GetStuckItems()
ReviewService.GetNeedsDecisionItems()
PlanningService.ChooseRealisticGoal()
TaskService.SplitIntoTinySteps()
```

### Review Tests

- Review includes completed tasks, focus sessions, and movement wins.
- Stuck items are tasks with blocked status or repeated snoozes.
- Needs Decision items are visible without overwhelming the screen.
- Save Review persists selected next focus.
- Review works with no data and with a full week of data.

## Find Workflow

Find searches the local database first.

```mermaid
flowchart TD
    A["Click Find"] --> B["Enter search text"]
    B --> C["Search tasks, projects, notes"]
    C --> D["Show results"]
    D --> E["Open local item"]
    E --> F["Optional open source item"]
```

### Components To Code

```text
FindView
FindViewModel
SearchService.SearchTasks()
SearchService.SearchProjects()
SearchService.SearchNotes()
IExternalTaskAdapter.OpenSourceItem()
```

### Find Tests

- Search finds task titles.
- Search finds notes.
- Search handles no results.
- Search does not require Microsoft sync.
- Open Source Item calls the correct adapter when a source link exists.

## Settings Workflow

Settings is where technical details may exist, but still in plain language.

```mermaid
flowchart TD
    A["Click Settings"] --> B["General"]
    A --> C["Connections"]
    A --> D["Colors"]
    A --> E["Rules"]
    B --> F["Save settings"]
    C --> G["Test connections"]
    D --> F
    E --> F
```

### Components To Code

```text
SettingsView
SettingsViewModel
SettingsService.Load()
SettingsService.Save()
ConnectionTestService.TestTodo()
ConnectionTestService.TestPlanner()
ConnectionTestService.TestProjectDesktop()
DestinationRuleService.SaveRules()
```

### Settings Tests

- Settings load from disk.
- Settings save to disk.
- Invalid settings are rejected with a simple message.
- Connection tests do not block the main UI.
- Destination rules are validated before saving.

## Sync Design

All user-facing changes follow the same rule: save local first, then sync.

```mermaid
flowchart TD
    A["User action"] --> B["Save to SQLite"]
    B --> C["Create SyncQueueItem"]
    C --> D["Background worker picks item"]
    D --> E{"Destination"}
    E --> F["Microsoft To Do"]
    E --> G["Planner"]
    E --> H["Project COM"]
    E --> I["Phone companion"]
    F --> J["Mark synced or failed"]
    G --> J
    H --> J
    I --> J
```

### Components To Code

```text
SyncService
SyncQueueRepository
SyncWorker
TodoGraphAdapter
PlannerGraphAdapter
ProjectComAdapter
PhoneCompanionAdapter
ConflictResolver
```

### Sync Tests

- Queue item is created for each sync-worthy change.
- Sync retry count increases after failure.
- Failed sync keeps local data intact.
- Successful sync updates SourceLink and LastSyncedAt.
- Adapter failures are logged without crashing the app.
- Conflict resolver preserves the newest safe local change by default.

## Local Database Design

Suggested initial tables:

```text
Tasks
Projects
Milestones
Notes
FocusSessions
SuccessGoals
MovementSessions
SourceLinks
SyncQueue
Settings
ReviewEntries
```

### Database Tests

- Database can be created from empty.
- Migrations are repeatable.
- Repositories can add, update, delete, and query records.
- Local data survives app restart.
- Database closes cleanly during shutdown.

## Application Shutdown Design

Shutdown should be calm and protective. The app should not lose work because a sync is slow.

```mermaid
sequenceDiagram
    participant User
    participant AppShell
    participant Services
    participant Database
    participant Sync
    participant Log

    User->>AppShell: Close app
    AppShell->>Services: Request shutdown
    Services->>Database: Flush pending local saves
    Services->>Sync: Persist queue and stop worker
    Services->>Log: Write shutdown summary
    Services->>Database: Close connection
    AppShell-->>User: Exit
```

### Components To Code

```text
ShutdownService
  RequestShutdown()
  FlushLocalChanges()
  StopBackgroundWorkers()
  PersistSyncQueue()
  CloseDatabase()

AppShellViewModel
  CanClose()
  Close()
```

### Shutdown Tests

- App closes cleanly with no pending work.
- App closes cleanly with unsynced queue items.
- Pending local edits are flushed before exit.
- Background sync worker stops within timeout.
- Next launch resumes unsynced queue items.

## Testing Strategy

### Unit Tests

Test pure business rules:

- task status changes
- date hint conversion
- priority assignment
- tiny-step splitting
- focus session timing rules
- movement task creation
- review summary calculations

### Integration Tests

Test real infrastructure with controlled dependencies:

- SQLite repositories
- settings file read/write
- sync queue persistence
- adapter interface contracts
- Project desktop detection when available

### UI Tests

Test user paths:

- launch to Home
- Capture and save task
- Today Start and Done
- Plan an inbox item
- Start a 20-minute session
- Create a Move activity
- Review weekly progress
- close and reopen with data intact

### Manual Acceptance Tests

Before each milestone is considered done:

- A nontechnical user can complete the workflow by clicking only.
- No main screen exposes API, Graph, OAuth, COM, VBA, database, or adapter language.
- The app remains useful when offline.
- The app does not show more decisions than needed.
- The user can recover from a failed sync without losing the task.

## Build Order

```text
1. Core domain objects
2. SQLite database and repositories
3. App bootstrap and Home screen
4. Capture workflow
5. Today workflow
6. Done workflow
7. Start Work timer workflow
8. Move workflow
9. Plan workflow
10. Review workflow
11. Find workflow
12. Settings workflow
13. Sync queue
14. Microsoft To Do adapter
15. Project desktop detection and import
16. Planner availability test
17. Phone companion quick capture
```

## Definition Of Done For Each Component

Each component is complete only when:

- Code is implemented.
- Unit tests pass.
- Relevant integration tests pass.
- User-facing text is simple and nontechnical.
- Failure state is handled.
- Local data is not lost if sync fails.
- The component is documented enough for future maintenance.

