using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyCLB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GooglePlaceId",
                table: "Branches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GooglePlaceId",
                table: "Branches",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
