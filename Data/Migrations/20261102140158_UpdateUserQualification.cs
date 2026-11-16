using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trustesse.Ivoluntia.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserQualification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQualifications_Qualifications_QualificationTypeId",
                table: "UserQualifications");

            migrationBuilder.DropTable(
                name: "UserInterests");

            migrationBuilder.DropTable(
                name: "UserSkills");

            migrationBuilder.RenameColumn(
                name: "QualificationTypeId",
                table: "UserQualifications",
                newName: "QualificationId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQualifications_UserId_QualificationTypeId",
                table: "UserQualifications",
                newName: "IX_UserQualifications_UserId_QualificationId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQualifications_QualificationTypeId",
                table: "UserQualifications",
                newName: "IX_UserQualifications_QualificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQualifications_Qualifications_QualificationId",
                table: "UserQualifications",
                column: "QualificationId",
                principalTable: "Qualifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQualifications_Qualifications_QualificationId",
                table: "UserQualifications");

            migrationBuilder.RenameColumn(
                name: "QualificationId",
                table: "UserQualifications",
                newName: "QualificationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQualifications_UserId_QualificationId",
                table: "UserQualifications",
                newName: "IX_UserQualifications_UserId_QualificationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQualifications_QualificationId",
                table: "UserQualifications",
                newName: "IX_UserQualifications_QualificationTypeId");

            migrationBuilder.CreateTable(
                name: "UserInterests",
                columns: table => new
                {
                    InterestsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => new { x.InterestsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserInterests_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInterests_Interests_InterestsId",
                        column: x => x.InterestsId,
                        principalTable: "Interests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSkills",
                columns: table => new
                {
                    SkillsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkills", x => new { x.SkillsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserSkills_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UsersId",
                table: "UserInterests",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_UsersId",
                table: "UserSkills",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQualifications_Qualifications_QualificationTypeId",
                table: "UserQualifications",
                column: "QualificationTypeId",
                principalTable: "Qualifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
