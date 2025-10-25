using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class IncludeRelationshipFoundationProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FoundationId",
                table: "Programs",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "HasDonation",
                table: "Programs",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Programs",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Programs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Programs_FoundationId",
                table: "Programs",
                column: "FoundationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Foundations_FoundationId",
                table: "Programs",
                column: "FoundationId",
                principalTable: "Foundations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Foundations_FoundationId",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_Programs_FoundationId",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "FoundationId",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "HasDonation",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Programs");
        }
    }
}
