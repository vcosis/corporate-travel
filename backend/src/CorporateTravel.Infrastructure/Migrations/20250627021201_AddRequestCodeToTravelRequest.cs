using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorporateTravel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestCodeToTravelRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestCode",
                table: "TravelRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestCode",
                table: "TravelRequests");
        }
    }
}
