using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorthwindTraders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
// EF Core Method — CreateTable
            migrationBuilder.CreateTable(
                name: "ShipmentStates",
                columns: table => new
                {
                    ShipmentStateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentStates", x => x.ShipmentStateId);
                });

            // EF Core Method — InsertData (seed rows)
            migrationBuilder.InsertData(
                table: "ShipmentStates",
                columns: new[] { "ShipmentStateId", "Name", "Description" },
                values: new object[,]
                {
                    { 1, "Pending",    "Order received, not yet processed" },
                    { 2, "Processing", "Order is being prepared" },
                    { 3, "Shipped",    "Order has been shipped" },
                    { 4, "Invoiced",   "Invoice has been generated" },
                    { 5, "Completed",  "Order delivered and completed" },
                    { 6, "Cancelled",  "Order has been cancelled" }
                });

            // EF Core Method — AddColumn
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Employees",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Employees",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShipmentStateId",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalShipAddress",
                table: "Orders",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidatedShipAddress",
                table: "Orders",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShipLatitude",
                table: "Orders",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShipLongitude",
                table: "Orders",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillAddress",
                table: "Orders",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillCity",
                table: "Orders",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillRegion",
                table: "Orders",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillPostalCode",
                table: "Orders",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillCountry",
                table: "Orders",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalBillAddress",
                table: "Orders",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidatedBillAddress",
                table: "Orders",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BillLatitude",
                table: "Orders",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BillLongitude",
                table: "Orders",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Orders",
                maxLength: 1000,
                nullable: true);

            // EF Core Method — AddForeignKey
            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ShipmentStates",
                table: "Orders",
                column: "ShipmentStateId",
                principalTable: "ShipmentStates",
                principalColumn: "ShipmentStateId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
migrationBuilder.DropForeignKey(name: "FK_Orders_ShipmentStates", table: "Orders");
            migrationBuilder.DropTable(name: "ShipmentStates");
            migrationBuilder.DropColumn(name: "Email", table: "Employees");
            migrationBuilder.DropColumn(name: "PasswordHash", table: "Employees");
            migrationBuilder.DropColumn(name: "ShipmentStateId", table: "Orders");
            migrationBuilder.DropColumn(name: "OriginalShipAddress", table: "Orders");
            migrationBuilder.DropColumn(name: "ValidatedShipAddress", table: "Orders");
            migrationBuilder.DropColumn(name: "ShipLatitude", table: "Orders");
            migrationBuilder.DropColumn(name: "ShipLongitude", table: "Orders");
            migrationBuilder.DropColumn(name: "BillAddress", table: "Orders");
            migrationBuilder.DropColumn(name: "BillCity", table: "Orders");
            migrationBuilder.DropColumn(name: "BillRegion", table: "Orders");
            migrationBuilder.DropColumn(name: "BillPostalCode", table: "Orders");
            migrationBuilder.DropColumn(name: "BillCountry", table: "Orders");
            migrationBuilder.DropColumn(name: "OriginalBillAddress", table: "Orders");
            migrationBuilder.DropColumn(name: "ValidatedBillAddress", table: "Orders");
            migrationBuilder.DropColumn(name: "BillLatitude", table: "Orders");
            migrationBuilder.DropColumn(name: "BillLongitude", table: "Orders");
            migrationBuilder.DropColumn(name: "Notes", table: "Orders");
        }
    }
}
