using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autodesk.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Invoices_InvoiceId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices");

            migrationBuilder.RenameTable(
                name: "Invoices",
                newName: "ErrorLogs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ErrorLogs",
                table: "ErrorLogs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ErrorLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ExceptionType = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    StackTrace = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Context = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLog", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ErrorLogs_InvoiceId",
                table: "Products",
                column: "InvoiceId",
                principalTable: "ErrorLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ErrorLogs_InvoiceId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ErrorLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ErrorLogs",
                table: "ErrorLogs");

            migrationBuilder.RenameTable(
                name: "ErrorLogs",
                newName: "Invoices");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Invoices_InvoiceId",
                table: "Products",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
