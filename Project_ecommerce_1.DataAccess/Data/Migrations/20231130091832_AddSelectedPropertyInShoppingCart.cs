using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_ecommerce_1.DataAccess.Migrations
{
    public partial class AddSelectedPropertyInShoppingCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Selected",
                table: "shoppingCarts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Selected",
                table: "shoppingCarts");
        }
    }
}
