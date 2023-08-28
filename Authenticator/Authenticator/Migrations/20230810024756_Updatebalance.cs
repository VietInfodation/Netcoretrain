using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authenticator.Migrations
{
    public partial class Updatebalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BalanceAccount",
                table: "AspNetUsers",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceAccount",
                table: "AspNetUsers");
        }
    }
}
