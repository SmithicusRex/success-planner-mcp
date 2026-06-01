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
- Make each completed milestone visible in the running app.
- Test each component in isolation before connecting it to the full app.
- Support future phone companion sync without redesigning the core model.

## Implementation Pattern

Development should follow visible vertical slices. A backend service may be built first, but it is only a sub-step. A workflow component is not complete until the user can see it, click it, and observe its current state in the app.

Each workflow should move through this order:

```text
1. Domain model
2. Service or repository
3. View model
4. View
5. Navigation from Home
6. User-visible status or diagnostics
7. Unit tests
8. Integration tests where relevant
9. Manual click-through acceptance
```

For infrastructure-only work, such as settings, database, sync queue, or adapters, the app must still provide visual insight before the milestone is considered complete. That may be a real workflow screen, a status panel, or a plain-language diagnostics screen.

Example:

```text
SettingsService coded
  -> not complete as a user milestone

SettingsService + SettingsViewModel + SettingsView + Home navigation + visible loaded settings
  -> complete Settings workflow milestone
```

This keeps the project honest: the code may be complex under the hood, but every finished step should make the app more understandable from the outside.

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

### Visible Settings Requirements

The Settings workflow must show the user what the app actually loaded.

The first Settings screen should display:

- Profile name
- Default focus minutes
- Sync on launch status
- Large controls setting
- Enabled connections: To Do, Planner, Project Desktop, Phone Companion
- Destination rules summary
- Settings file status: Loaded defaults, Loaded from file, Recovered from backup, or Recreated after invalid file

The first Settings screen does not need to expose every edit control. It does need to prove visually that the SettingsService is active and understandable.

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
- The running app shows visible evidence of the new capability.
- No main screen exposes API, Graph, OAuth, COM, VBA, database, or adapter language.
- The app remains useful when offline.
- The app does not show more decisions than needed.
- The user can recover from a failed sync without losing the task.

## Code Development Roadmap

This roadmap is the controlling implementation sequence. Each major step has a benchmark gate. The next major step should not begin until the benchmark gate for the current step is met.

Benchmarks must describe a distinct completed accomplishment. They should not include future refinements, deferred cleanup, or placeholder caveats. If a future refinement is known, it must appear as its own later roadmap step with its own benchmark gate.

### ~~1. App Bootstrap And Home Shell~~

~~1.1. Create the WPF app project and solution file.~~

~~1.2. Implement `AppBootstrapper`.~~

~~1.3. Load settings during startup.~~

~~1.4. Open the local data service during startup.~~

~~1.5. Start background workers without blocking the Home screen.~~

~~1.6. Show the Home screen with the main action tiles.~~

~~1.7. Implement clean shutdown that stops workers and closes local services.~~

~~Benchmark gate:~~

- ~~App launches from desktop shortcut.~~
- ~~Home screen appears quickly.~~
- ~~Status shows `Ready`.~~
- ~~App closes without error.~~
- ~~`dotnet build SuccessPlannerMCP.slnx` passes.~~

Status:

- ~~Complete.~~

### ~~2. Navigation Shell And Screen Host~~

~~2.1. Create a shared screen contract for app pages.~~

~~2.2. Add `NavigationService`.~~

~~2.3. Add screen host area to `MainWindow`.~~

~~2.4. Wire Home tile buttons to navigation commands.~~

~~2.5. Add Back and Home controls where appropriate.~~

~~2.6. Add initial visible screens for Capture, Today, Plan, Start, Done, Move, Review, Find, and Settings.~~

~~Benchmark gate:~~

- ~~Every Home tile opens a visible initial screen.~~
- ~~Settings and Find buttons open visible screens.~~
- ~~User can return Home by clicking.~~
- ~~No screen exposes technical implementation labels.~~
- ~~Build passes.~~

### ~~3. Settings Workflow Visible Slice~~

~~3.1. Finish `SettingsService` local load/save behavior.~~

~~3.2. Create `SettingsViewModel`.~~

~~3.3. Create `SettingsView`.~~

~~3.4. Show profile name, focus minutes, sync-on-launch, display settings, connection toggles, and destination rules summary.~~

~~3.5. Show settings file status in plain language.~~

~~3.6. Add Save and Cancel behavior.~~

~~3.7. Validate settings before saving.~~

~~3.8. Show recovery status when invalid settings were recreated.~~

~~Benchmark gate:~~

- ~~User can open Settings from the app.~~
- ~~User can see the settings that were loaded.~~
- ~~User can change at least profile name and default focus minutes.~~
- ~~User can save settings and see changes after app restart.~~
- ~~Corrupt settings file does not prevent launch.~~
- ~~Build passes.~~

### ~~4. Core Domain Objects~~

~~4.1. Create `TaskItem`.~~

~~4.2. Create `ProjectItem`.~~

~~4.3. Create `MilestoneItem`.~~

~~4.4. Create `NoteItem`.~~

~~4.5. Create `FocusSession`.~~

~~4.6. Create `SuccessGoal`.~~

~~4.7. Create `MovementSession`.~~

~~4.8. Create `SourceLink`.~~

~~4.9. Add enums for status, priority, source system, and sync state.~~

~~Benchmark gate:~~

- ~~Domain objects compile in a separate core area or namespace.~~
- ~~Objects represent Capture, Today, Start, Move, and Review needs.~~
- ~~Unit tests cover basic object creation and status transitions.~~
- ~~Build and tests pass.~~

### ~~5. SQLite Database And Repositories~~

~~5.1. Replace the bootstrap local data marker with SQLite.~~

~~5.2. Add database migration structure.~~

~~5.3. Create tables for tasks, projects, milestones, notes, focus sessions, movement sessions, source links, settings metadata, and sync queue.~~

~~5.4. Implement repositories for tasks and settings metadata first.~~

~~5.5. Add database health check.~~

~~5.6. Add app startup migration.~~

~~Benchmark gate:~~

- ~~App creates a real local SQLite database.~~
- ~~App can restart without losing local data.~~
- ~~Repository tests pass.~~
- ~~Database failure shows a simple user-facing error.~~
- ~~Build and tests pass.~~

### ~~6. Capture Workflow~~

~~6.1. Create `CaptureViewModel`.~~

~~6.2. Create `CaptureView`.~~

~~6.3. Add task title input.~~

~~6.4. Add date hint buttons.~~

~~6.5. Add destination choice with `Let MCP Choose`.~~

~~6.6. Save captured task locally.~~

~~6.7. Show success feedback and Capture Another option.~~

~~Benchmark gate:~~

- ~~User can create a task by clicking through Capture.~~
- ~~Task persists after restart.~~
- ~~Empty task title is rejected politely.~~
- ~~Capture works without Microsoft sync.~~
- ~~Build and tests pass.~~

### ~~7. Today Workflow~~

~~7.1. Create `TodayViewModel`.~~

~~7.2. Create `TodayView`.~~

~~7.3. Load tasks due today or selected for today.~~

~~7.4. Show tasks as cards.~~

~~7.5. Add Start, Done, Snooze, and Note actions.~~

~~7.6. Save each action locally.~~

~~Benchmark gate:~~

- ~~User can see today tasks.~~
- ~~User can mark a today task done.~~
- ~~User can snooze a task.~~
- ~~Changes survive restart.~~
- ~~Build and tests pass.~~

### ~~8. Done Workflow~~

~~8.1. Create `DoneViewModel`.~~

~~8.2. Create `DoneView`.~~

~~8.3. Show recent active tasks.~~

~~8.4. Complete selected task.~~

~~8.5. Record completion as a small win.~~

~~8.6. Show brief success feedback.~~

~~Benchmark gate:~~

- ~~User can complete a task from Done screen.~~
- ~~Completed task appears in Review data.~~
- ~~Completing twice does not corrupt data.~~
- ~~Build and tests pass.~~

### ~~9. Start Work Timer Workflow~~

~~9.1. Create `StartWorkViewModel`.~~

~~9.2. Create `StartWorkView`.~~

~~9.3. Suggest a best next action.~~

~~9.4. Add 10, 15, and 20 minute session choices.~~

~~9.5. Implement start, pause, resume, complete, and blocked states.~~

~~9.6. Record focus session locally.~~

~~9.7. Suggest a break after session completion.~~

~~Benchmark gate:~~

- ~~User can start a 20-minute focus session.~~
- ~~Timer state is visible.~~
- ~~Completed session is saved.~~
- ~~Blocked session does not mark task complete.~~
- ~~Build and tests pass.~~

### ~~10. Move Workflow~~

~~10.1. Create `MoveViewModel`.~~

~~10.2. Create `MoveView`.~~

~~10.3. Add Walk, Workout, and Stretch choices.~~

~~10.4. Add Now and Schedule choices.~~

~~10.5. Add mind occupier options.~~

~~10.6. Add spouse option.~~

~~10.7. Save movement activity locally.~~

~~Benchmark gate:~~

- ~~User can schedule or start a movement activity.~~
- ~~Movement activity appears as a success item in Review.~~
- ~~Movement works without Microsoft sync.~~
- ~~Build and tests pass.~~

### ~~11. Plan Workflow~~

~~11.1. Create `PlanViewModel`.~~

~~11.2. Create `PlanView`.~~

~~11.3. Show unplanned inbox.~~

~~11.4. Add priority, date, project, and minimum win controls.~~

~~11.5. Add split-into-tiny-steps behavior.~~

~~11.6. Save planning changes locally.~~

~~Benchmark gate:~~

- ~~User can turn an inbox item into a planned small action.~~
- ~~Minimum win is visible and saved.~~
- ~~Tiny steps are created without losing the original task.~~
- ~~Build and tests pass.~~

### ~~12. Review Workflow~~

~~12.1. Create `ReviewViewModel`.~~

~~12.2. Create `ReviewView`.~~

~~12.3. Show small wins.~~

~~12.4. Show stuck items.~~

~~12.5. Show needs-decision items.~~

~~12.6. Let user choose next focus.~~

~~12.7. Add focus-session and movement-session success items to Review.~~

~~Benchmark gate:~~

- ~~Review screen summarizes completed tasks, focus sessions, and movement.~~
- ~~Stuck items are visible.~~
- ~~User can save next focus.~~
- ~~Build and tests pass.~~

### 13. Find Workflow

~~13.1. Create `FindViewModel`.~~

~~13.2. Create `FindView`.~~

~~13.3. Search task titles, notes, projects, and source links.~~

~~13.4. Open selected local item.~~

13.5. Add an inactive Open Source control that clearly shows when no source item is connected.

Benchmark gate:

- ~~User can search local data.~~
- ~~Empty results are handled gracefully.~~
- ~~Search does not require Microsoft sync.~~
- ~~Selected search result opens as a local item.~~
- ~~Build and tests pass.~~

### ~~14. Sync Queue With Visible Status~~

~~14.1. Create sync queue table.~~

~~14.2. Create `SyncQueueRepository`.~~

~~14.3. Add `SyncService`.~~

~~14.4. Add background sync worker.~~

~~14.5. Show sync status in app header or diagnostics.~~

~~14.6. Add retry and failure state.~~

~~Benchmark gate:~~

- ~~Sync queue table is created by migration.~~
- ~~Sync queue repository can add, read, update, and delete queue items.~~
- ~~Local changes create queue items.~~
- ~~Background sync worker starts and picks ready queue items.~~
- ~~Header or diagnostics shows sync state.~~
- ~~Failed sync does not lose local data.~~
- ~~Queue survives app restart.~~
- ~~Build and tests pass.~~

### 15. Microsoft To Do Adapter

~~15.1. Add Microsoft To Do connection status model.~~

~~15.2. Add the first usable authentication or connection-test path.~~

~~15.3. Pull To Do lists and tasks where available.~~

~~15.4. Push captured tasks to To Do where configured.~~

15.5. Show To Do connection status in Settings.

Benchmark gate:

- Settings shows To Do connection status.
- ~~App can test whether To Do integration is available.~~
- ~~Successful sync updates source links.~~
- Failed sync remains visible and recoverable.
- ~~Build and tests pass.~~

### 16. Microsoft Project Desktop Detection And Import

16.1. Detect Project desktop executable.

16.2. Add Project connection status in Settings.

16.3. Add local Project file selection.

16.4. Import Project tasks through the selected automation path.

16.5. Map Project tasks into local domain objects.

Benchmark gate:

- Settings shows Project desktop detection status.
- User can select or detect a Project source.
- Imported tasks appear locally.
- Import failure is visible and recoverable.
- Build and tests pass.

### 17. Planner Availability Test

17.1. Add Planner connection status model.

17.2. Test whether Planner data is accessible for the current account.

17.3. Show Planner status in Settings.

17.4. Add read-only Planner task import if available.

Benchmark gate:

- Settings shows Planner availability.
- App explains unavailable Planner access in plain language.
- Available Planner tasks can be shown locally.
- Build and tests pass.

### 18. Phone Companion Quick Capture

18.1. Define phone companion sync contract.

18.2. Add phone companion status in Settings.

18.3. Add quick capture endpoint or shared cloud path.

18.4. Add phone capture import into local inbox.

Benchmark gate:

- Phone companion path is visible in Settings.
- Quick capture can create a local inbox task.
- Offline or unavailable phone sync is shown plainly.
- Build and tests pass.

## Definition Of Done For Each Component

Each component is complete only when:

- Code is implemented.
- The feature is reachable from the running app when it is user-facing.
- The app shows visible status, state, or results for the feature.
- Unit tests pass.
- Relevant integration tests pass.
- User-facing text is simple and nontechnical.
- Failure state is handled.
- Local data is not lost if sync fails.
- The component is documented enough for future maintenance.
