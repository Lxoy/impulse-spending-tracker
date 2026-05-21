using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace impulse_spending_tracker.Migrations
{
    /// <inheritdoc />
    public partial class MakeBooleanFlagsRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Merchants SET IsOnlineOnly = 0 WHERE IsOnlineOnly IS NULL;");
            migrationBuilder.Sql("UPDATE BudgetPlans SET IsActive = 0 WHERE IsActive IS NULL;");

            migrationBuilder.AlterColumn<bool>(
                name: "IsOnlineOnly",
                table: "Merchants",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BudgetPlans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsOnlineOnly",
                table: "Merchants",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BudgetPlans",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");
        }
    }
}