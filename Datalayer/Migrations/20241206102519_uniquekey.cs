using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalayer.Migrations
{
    /// <inheritdoc />
    public partial class uniquekey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UniqueKey",
                table: "State",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "DestinationCityUniqueKey",
                table: "Requests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceCityUniqueKey",
                table: "Requests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UniqueKey",
                table: "Country",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "UniqueKey",
                table: "City",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_State_UniqueKey",
                table: "State",
                column: "UniqueKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Country_UniqueKey",
                table: "Country",
                column: "UniqueKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_City_UniqueKey",
                table: "City",
                column: "UniqueKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_State_UniqueKey",
                table: "State");

            migrationBuilder.DropIndex(
                name: "IX_Country_UniqueKey",
                table: "Country");

            migrationBuilder.DropIndex(
                name: "IX_City_UniqueKey",
                table: "City");

            migrationBuilder.DropColumn(
                name: "UniqueKey",
                table: "State");

            migrationBuilder.DropColumn(
                name: "DestinationCityUniqueKey",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "SourceCityUniqueKey",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "UniqueKey",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "UniqueKey",
                table: "City");
        }
    }
}
