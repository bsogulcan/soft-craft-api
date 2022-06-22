using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftCraft.Migrations
{
    public partial class _0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Entities_EntityId",
                table: "Properties");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Entities_EntityId",
                table: "Properties",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Entities_EntityId",
                table: "Properties");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Entities_EntityId",
                table: "Properties",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
