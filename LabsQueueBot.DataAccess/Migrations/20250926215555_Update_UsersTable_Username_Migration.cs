using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabsQueueBot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Update_UsersTable_Username_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserRepository",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserRepository");
        }
    }
}
