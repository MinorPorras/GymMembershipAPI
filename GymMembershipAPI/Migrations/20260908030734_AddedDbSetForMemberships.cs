using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMembershipAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedDbSetForMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Membership_Members_MemberId",
                table: "Membership");

            migrationBuilder.DropForeignKey(
                name: "FK_Membership_MembershipTypes_MembershipTypeId",
                table: "Membership");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Membership",
                table: "Membership");

            migrationBuilder.RenameTable(
                name: "Membership",
                newName: "Memberships");

            migrationBuilder.RenameIndex(
                name: "IX_Membership_PublicId",
                table: "Memberships",
                newName: "IX_Memberships_PublicId");

            migrationBuilder.RenameIndex(
                name: "IX_Membership_MembershipTypeId",
                table: "Memberships",
                newName: "IX_Memberships_MembershipTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Membership_MemberId",
                table: "Memberships",
                newName: "IX_Memberships_MemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Members_MemberId",
                table: "Memberships",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_MembershipTypes_MembershipTypeId",
                table: "Memberships",
                column: "MembershipTypeId",
                principalTable: "MembershipTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Members_MemberId",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_MembershipTypes_MembershipTypeId",
                table: "Memberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships");

            migrationBuilder.RenameTable(
                name: "Memberships",
                newName: "Membership");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_PublicId",
                table: "Membership",
                newName: "IX_Membership_PublicId");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_MembershipTypeId",
                table: "Membership",
                newName: "IX_Membership_MembershipTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_MemberId",
                table: "Membership",
                newName: "IX_Membership_MemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Membership",
                table: "Membership",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Membership_Members_MemberId",
                table: "Membership",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Membership_MembershipTypes_MembershipTypeId",
                table: "Membership",
                column: "MembershipTypeId",
                principalTable: "MembershipTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
