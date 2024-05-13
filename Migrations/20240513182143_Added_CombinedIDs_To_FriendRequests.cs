using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syncord.Migrations
{
    /// <inheritdoc />
    public partial class Added_CombinedIDs_To_FriendRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CombinedIds",
                table: "friendRequests",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CombinedIds",
                table: "friendRequests");
        }
    }
}
