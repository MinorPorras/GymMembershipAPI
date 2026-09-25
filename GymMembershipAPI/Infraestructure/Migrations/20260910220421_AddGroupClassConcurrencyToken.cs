using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMembershipAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupClassConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "Bookings",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastBookingAt",
                table: "GroupClasses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "GroupClasses",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Bookings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_GroupClasses_PublicId",
                table: "GroupClasses",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupClasses_PublicId",
                table: "GroupClasses");

            migrationBuilder.DropColumn(
                name: "LastBookingAt",
                table: "GroupClasses");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "GroupClasses");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Bookings",
                newName: "BookingDate");
        }
    }
}
