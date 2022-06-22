using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftCraft.Migrations
{
    public partial class AddedRelationTypeToProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelationType",
                table: "Properties",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelationType",
                table: "Properties");
        }
    }
}
