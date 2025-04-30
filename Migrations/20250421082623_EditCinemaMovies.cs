using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieMart.Migrations
{
    /// <inheritdoc />
    public partial class EditCinemaMovies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "CinemaMovies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CinemaMovies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
