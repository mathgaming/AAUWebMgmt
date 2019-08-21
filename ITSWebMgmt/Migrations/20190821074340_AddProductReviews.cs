using Microsoft.EntityFrameworkCore.Migrations;

namespace ITSWebMgmt.Migrations
{
    public partial class AddProductReviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogEntryArgument_LogEntries_LogEntryId",
                table: "LogEntryArgument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogEntryArgument",
                table: "LogEntryArgument");

            migrationBuilder.RenameTable(
                name: "LogEntryArgument",
                newName: "LogEntryArguments");

            migrationBuilder.RenameIndex(
                name: "IX_LogEntryArgument_LogEntryId",
                table: "LogEntryArguments",
                newName: "IX_LogEntryArguments_LogEntryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogEntryArguments",
                table: "LogEntryArguments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LogEntryArguments_LogEntries_LogEntryId",
                table: "LogEntryArguments",
                column: "LogEntryId",
                principalTable: "LogEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogEntryArguments_LogEntries_LogEntryId",
                table: "LogEntryArguments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogEntryArguments",
                table: "LogEntryArguments");

            migrationBuilder.RenameTable(
                name: "LogEntryArguments",
                newName: "LogEntryArgument");

            migrationBuilder.RenameIndex(
                name: "IX_LogEntryArguments_LogEntryId",
                table: "LogEntryArgument",
                newName: "IX_LogEntryArgument_LogEntryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogEntryArgument",
                table: "LogEntryArgument",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LogEntryArgument_LogEntries_LogEntryId",
                table: "LogEntryArgument",
                column: "LogEntryId",
                principalTable: "LogEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
