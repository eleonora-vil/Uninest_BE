using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_EXE201.Migrations
{
    public partial class v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoRenewMembership",
                table: "User",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMember",
                table: "User",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MembershipEndDate",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MembershipStartDate",
                table: "User",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRenewMembership",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsMember",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MembershipEndDate",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MembershipStartDate",
                table: "User");
        }
    }
}
