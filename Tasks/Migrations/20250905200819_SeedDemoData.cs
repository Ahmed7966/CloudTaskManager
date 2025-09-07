using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CloudTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentTaskId",
                table: "TaskItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Boards",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 1, "This is a demo board for testing.", "Demo Project" });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "BoardId", "Role", "UserId" },
                values: new object[,]
                {
                    { 1, 1, "BoardOwner", "demo-user-1" },
                    { 2, 1, "User", "demo-user-2" }
                });

            migrationBuilder.InsertData(
                table: "TaskItems",
                columns: new[] { "Id", "AssignedToUserId", "BoardId", "CreatedAt", "Description", "DueDate", "ParentTaskId", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("8252bf04-151b-40c6-92db-b8904781ca50"), null, 1, new DateTime(2025, 9, 5, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5947), "Add Identity + JWT auth", new DateTime(2025, 9, 10, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5943), null, "InProgress", "Implement login", new DateTime(2025, 9, 5, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5948) },
                    { new Guid("d4f62163-15eb-431a-9495-2a7bbd3cb998"), null, 1, new DateTime(2025, 9, 5, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5556), "Initialize solution, setup GitHub repo", new DateTime(2025, 9, 7, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5064), null, "Pending", "Setup project", new DateTime(2025, 9, 5, 20, 8, 18, 696, DateTimeKind.Utc).AddTicks(5746) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_ParentTaskId",
                table: "TaskItems",
                column: "ParentTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_TaskItems_ParentTaskId",
                table: "TaskItems",
                column: "ParentTaskId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_TaskItems_ParentTaskId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_ParentTaskId",
                table: "TaskItems");

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("8252bf04-151b-40c6-92db-b8904781ca50"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("d4f62163-15eb-431a-9495-2a7bbd3cb998"));

            migrationBuilder.DeleteData(
                table: "Boards",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "ParentTaskId",
                table: "TaskItems");
        }
    }
}
