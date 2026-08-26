using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserQualification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserQualifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QualificationTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProofOfQualificationURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQualifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserQualifications_Qualifications_QualificationTypeId",
                        column: x => x.QualificationTypeId,
                        principalTable: "Qualifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQualifications_QualificationTypeId",
                table: "UserQualifications",
                column: "QualificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQualifications_UserId_QualificationTypeId",
                table: "UserQualifications",
                columns: new[] { "UserId", "QualificationTypeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserQualifications");
        }
    }
}
