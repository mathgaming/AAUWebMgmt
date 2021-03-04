using Microsoft.EntityFrameworkCore.Migrations;

namespace ITSWebMgmt.Migrations
{
    public partial class MacCSV : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MacCSVInfos",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComputerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OESSAssetNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AAUNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MacCSVInfos", x => x.SerialNumber);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MacCSVInfos");
        }
    }
}
