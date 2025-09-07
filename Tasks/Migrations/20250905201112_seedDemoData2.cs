using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CloudTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class seedDemoData2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("8252bf04-151b-40c6-92db-b8904781ca50"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("d4f62163-15eb-431a-9495-2a7bbd3cb998"));

            migrationBuilder.InsertData(
                table: "TaskItems",
                columns: new[] { "Id", "AssignedToUserId", "BoardId", "CreatedAt", "Description", "DueDate", "ParentTaskId", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), null, 1, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Initialize solution, setup GitHub repo", new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Pending", "Setup project", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, 1, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Add Identity + JWT auth", new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "InProgress", "Implement login", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.InsertData(
                table: "TaskItems",
                columns: new[] { "Id", "AssignedToUserId", "BoardId", "CreatedAt", "Description", "DueDate", "ParentTaskId", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("8252bf04-151b-40c6-92db-b8904781ca50"), null, 1, new DateTime(2025, 9, 5, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5947), "Add Identity + JWT auth", new DateTime(2025, 9, 10, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5943), null, "InProgress", "Implement login", new DateTime(2025, 9, 5, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5948) },
                    { new Guid("d4f62163-15eb-431a-9495-2a7bbd3cb998"), null, 1, new DateTime(2025, 9, 5, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5556), "Initialize solution, setup GitHub repo", new DateTime(2025, 9, 7, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5064), null, "Pending", "Setup project", new DateTime(2025, 9, 5, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5746) }
                });
        }
    }
}
