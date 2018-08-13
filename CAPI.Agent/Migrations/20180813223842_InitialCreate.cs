using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CAPI.Agent.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Accession = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    AdditionMethod = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    SourceAet = table.Column<string>(nullable: true),
                    PatientId = table.Column<string>(nullable: true),
                    PatientFullName = table.Column<string>(nullable: true),
                    PatientBirthDate = table.Column<string>(nullable: true),
                    CurrentAccession = table.Column<string>(nullable: true),
                    PriorAccession = table.Column<string>(nullable: true),
                    DefaultDestination = table.Column<string>(nullable: true),
                    ExtractBrain = table.Column<bool>(nullable: false),
                    ExtractBrainParams = table.Column<string>(nullable: true),
                    Register = table.Column<bool>(nullable: false),
                    BiasFieldCorrection = table.Column<bool>(nullable: false),
                    BiasFieldCorrectionParams = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
