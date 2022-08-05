using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftCraft.Migrations
{
    public partial class PropertyReleation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LinkedPropertyId",
                table: "Properties",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_LinkedPropertyId",
                table: "Properties",
                column: "LinkedPropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Properties_LinkedPropertyId",
                table: "Properties",
                column: "LinkedPropertyId",
                principalTable: "Properties",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Properties_LinkedPropertyId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_LinkedPropertyId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "LinkedPropertyId",
                table: "Properties");
        }
    }
}
