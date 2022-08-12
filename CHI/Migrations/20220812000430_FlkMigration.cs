using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CHI.Migrations
{
    public partial class FlkMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningPermision_Users_UserId",
                table: "PlanningPermision");

            migrationBuilder.AddColumn<int>(
                name: "FlkRejectCasesCount",
                table: "Registers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BillRegisterCode",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "CloseCaseCode",
                table: "Cases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MedicalHistoryNumber",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningPermision_Users_UserId",
                table: "PlanningPermision",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningPermision_Users_UserId",
                table: "PlanningPermision");

            migrationBuilder.DropColumn(
                name: "FlkRejectCasesCount",
                table: "Registers");

            migrationBuilder.DropColumn(
                name: "BillRegisterCode",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CloseCaseCode",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "MedicalHistoryNumber",
                table: "Cases");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningPermision_Users_UserId",
                table: "PlanningPermision",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
