using Microsoft.EntityFrameworkCore.Migrations;

namespace ITSWebMgmt.Migrations
{
    public partial class OESSUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestedFor",
                table: "TrashRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedForOESSStaffID",
                table: "TrashRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedForOSSSName",
                table: "TrashRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedFor",
                table: "TrashRequests");

            migrationBuilder.DropColumn(
                name: "RequestedForOESSStaffID",
                table: "TrashRequests");

            migrationBuilder.DropColumn(
                name: "RequestedForOSSSName",
                table: "TrashRequests");
        }
    }
}
