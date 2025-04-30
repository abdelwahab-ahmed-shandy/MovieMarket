using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieMart.Migrations
{
    /// <inheritdoc />
    public partial class EditInEpisodeAndSeasonTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Episodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Episodes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Episodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "Rating", "ThumbnailUrl" },
                values: new object[] { "", 5.5m, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Episodes");
        }
    }
}
