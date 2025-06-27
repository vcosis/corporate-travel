using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorporateTravel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingUsersCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing users that have the default CreatedAt value (0001-01-01)
            migrationBuilder.Sql(@"
                UPDATE [AspNetUsers] 
                SET [CreatedAt] = GETUTCDATE() 
                WHERE [CreatedAt] = '0001-01-01T00:00:00.0000000'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to default value if needed
            migrationBuilder.Sql(@"
                UPDATE [AspNetUsers] 
                SET [CreatedAt] = '0001-01-01T00:00:00.0000000' 
                WHERE [CreatedAt] = GETUTCDATE()
            ");
        }
    }
}
