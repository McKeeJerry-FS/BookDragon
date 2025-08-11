using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookDragon.Data.Migrations
{
    /// <inheritdoc />
    public partial class _006_AddBookTypeToBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookType",
                table: "Books",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookType",
                table: "Books");
        }
    }
}
