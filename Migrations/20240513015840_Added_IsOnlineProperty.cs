using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syncord.Migrations
{
    /// <inheritdoc />
    public partial class Added_IsOnlineProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isOnline",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isOnline",
                table: "AspNetUsers");
        }
    }
}
