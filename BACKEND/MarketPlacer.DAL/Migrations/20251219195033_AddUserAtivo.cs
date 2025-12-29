using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlacer.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAtivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Users");
        }
    }
}
