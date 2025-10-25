using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class userskilltbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Country",
            //    table: "Locations");

            //migrationBuilder.DropColumn(
            //    name: "State",
            //    table: "Locations");

            //migrationBuilder.AddColumn<Guid>(
            //    name: "CountryId",
            //    table: "Locations",
            //    type: "char(36)",
            //    nullable: false,
            //    defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            //    collation: "ascii_general_ci");

            //migrationBuilder.AddColumn<Guid>(
            //    name: "StateId",
            //    table: "Locations",
            //    type: "char(36)",
            //    nullable: false,
            //    defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            //    collation: "ascii_general_ci");

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "DateOfBirth",
            //    table: "AspNetUsers",
            //    type: "datetime(6)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "OTP",
            //    table: "AspNetUsers",
            //    type: "varchar(6)",
            //    maxLength: 6,
            //    nullable: true)
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "OtpSubmittedTime",
            //    table: "AspNetUsers",
            //    type: "datetime(6)",
            //    nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "Countries",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
            //        CountryName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Countries", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "UserInterestLinks",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "varchar(255)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        InterestId = table.Column<string>(type: "varchar(255)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Id = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CreatedBy = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        DateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        DateUpdated = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeprecated = table.Column<bool>(type: "tinyint(1)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserInterestLinks", x => new { x.UserId, x.InterestId });
            //        table.ForeignKey(
            //            name: "FK_UserInterestLinks_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_UserInterestLinks_Interests_InterestId",
            //            column: x => x.InterestId,
            //            principalTable: "Interests",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "UserSkillLinks",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "varchar(255)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        SkillId = table.Column<string>(type: "varchar(255)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Id = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CreatedBy = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        DateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        DateUpdated = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeprecated = table.Column<bool>(type: "tinyint(1)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserSkillLinks", x => new { x.UserId, x.SkillId });
            //        table.ForeignKey(
            //            name: "FK_UserSkillLinks_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_UserSkillLinks_Skills_SkillId",
            //            column: x => x.SkillId,
            //            principalTable: "Skills",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "States",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
            //        StateName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CountryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_States", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_States_Countries_CountryId",
            //            column: x => x.CountryId,
            //            principalTable: "Countries",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Locations_CountryId",
            //    table: "Locations",
            //    column: "CountryId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Locations_StateId",
            //    table: "Locations",
            //    column: "StateId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_States_CountryId",
            //    table: "States",
            //    column: "CountryId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserInterestLinks_InterestId",
            //    table: "UserInterestLinks",
            //    column: "InterestId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserSkillLinks_SkillId",
            //    table: "UserSkillLinks",
            //    column: "SkillId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Locations_Countries_CountryId",
            //    table: "Locations",
            //    column: "CountryId",
            //    principalTable: "Countries",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Locations_States_StateId",
            //    table: "Locations",
            //    column: "StateId",
            //    principalTable: "States",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Countries_CountryId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_States_StateId",
                table: "Locations");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "UserInterestLinks");

            migrationBuilder.DropTable(
                name: "UserSkillLinks");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Locations_CountryId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_StateId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "StateId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OTP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpSubmittedTime",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Locations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Locations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
