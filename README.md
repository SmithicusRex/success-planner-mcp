# Success Planner MCP

Personal Success Planner.

Success Planner MCP is a solo, local planning and focus app for personal success development.

The goal is to provide a simple point-and-click control center that hides the complexity of Microsoft To Do, Planner, Project, local storage, and automation behind a calm visual interface.

## Product Intent

Success Planner MCP is designed for a fast creative mind with many active ideas and projects. It should help the user:

- Capture ideas quickly
- See what matters today
- Break work into small actions
- Start focused 20-minute sessions
- Take useful breaks
- Rotate between creative projects
- Schedule movement and physical activity
- Review progress through small wins

## Current Design

The first design pass is in [SuccessPlannerMCP_DESIGN.md](SuccessPlannerMCP_DESIGN.md).

## Proposed Stack

- Desktop app: C# / .NET / WPF
- Phone companion: .NET MAUI
- Local storage: SQLite
- Microsoft sync: Microsoft Graph where available
- Microsoft Project desktop: COM/VBA automation

## First Milestone

Version 0.1 should prove the local control experience before deep Microsoft integration:

- Home screen
- Capture screen
- Today screen
- Start Work screen with timer
- Move screen
- Local SQLite storage
- Manual task creation
- Done and snooze actions
- Simple weekly review
