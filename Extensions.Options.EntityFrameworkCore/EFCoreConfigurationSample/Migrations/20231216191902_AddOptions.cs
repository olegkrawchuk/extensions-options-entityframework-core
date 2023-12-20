using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreConfigurationSample.Migrations
{
    /// <inheritdoc />
    public partial class AddOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CORE_PARAMETERS",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_PARAMETERS", x => x.Name);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CORE_PARAMETERS");
        }
    }
}
