using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingGoalsWithContributionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GoalId",
                table: "Movements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SavingGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TargetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingGoals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movements_GoalId",
                table: "Movements",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingGoals_UserId",
                table: "SavingGoals",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_SavingGoals_GoalId",
                table: "Movements",
                column: "GoalId",
                principalTable: "SavingGoals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movements_SavingGoals_GoalId",
                table: "Movements");

            migrationBuilder.DropTable(
                name: "SavingGoals");

            migrationBuilder.DropIndex(
                name: "IX_Movements_GoalId",
                table: "Movements");

            migrationBuilder.DropColumn(
                name: "GoalId",
                table: "Movements");
        }
    }
}
