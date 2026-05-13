using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebReckrytingSystem.Migrations
{
    /// <inheritdoc />
    public partial class addpraktik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_practicum",
                table: "vacancies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_practicum",
                table: "vacancies");
        }
    }
}
