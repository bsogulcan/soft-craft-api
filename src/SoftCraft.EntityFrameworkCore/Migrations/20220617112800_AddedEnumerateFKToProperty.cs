using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftCraft.Migrations
{
    public partial class AddedEnumerateFKToProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EnumerateId",
                table: "Properties",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnumProperty",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_EnumerateId",
                table: "Properties",
                column: "EnumerateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Enumerates_EnumerateId",
                table: "Properties",
                column: "EnumerateId",
                principalTable: "Enumerates",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Enumerates_EnumerateId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_EnumerateId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "EnumerateId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsEnumProperty",
                table: "Properties");
        }
    }
}
