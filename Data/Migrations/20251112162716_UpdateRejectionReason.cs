using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRejectionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RejectionComment",
                table: "ProgramRejectionReasons",
                newName: "QueriedMessage");

            migrationBuilder.RenameColumn(
                name: "RejectedBy",
                table: "ProgramRejectionReasons",
                newName: "QueriedByFullName");

            migrationBuilder.AddColumn<string>(
                name: "QueriedBy",
                table: "ProgramRejectionReasons",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QueriedBy",
                table: "ProgramRejectionReasons");

            migrationBuilder.RenameColumn(
                name: "QueriedMessage",
                table: "ProgramRejectionReasons",
                newName: "RejectionComment");

            migrationBuilder.RenameColumn(
                name: "QueriedByFullName",
                table: "ProgramRejectionReasons",
                newName: "RejectedBy");
        }
    }
}
