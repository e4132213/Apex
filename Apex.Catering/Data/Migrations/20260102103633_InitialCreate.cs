using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Apex.Catering.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    FoodItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.FoodItemId);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MenuName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "FoodBookings",
                columns: table => new
                {
                    FoodBookingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientReferenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "INTEGER", nullable: false),
                    MenuId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodBookings", x => x.FoodBookingId);
                    table.ForeignKey(
                        name: "FK_FoodBookings_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuFoodItems",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "INTEGER", nullable: false),
                    FoodItemId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuFoodItems", x => new { x.MenuId, x.FoodItemId });
                    table.ForeignKey(
                        name: "FK_MenuFoodItems_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItems",
                        principalColumn: "FoodItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuFoodItems_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "FoodItems",
                columns: new[] { "FoodItemId", "Description", "UnitPrice" },
                values: new object[,]
                {
                    { 1, "Chicken Sandwich", 5.99m },
                    { 2, "Veggie Wrap", 4.99m },
                    { 3, "Caesar Salad", 3.99m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodBookings_MenuId",
                table: "FoodBookings",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuFoodItems_FoodItemId",
                table: "MenuFoodItems",
                column: "FoodItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodBookings");

            migrationBuilder.DropTable(
                name: "MenuFoodItems");

            migrationBuilder.DropTable(
                name: "FoodItems");

            migrationBuilder.DropTable(
                name: "Menus");
        }
    }
}
