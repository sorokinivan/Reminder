using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReminderWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemindTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Todothings",
                table: "Todothings");

            migrationBuilder.RenameTable(
                name: "Todothings",
                newName: "ToDoThings");

            migrationBuilder.AddColumn<long>(
                name: "RemindTime",
                table: "ToDoThings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ToDoThings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ToDoThings",
                table: "ToDoThings",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ToDoThings",
                table: "ToDoThings");

            migrationBuilder.DropColumn(
                name: "RemindTime",
                table: "ToDoThings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ToDoThings");

            migrationBuilder.RenameTable(
                name: "ToDoThings",
                newName: "Todothings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Todothings",
                table: "Todothings",
                column: "Id");
        }
    }
}
