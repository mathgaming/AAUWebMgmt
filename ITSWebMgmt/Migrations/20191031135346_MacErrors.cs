using Microsoft.EntityFrameworkCore.Migrations;

namespace ITSWebMgmt.Migrations
{
    public partial class MacErrors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MacErrors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Heading = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Severeness = table.Column<int>(nullable: false),
                    GroupName = table.Column<string>(nullable: true),
                    CaseLink = table.Column<string>(nullable: true),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MacErrors", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MacErrors");
        }
    }
}
