using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypeToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SupportingDocuments",
                newName: "DocumentId");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Claims",
                newName: "AdditionalNotes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Claims",
                newName: "ClaimId");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "SupportingDocuments",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Claims",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "HoursWorked",
                table: "Claims",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDate",
                table: "Claims",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerId",
                table: "Claims",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "SupportingDocuments");

            migrationBuilder.DropColumn(
                name: "ReviewDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ReviewerId",
                table: "Claims");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "SupportingDocuments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AdditionalNotes",
                table: "Claims",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "ClaimId",
                table: "Claims",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Claims",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<double>(
                name: "HoursWorked",
                table: "Claims",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");
        }
    }
}
