using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFoundationCauses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoundationCauses_Causes_CausesId",
                table: "FoundationCauses");

            migrationBuilder.DropForeignKey(
                name: "FK_FoundationCauses_Foundations_FoundationsId",
                table: "FoundationCauses");

            migrationBuilder.RenameColumn(
                name: "FoundationsId",
                table: "FoundationCauses",
                newName: "FoundationId");

            migrationBuilder.RenameColumn(
                name: "CausesId",
                table: "FoundationCauses",
                newName: "CauseId");

            migrationBuilder.RenameIndex(
                name: "IX_FoundationCauses_FoundationsId",
                table: "FoundationCauses",
                newName: "IX_FoundationCauses_FoundationId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "FoundationCauses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "FoundationCauses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeprecated",
                table: "FoundationCauses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_FoundationCauses_Causes_CauseId",
                table: "FoundationCauses",
                column: "CauseId",
                principalTable: "Causes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FoundationCauses_Foundations_FoundationId",
                table: "FoundationCauses",
                column: "FoundationId",
                principalTable: "Foundations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoundationCauses_Causes_CauseId",
                table: "FoundationCauses");

            migrationBuilder.DropForeignKey(
                name: "FK_FoundationCauses_Foundations_FoundationId",
                table: "FoundationCauses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FoundationCauses");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "FoundationCauses");

            migrationBuilder.DropColumn(
                name: "IsDeprecated",
                table: "FoundationCauses");

            migrationBuilder.RenameColumn(
                name: "FoundationId",
                table: "FoundationCauses",
                newName: "FoundationsId");

            migrationBuilder.RenameColumn(
                name: "CauseId",
                table: "FoundationCauses",
                newName: "CausesId");

            migrationBuilder.RenameIndex(
                name: "IX_FoundationCauses_FoundationId",
                table: "FoundationCauses",
                newName: "IX_FoundationCauses_FoundationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoundationCauses_Causes_CausesId",
                table: "FoundationCauses",
                column: "CausesId",
                principalTable: "Causes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FoundationCauses_Foundations_FoundationsId",
                table: "FoundationCauses",
                column: "FoundationsId",
                principalTable: "Foundations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
