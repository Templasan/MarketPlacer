using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlacer.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEstoqueAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Estoque",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Estoque",
                table: "Products");
        }
    }
}
