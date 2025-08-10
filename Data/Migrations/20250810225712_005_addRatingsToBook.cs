using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookDragon.Data.Migrations
{
    /// <inheritdoc />
    public partial class _005_addRatingsToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Books",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RatingReason",
                table: "Books",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "RatingReason",
                table: "Books");
        }
    }
}
