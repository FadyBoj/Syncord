using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syncord.Migrations
{
    /// <inheritdoc />
    public partial class Added_combined_unique_constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CombinedIds",
                table: "friendRequests",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.CreateIndex(
                name: "IX_friendRequests_CombinedIds",
                table: "friendRequests",
                column: "CombinedIds",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_friendRequests_CombinedIds",
                table: "friendRequests");

            migrationBuilder.AlterColumn<string>(
                name: "CombinedIds",
                table: "friendRequests",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");
        }
    }
}
