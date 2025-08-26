using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnhnaceGetAllrestaurantendpointperformancebyaddingindexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsOpen",
                table: "Restaurants",
                column: "IsOpen");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsOpen_Rating",
                table: "Restaurants",
                columns: new[] { "IsOpen", "Rating" });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Name",
                table: "Restaurants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Rating",
                table: "Restaurants",
                column: "Rating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsOpen",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsOpen_Rating",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Name",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Rating",
                table: "Restaurants");
        }
    }
}
