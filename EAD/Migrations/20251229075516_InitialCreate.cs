using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EAD.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MealItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tmp_ms_x__3214EC077CB09D50", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tmp_ms_x__3214EC07B1AEAFBA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeek = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MealType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MealItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DailyMen__3214EC0785AD9854", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyMenu_MealItem",
                        column: x => x.MealItemId,
                        principalTable: "MealItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    GeneratedOn = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    PaidOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bills_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillRecheckRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RequestMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedOn = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BillRech__3214EC0729003E27", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecheckRequest_Bill",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecheckRequest_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DailyConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MealItemId = table.Column<int>(type: "int", nullable: false),
                    ConsumptionDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],getdate()))"),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsBilled = table.Column<bool>(type: "bit", nullable: false),
                    BillId = table.Column<int>(type: "int", nullable: true),
                    WasUserPresent = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DailyCon__3214EC074D020E3D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyConsumption_MealItem",
                        column: x => x.MealItemId,
                        principalTable: "MealItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DailyConsumption_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyConsumptions_Bill",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillRecheckRequests_BillId",
                table: "BillRecheckRequests",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillRecheckRequests_UserId",
                table: "BillRecheckRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_User_GeneratedOn",
                table: "Bills",
                columns: new[] { "UserId", "GeneratedOn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyConsumptions_BillId",
                table: "DailyConsumptions",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyConsumptions_MealItemId",
                table: "DailyConsumptions",
                column: "MealItemId");

            migrationBuilder.CreateIndex(
                name: "UQ_DailyConsumption",
                table: "DailyConsumptions",
                columns: new[] { "UserId", "MealItemId", "ConsumptionDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenu_MealItemId",
                table: "DailyMenu",
                column: "MealItemId");

            migrationBuilder.CreateIndex(
                name: "UQ__tmp_ms_x__A9D10534D3A90284",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillRecheckRequests");

            migrationBuilder.DropTable(
                name: "DailyConsumptions");

            migrationBuilder.DropTable(
                name: "DailyMenu");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "MealItems");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
