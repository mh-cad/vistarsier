using Microsoft.EntityFrameworkCore.Migrations;

namespace CAPI.Agent.Migrations
{
    public partial class AddRegistrationDataToJobClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationData",
                table: "Jobs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationData",
                table: "Jobs");
        }
    }
}
