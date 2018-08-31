using Microsoft.EntityFrameworkCore.Migrations;

namespace CAPI.Agent.Migrations
{
    public partial class CaseCommentPropAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Cases",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Cases");
        }
    }
}
