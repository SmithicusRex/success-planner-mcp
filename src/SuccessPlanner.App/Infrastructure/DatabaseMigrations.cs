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
            """)
    ];
}
