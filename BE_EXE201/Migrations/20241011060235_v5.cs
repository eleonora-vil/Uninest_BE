using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_EXE201.Migrations
{
    public partial class v5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "PaymentTransaction",
                newName: "UpdatedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "PaymentTransaction",
                newName: "CreatedDate");
        }
    }
}
