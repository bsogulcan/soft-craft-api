using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftCraft.Migrations
{
    public partial class AddedEntityIdToProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Entities_RelationalEntityId1",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_RelationalEntityId1",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "RelationalEntityId1",
                table: "Properties");

            migrationBuilder.AlterColumn<long>(
                name: "RelationalEntityId",
                table: "Properties",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EntityId",
                table: "Properties",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_EntityId",
                table: "Properties",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_RelationalEntityId",
                table: "Properties",
                column: "RelationalEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Entities_EntityId",
                table: "Properties",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Entities_RelationalEntityId",
                table: "Properties",
                column: "RelationalEntityId",
                principalTable: "Entities",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Entities_EntityId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Entities_RelationalEntityId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_EntityId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_RelationalEntityId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "RelationalEntityId",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RelationalEntityId1",
                table: "Properties",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_RelationalEntityId1",
                table: "Properties",
                column: "RelationalEntityId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Entities_RelationalEntityId1",
                table: "Properties",
                column: "RelationalEntityId1",
                principalTable: "Entities",
                principalColumn: "Id");
        }
    }
}
