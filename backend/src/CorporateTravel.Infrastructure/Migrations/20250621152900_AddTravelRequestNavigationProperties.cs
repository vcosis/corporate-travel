using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorporateTravel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelRequestNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TravelRequests_ApproverId",
                table: "TravelRequests",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRequests_RequestingUserId",
                table: "TravelRequests",
                column: "RequestingUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelRequests_AspNetUsers_ApproverId",
                table: "TravelRequests",
                column: "ApproverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelRequests_AspNetUsers_RequestingUserId",
                table: "TravelRequests",
                column: "RequestingUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TravelRequests_AspNetUsers_ApproverId",
                table: "TravelRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TravelRequests_AspNetUsers_RequestingUserId",
                table: "TravelRequests");

            migrationBuilder.DropIndex(
                name: "IX_TravelRequests_ApproverId",
                table: "TravelRequests");

            migrationBuilder.DropIndex(
                name: "IX_TravelRequests_RequestingUserId",
                table: "TravelRequests");
        }
    }
}
