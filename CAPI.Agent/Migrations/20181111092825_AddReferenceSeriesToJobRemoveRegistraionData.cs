using Microsoft.EntityFrameworkCore.Migrations;

namespace CAPI.Agent.Migrations
{
    public partial class AddReferenceSeriesToJobRemoveRegistraionData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegistrationData",
                table: "Jobs",
                newName: "ReferenceSeries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReferenceSeries",
                table: "Jobs",
                newName: "RegistrationData");
        }
    }
}
