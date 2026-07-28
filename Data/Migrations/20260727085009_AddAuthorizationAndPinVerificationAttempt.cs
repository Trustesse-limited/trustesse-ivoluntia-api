using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorizationAndPinVerificationAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authorizations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TokenHash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: true),
                    TokenSalt = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    InitiatorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ByteString = table.Column<byte[]>(type: "varbinary(16)", maxLength: 16, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Authorizations_AspNetUsers_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PinVerificationAttempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastAttemptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinVerificationAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authorizations_ByteString",
                table: "Authorizations",
                column: "ByteString",
                unique: true,
                filter: "[ByteString] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Authorizations_InitiatorId",
                table: "Authorizations",
                column: "InitiatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Authorizations");

            migrationBuilder.DropTable(
                name: "PinVerificationAttempts");
        }
    }
}
