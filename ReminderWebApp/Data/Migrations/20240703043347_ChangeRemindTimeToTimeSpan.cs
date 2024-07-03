using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReminderWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRemindTimeToTimeSpan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "RemindTime",
                table: "ToDoThings",
                type: "time",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "RemindTime",
                table: "ToDoThings",
                type: "float",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");
        }
    }
}
