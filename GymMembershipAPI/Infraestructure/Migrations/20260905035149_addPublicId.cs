using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMembershipAPI.Migrations
{
    /// <inheritdoc />
    public partial class addPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublicId",
                table: "RegisterAccesses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublicId",
                table: "MembershipTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublicId",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublicId",
                table: "GroupClasses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublicId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipTypes_PublicId",
                table: "MembershipTypes",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_PublicId",
                table: "Members",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PublicId",
                table: "Bookings",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MembershipTypes_PublicId",
                table: "MembershipTypes");

            migrationBuilder.DropIndex(
                name: "IX_Members_PublicId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PublicId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "RegisterAccesses");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "MembershipTypes");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "GroupClasses");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Bookings");
        }
    }
}
