using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addindexforperformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_RestaurantId",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_RestaurantId_Id",
                table: "Items",
                columns: new[] { "RestaurantId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_RestaurantId_Id",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_RestaurantId",
                table: "Items",
                column: "RestaurantId");
        }
    }
}
