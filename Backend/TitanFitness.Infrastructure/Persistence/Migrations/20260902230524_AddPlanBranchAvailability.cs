using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TitanFitness.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanBranchAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanBranches",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanBranches", x => new { x.PlanId, x.BranchId });
                    table.ForeignKey(
                        name: "FK_PlanBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanBranches_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanBranches_BranchId",
                table: "PlanBranches",
                column: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanBranches");
        }
    }
}
