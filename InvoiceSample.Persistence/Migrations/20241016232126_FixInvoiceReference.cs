using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceSample.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixInvoiceReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Invoices_InvoiceId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_InvoiceId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "SalesOrders");

            migrationBuilder.CreateTable(
                name: "InvoiceSalesOrder",
                columns: table => new
                {
                    InvoicesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesOrdersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSalesOrder", x => new { x.InvoicesId, x.SalesOrdersId });
                    table.ForeignKey(
                        name: "FK_InvoiceSalesOrder_Invoices_InvoicesId",
                        column: x => x.InvoicesId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceSalesOrder_SalesOrders_SalesOrdersId",
                        column: x => x.SalesOrdersId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSalesOrder_SalesOrdersId",
                table: "InvoiceSalesOrder",
                column: "SalesOrdersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceSalesOrder");

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                table: "SalesOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_InvoiceId",
                table: "SalesOrders",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Invoices_InvoiceId",
                table: "SalesOrders",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }
    }
}
