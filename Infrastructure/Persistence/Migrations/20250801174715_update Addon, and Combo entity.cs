using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateAddonandComboentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "Combos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "AddOns",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Combos_RestaurantId",
                table: "Combos",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_AddOns_RestaurantId",
                table: "AddOns",
                column: "RestaurantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AddOns_Restaurants_RestaurantId",
                table: "AddOns",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Combos_Restaurants_RestaurantId",
                table: "Combos",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddOns_Restaurants_RestaurantId",
                table: "AddOns");

            migrationBuilder.DropForeignKey(
                name: "FK_Combos_Restaurants_RestaurantId",
                table: "Combos");

            migrationBuilder.DropIndex(
                name: "IX_Combos_RestaurantId",
                table: "Combos");

            migrationBuilder.DropIndex(
                name: "IX_AddOns_RestaurantId",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Combos");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "AddOns");
        }
    }
}
