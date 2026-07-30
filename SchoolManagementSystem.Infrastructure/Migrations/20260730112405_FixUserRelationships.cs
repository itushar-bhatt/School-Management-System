using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Users_UserId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Users_UserId1",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentParents_Parents_ParentId1",
                table: "StudentParents");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentParents_Students_StudentId1",
                table: "StudentParents");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_UserId1",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_UserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_UserId1",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_StudentParents_ParentId1",
                table: "StudentParents");

            migrationBuilder.DropIndex(
                name: "IX_StudentParents_StudentId1",
                table: "StudentParents");

            migrationBuilder.DropIndex(
                name: "IX_Parents_UserId",
                table: "Parents");

            migrationBuilder.DropIndex(
                name: "IX_Parents_UserId1",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentId1",
                table: "StudentParents");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "StudentParents");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Parents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentId1",
                table: "StudentParents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentId1",
                table: "StudentParents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Parents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId1",
                table: "Students",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentParents_ParentId1",
                table: "StudentParents",
                column: "ParentId1");

            migrationBuilder.CreateIndex(
                name: "IX_StudentParents_StudentId1",
                table: "StudentParents",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_UserId",
                table: "Parents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_UserId1",
                table: "Parents",
                column: "UserId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Users_UserId",
                table: "Parents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Users_UserId1",
                table: "Parents",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentParents_Parents_ParentId1",
                table: "StudentParents",
                column: "ParentId1",
                principalTable: "Parents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentParents_Students_StudentId1",
                table: "StudentParents",
                column: "StudentId1",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_UserId1",
                table: "Students",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
