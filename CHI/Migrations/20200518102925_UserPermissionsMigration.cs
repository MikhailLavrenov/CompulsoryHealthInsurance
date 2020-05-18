using Microsoft.EntityFrameworkCore.Migrations;

namespace CHI.Migrations
{
    public partial class UserPermissionsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AttachedPatientsPermision",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MedicalExaminationsPermision",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReferencesPerimision",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RegistersPermision",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SettingsPermision",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsersPerimision",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachedPatientsPermision",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MedicalExaminationsPermision",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReferencesPerimision",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistersPermision",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SettingsPermision",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UsersPerimision",
                table: "Users");
        }
    }
}
