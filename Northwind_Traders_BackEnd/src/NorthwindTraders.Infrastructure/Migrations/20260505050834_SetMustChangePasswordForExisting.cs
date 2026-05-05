using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorthwindTraders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetMustChangePasswordForExisting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All existing employees were seeded with a default password.
            // Force them to change it on their next login.
            migrationBuilder.Sql("UPDATE [Employees] SET [MustChangePassword] = 1 WHERE [PasswordHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Employees] SET [MustChangePassword] = 0");
        }
    }
}
