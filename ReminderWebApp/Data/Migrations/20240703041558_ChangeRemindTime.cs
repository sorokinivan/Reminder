using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReminderWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRemindTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "RemindTime",
                table: "ToDoThings",
                type: "float",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "RemindTime",
                table: "ToDoThings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
