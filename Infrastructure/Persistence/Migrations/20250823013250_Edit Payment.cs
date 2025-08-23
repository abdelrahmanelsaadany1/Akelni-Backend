using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EditPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop the existing foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_Id",
                table: "Payments");

            // Step 2: Create a temporary table with correct structure
            migrationBuilder.Sql(@"
                CREATE TABLE PaymentsTemp (
                    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    StripePaymentIntentId nvarchar(max) NOT NULL,
                    Amount decimal(18,2) NOT NULL,
                    PaidAt datetime2 NOT NULL,
                    OrderId int NOT NULL
                );
            ");

            // Step 3: Copy data from old table to temp table
            migrationBuilder.Sql(@"
                INSERT INTO PaymentsTemp (StripePaymentIntentId, Amount, PaidAt, OrderId)
                SELECT StripePaymentIntentId, Amount, PaidAt, Id
                FROM Payments;
            ");

            // Step 4: Drop the old table
            migrationBuilder.DropTable("Payments");

            // Step 5: Rename temp table to Payments
            migrationBuilder.Sql("EXEC sp_rename 'PaymentsTemp', 'Payments';");

            // Step 6: Create the proper foreign key constraint
            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the changes
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            // Create temp table with old structure
            migrationBuilder.Sql(@"
                CREATE TABLE PaymentsTemp (
                    Id int NOT NULL PRIMARY KEY,
                    StripePaymentIntentId nvarchar(max) NOT NULL,
                    Amount decimal(18,2) NOT NULL,
                    PaidAt datetime2 NOT NULL,
                    OrderId int NOT NULL
                );
            ");

            // Copy data back
            migrationBuilder.Sql(@"
                INSERT INTO PaymentsTemp (Id, StripePaymentIntentId, Amount, PaidAt, OrderId)
                SELECT OrderId, StripePaymentIntentId, Amount, PaidAt, OrderId
                FROM Payments;
            ");

            // Drop and recreate
            migrationBuilder.DropTable("Payments");
            migrationBuilder.Sql("EXEC sp_rename 'PaymentsTemp', 'Payments';");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_Id",
                table: "Payments",
                column: "Id",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}