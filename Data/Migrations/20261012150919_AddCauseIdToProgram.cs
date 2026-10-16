using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCauseIdToProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CauseId",
                table: "Programs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Programs_CauseId",
                table: "Programs",
                column: "CauseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Causes_CauseId",
                table: "Programs",
                column: "CauseId",
                principalTable: "Causes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Causes_CauseId",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_Programs_CauseId",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "CauseId",
                table: "Programs");
        }
    }
}
