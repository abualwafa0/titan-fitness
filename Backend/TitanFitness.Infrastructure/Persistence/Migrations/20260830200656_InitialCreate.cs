using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TitanFitness.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OpeningTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ClosingTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DurationInMonths = table.Column<int>(type: "int", nullable: false),
                    MaximumFreezeDays = table.Column<int>(type: "int", nullable: false),
                    MaximumNumberOfFreezes = table.Column<int>(type: "int", nullable: false),
                    GuestPassQuota = table.Column<int>(type: "int", nullable: false),
                    AccessScope = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MembershipNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    JoinedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_Members_Branches_HomeBranchId",
                        column: x => x.HomeBranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Studios",
                columns: table => new
                {
                    StudioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudioName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studios", x => x.StudioId);
                    table.ForeignKey(
                        name: "FK_Studios_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trainers",
                columns: table => new
                {
                    TrainerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainerNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrainerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainers", x => x.TrainerId);
                    table.ForeignKey(
                        name: "FK_Trainers_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    CheckInId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CheckInDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
                    RefusalReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.CheckInId);
                    table.ForeignKey(
                        name: "FK_CheckIns_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    MembershipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AgreedPricePaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AgreedDurationInMonths = table.Column<int>(type: "int", nullable: false),
                    AgreedMaximumFreezeDays = table.Column<int>(type: "int", nullable: false),
                    AgreedMaximumNumberOfFreezes = table.Column<int>(type: "int", nullable: false),
                    AgreedGuestPassQuota = table.Column<int>(type: "int", nullable: false),
                    AgreedAccessScope = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.MembershipId);
                    table.ForeignKey(
                        name: "FK_Memberships_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Memberships_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassSessions",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    StudioId = table.Column<int>(type: "int", nullable: false),
                    TrainerId = table.Column<int>(type: "int", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    DurationInMinutes = table.Column<int>(type: "int", nullable: false),
                    CapacityLimit = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_ClassSessions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassSessions_Studios_StudioId",
                        column: x => x.StudioId,
                        principalTable: "Studios",
                        principalColumn: "StudioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassSessions_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "TrainerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Freezes",
                columns: table => new
                {
                    FreezeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MembershipId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationInMonths = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Freezes", x => x.FreezeId);
                    table.ForeignKey(
                        name: "FK_Freezes_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "MembershipId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuestPasses",
                columns: table => new
                {
                    GuestPassId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MembershipId = table.Column<int>(type: "int", nullable: false),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    UsedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestPasses", x => x.GuestPassId);
                    table.ForeignKey(
                        name: "FK_GuestPasses_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "MembershipId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    BookedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    WaitlistPosition = table.Column<int>(type: "int", nullable: true),
                    NotesForTrainer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_ClassSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ClassSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_MemberId",
                table: "Bookings",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SessionId_MemberId",
                table: "Bookings",
                columns: new[] { "SessionId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SessionId_Status_WaitlistPosition",
                table: "Bookings",
                columns: new[] { "SessionId", "Status", "WaitlistPosition" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_BranchId_CheckInDateTime",
                table: "CheckIns",
                columns: new[] { "BranchId", "CheckInDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_MemberId_CheckInDateTime",
                table: "CheckIns",
                columns: new[] { "MemberId", "CheckInDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_BranchId_SessionDate",
                table: "ClassSessions",
                columns: new[] { "BranchId", "SessionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_StudioId_SessionDate_StartTime",
                table: "ClassSessions",
                columns: new[] { "StudioId", "SessionDate", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_TrainerId_SessionDate_StartTime",
                table: "ClassSessions",
                columns: new[] { "TrainerId", "SessionDate", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Freezes_MembershipId_StartDate",
                table: "Freezes",
                columns: new[] { "MembershipId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestPasses_MembershipId_IssuedOn",
                table: "GuestPasses",
                columns: new[] { "MembershipId", "IssuedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Members_HomeBranchId",
                table: "Members",
                column: "HomeBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_MembershipNumber",
                table: "Members",
                column: "MembershipNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MemberId_StartDate_EndDate",
                table: "Memberships",
                columns: new[] { "MemberId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_PlanId",
                table: "Memberships",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Studios_BranchId_StudioName",
                table: "Studios",
                columns: new[] { "BranchId", "StudioName" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_BranchId_IsActive",
                table: "Trainers",
                columns: new[] { "BranchId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_TrainerNumber",
                table: "Trainers",
                column: "TrainerNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "Freezes");

            migrationBuilder.DropTable(
                name: "GuestPasses");

            migrationBuilder.DropTable(
                name: "ClassSessions");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "Studios");

            migrationBuilder.DropTable(
                name: "Trainers");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Branches");
        }
    }
}
