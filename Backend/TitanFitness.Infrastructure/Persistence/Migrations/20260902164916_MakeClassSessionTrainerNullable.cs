using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TitanFitness.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeClassSessionTrainerNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_Trainers_TrainerId",
                table: "ClassSessions");

            migrationBuilder.AlterColumn<int>(
                name: "TrainerId",
                table: "ClassSessions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_Trainers_TrainerId",
                table: "ClassSessions",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "TrainerId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_Trainers_TrainerId",
                table: "ClassSessions");

            migrationBuilder.AlterColumn<int>(
                name: "TrainerId",
                table: "ClassSessions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_Trainers_TrainerId",
                table: "ClassSessions",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "TrainerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
