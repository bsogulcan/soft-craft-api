using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftCraft.Migrations
{
    public partial class ConfiguredEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "Projects",
                newName: "WebAddress");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Projects",
                newName: "UniqueName");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Port",
                table: "Projects",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LogType",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "MultiTenant",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Entities",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "LogType",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MultiTenant",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Entities");

            migrationBuilder.RenameColumn(
                name: "WebAddress",
                table: "Projects",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "UniqueName",
                table: "Projects",
                newName: "Description");

            migrationBuilder.AlterColumn<int>(
                name: "Port",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
