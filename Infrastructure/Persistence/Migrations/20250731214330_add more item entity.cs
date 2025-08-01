using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addmoreitementity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SizeType",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ComboPrice",
                table: "ItemCombos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ItemCombos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalPrice",
                table: "ItemAddOns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ItemAddOns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Combos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "AddOns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ItemSizePrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSizePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemSizePrices_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemSizePrices_ItemId",
                table: "ItemSizePrices",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemSizePrices");

            migrationBuilder.DropColumn(
                name: "SizeType",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ComboPrice",
                table: "ItemCombos");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ItemCombos");

            migrationBuilder.DropColumn(
                name: "AdditionalPrice",
                table: "ItemAddOns");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ItemAddOns");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Combos");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "AddOns");
        }
    }
}
