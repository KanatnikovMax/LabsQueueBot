using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabsQueueBot.Migrations
{
    /// <inheritdoc />
    public partial class AddIsNotificationNeededColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNotifyNeeded",
                table: "UserRepository",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotifyNeeded",
                table: "UserRepository");
        }
    }
}
