namespace SuccessPlanner.App.Infrastructure;

public static class DatabaseMigrations
{
    public static IReadOnlyList<IDatabaseMigration> All { get; } =
    [
        new SqlDatabaseMigration(
            version: 1,
            name: "Create local store metadata",
            """
            CREATE TABLE IF NOT EXISTS local_store_metadata (
                key TEXT NOT NULL PRIMARY KEY,
                value TEXT NOT NULL
            );
            """,
            """
            INSERT OR IGNORE INTO local_store_metadata (key, value)
            VALUES ('store_kind', 'Success Planner MCP SQLite local store');
            """),
        new SqlDatabaseMigration(
            version: 2,
            name: "Create core application tables",
            """
            CREATE TABLE IF NOT EXISTS projects (
                id TEXT NOT NULL PRIMARY KEY,
                name TEXT NOT NULL,
                notes TEXT NOT NULL DEFAULT '',
                status TEXT NOT NULL,
                priority TEXT NOT NULL,
                start_date TEXT NULL,
                due_date TEXT NULL,
                created_at TEXT NOT NULL,
                completed_at TEXT NULL,
                immediate_need TEXT NOT NULL DEFAULT '',
                minimum_win TEXT NOT NULL DEFAULT '',
                task_ids_json TEXT NOT NULL DEFAULT '[]',
                milestone_ids_json TEXT NOT NULL DEFAULT '[]',
                tags_json TEXT NOT NULL DEFAULT '[]'
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS tasks (
                id TEXT NOT NULL PRIMARY KEY,
                title TEXT NOT NULL,
                notes TEXT NOT NULL DEFAULT '',
                status TEXT NOT NULL,
                priority TEXT NOT NULL,
                due_date TEXT NULL,
                start_date TEXT NULL,
                created_at TEXT NOT NULL,
                completed_at TEXT NULL,
                project_id TEXT NULL,
                estimated_minutes INTEGER NULL,
                energy_level TEXT NOT NULL DEFAULT 'Normal',
                is_tiny_step INTEGER NOT NULL DEFAULT 0 CHECK (is_tiny_step IN (0, 1)),
                is_physical_activity INTEGER NOT NULL DEFAULT 0 CHECK (is_physical_activity IN (0, 1)),
                tags_json TEXT NOT NULL DEFAULT '[]',
                FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE SET NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS milestones (
                id TEXT NOT NULL PRIMARY KEY,
                project_id TEXT NOT NULL,
                name TEXT NOT NULL,
                notes TEXT NOT NULL DEFAULT '',
                status TEXT NOT NULL,
                target_date TEXT NULL,
                created_at TEXT NOT NULL,
                completed_at TEXT NULL,
                minimum_win TEXT NOT NULL DEFAULT '',
                is_review_marker INTEGER NOT NULL DEFAULT 0 CHECK (is_review_marker IN (0, 1)),
                task_ids_json TEXT NOT NULL DEFAULT '[]',
                tags_json TEXT NOT NULL DEFAULT '[]',
                FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS notes (
                id TEXT NOT NULL PRIMARY KEY,
                owner_type TEXT NOT NULL,
                owner_id TEXT NULL,
                text TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL,
                is_pinned INTEGER NOT NULL DEFAULT 0 CHECK (is_pinned IN (0, 1)),
                is_review_highlight INTEGER NOT NULL DEFAULT 0 CHECK (is_review_highlight IN (0, 1)),
                tags_json TEXT NOT NULL DEFAULT '[]'
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS focus_sessions (
                id TEXT NOT NULL PRIMARY KEY,
                task_id TEXT NULL,
                intention TEXT NOT NULL,
                planned_minutes INTEGER NOT NULL,
                status TEXT NOT NULL,
                started_at TEXT NOT NULL,
                paused_at TEXT NULL,
                completed_at TEXT NULL,
                ended_at TEXT NULL,
                total_paused_minutes INTEGER NOT NULL DEFAULT 0,
                actual_focus_minutes INTEGER NULL,
                win_note TEXT NOT NULL DEFAULT '',
                blocked_reason TEXT NOT NULL DEFAULT '',
                tags_json TEXT NOT NULL DEFAULT '[]',
                FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE SET NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS success_goals (
                id TEXT NOT NULL PRIMARY KEY,
                title TEXT NOT NULL,
                why_it_matters TEXT NOT NULL DEFAULT '',
                minimum_win TEXT NOT NULL,
                stretch_goal TEXT NOT NULL DEFAULT '',
                status TEXT NOT NULL,
                priority TEXT NOT NULL,
                project_id TEXT NULL,
                start_date TEXT NULL,
                target_date TEXT NULL,
                created_at TEXT NOT NULL,
                completed_at TEXT NULL,
                completion_note TEXT NOT NULL DEFAULT '',
                task_ids_json TEXT NOT NULL DEFAULT '[]',
                tags_json TEXT NOT NULL DEFAULT '[]',
                FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE SET NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS movement_sessions (
                id TEXT NOT NULL PRIMARY KEY,
                activity_type TEXT NOT NULL,
                activity_name TEXT NOT NULL,
                status TEXT NOT NULL,
                task_id TEXT NULL,
                planned_minutes INTEGER NOT NULL,
                actual_minutes INTEGER NULL,
                created_at TEXT NOT NULL,
                scheduled_for TEXT NULL,
                started_at TEXT NULL,
                completed_at TEXT NULL,
                ended_at TEXT NULL,
                mind_occupier TEXT NOT NULL DEFAULT '',
                is_with_spouse INTEGER NOT NULL DEFAULT 0 CHECK (is_with_spouse IN (0, 1)),
                notes TEXT NOT NULL DEFAULT '',
                win_note TEXT NOT NULL DEFAULT '',
                tags_json TEXT NOT NULL DEFAULT '[]',
                FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE SET NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS source_links (
                id TEXT NOT NULL PRIMARY KEY,
                local_item_type TEXT NOT NULL,
                local_item_id TEXT NOT NULL,
                source_system TEXT NOT NULL,
                external_id TEXT NOT NULL,
                external_container_id TEXT NOT NULL DEFAULT '',
                external_display_name TEXT NOT NULL DEFAULT '',
                external_web_url TEXT NOT NULL DEFAULT '',
                source_version TEXT NOT NULL DEFAULT '',
                sync_state TEXT NOT NULL,
                created_at TEXT NOT NULL,
                last_attempted_at TEXT NULL,
                last_synced_at TEXT NULL,
                last_failed_at TEXT NULL,
                retry_count INTEGER NOT NULL DEFAULT 0,
                failure_message TEXT NOT NULL DEFAULT '',
                is_read_only INTEGER NOT NULL DEFAULT 0 CHECK (is_read_only IN (0, 1))
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS settings_metadata (
                key TEXT NOT NULL PRIMARY KEY,
                value TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );
            """,
            "CREATE INDEX IF NOT EXISTS idx_tasks_project_id ON tasks(project_id);",
            "CREATE INDEX IF NOT EXISTS idx_tasks_status_due_date ON tasks(status, due_date);",
            "CREATE INDEX IF NOT EXISTS idx_milestones_project_id ON milestones(project_id);",
            "CREATE INDEX IF NOT EXISTS idx_notes_owner ON notes(owner_type, owner_id);",
            "CREATE INDEX IF NOT EXISTS idx_focus_sessions_task_id ON focus_sessions(task_id);",
            "CREATE INDEX IF NOT EXISTS idx_success_goals_project_id ON success_goals(project_id);",
            "CREATE INDEX IF NOT EXISTS idx_movement_sessions_task_id ON movement_sessions(task_id);",
            "CREATE INDEX IF NOT EXISTS idx_source_links_local_item ON source_links(local_item_type, local_item_id);",
            "CREATE INDEX IF NOT EXISTS idx_source_links_source_item ON source_links(source_system, external_id);"),
        new SqlDatabaseMigration(
            version: 3,
            name: "Create sync queue table",
            """
            CREATE TABLE IF NOT EXISTS sync_queue (
                id TEXT NOT NULL PRIMARY KEY,
                local_item_type TEXT NOT NULL,
                local_item_id TEXT NOT NULL,
                source_system TEXT NOT NULL,
                source_link_id TEXT NULL,
                action_type TEXT NOT NULL,
                payload_json TEXT NOT NULL DEFAULT '{}',
                sync_state TEXT NOT NULL,
                retry_count INTEGER NOT NULL DEFAULT 0,
                next_attempt_at TEXT NULL,
                last_attempted_at TEXT NULL,
                failure_message TEXT NOT NULL DEFAULT '',
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL,
                FOREIGN KEY (source_link_id) REFERENCES source_links(id) ON DELETE SET NULL
            );
            """,
            "CREATE INDEX IF NOT EXISTS idx_sync_queue_state_next_attempt ON sync_queue(sync_state, next_attempt_at);",
            "CREATE INDEX IF NOT EXISTS idx_sync_queue_local_item ON sync_queue(local_item_type, local_item_id);",
            "CREATE INDEX IF NOT EXISTS idx_sync_queue_source_link ON sync_queue(source_link_id);")
    ];
}
