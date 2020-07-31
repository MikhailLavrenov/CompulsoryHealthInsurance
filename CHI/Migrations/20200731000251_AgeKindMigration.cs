using Microsoft.EntityFrameworkCore.Migrations;

namespace CHI.Migrations
{
    public partial class AgeKindMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parameters_Employees_EmployeeId",
                table: "Parameters");

            migrationBuilder.AddColumn<int>(
                name: "AgeKind",
                table: "Employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AgeKind",
                table: "Cases",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Parameters_Employees_EmployeeId",
                table: "Parameters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parameters_Employees_EmployeeId",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "AgeKind",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AgeKind",
                table: "Cases");

            migrationBuilder.AddForeignKey(
                name: "FK_Parameters_Employees_EmployeeId",
                table: "Parameters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
