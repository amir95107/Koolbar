using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalayer.Migrations
{
    /// <inheritdoc />
    public partial class LocationBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnglishTitle",
                table: "State",
                newName: "PersianTitle");

            migrationBuilder.RenameColumn(
                name: "EnglishTitle",
                table: "Country",
                newName: "PersianTitle");

            migrationBuilder.RenameColumn(
                name: "EnglishTitle",
                table: "City",
                newName: "PersianTitle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersianTitle",
                table: "State",
                newName: "EnglishTitle");

            migrationBuilder.RenameColumn(
                name: "PersianTitle",
                table: "Country",
                newName: "EnglishTitle");

            migrationBuilder.RenameColumn(
                name: "PersianTitle",
                table: "City",
                newName: "EnglishTitle");
        }
    }
}
