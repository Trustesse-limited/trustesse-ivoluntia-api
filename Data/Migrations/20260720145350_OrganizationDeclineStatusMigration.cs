using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationDeclineStatusMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organizationDeclineStatuses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FoundationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizationDeclineStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organizationDeclineStatuses_Foundations_FoundationId",
                        column: x => x.FoundationId,
                        principalTable: "Foundations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_organizationDeclineStatuses_FoundationId",
                table: "organizationDeclineStatuses",
                column: "FoundationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organizationDeclineStatuses");
        }
    }
}
