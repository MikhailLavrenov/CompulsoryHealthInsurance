using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CHI.Migrations
{
    public partial class CaseFilterAndIndicatorMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseFilters_Components_ComponentId",
                table: "CaseFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratio_Indicators_IndicatorId",
                table: "Ratio");

            migrationBuilder.DropColumn(
                name: "FacadeKind",
                table: "Indicators");

            migrationBuilder.DropColumn(
                name: "ValueKind",
                table: "Indicators");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "CaseFilters");

            migrationBuilder.RenameColumn(
                name: "IndicatorId",
                table: "Ratio",
                newName: "IndicatorBaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Ratio_IndicatorId",
                table: "Ratio",
                newName: "IX_Ratio_IndicatorBaseId");

            migrationBuilder.RenameColumn(
                name: "ComponentId",
                table: "CaseFilters",
                newName: "CaseFiltersCollectionBaseId");

            migrationBuilder.RenameIndex(
                name: "IX_CaseFilters_ComponentId",
                table: "CaseFilters",
                newName: "IX_CaseFilters_CaseFiltersCollectionBaseId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Indicators",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsTotal",
                table: "Components",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CaseFiltersCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFiltersCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseFiltersCollections_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseFiltersCollections_ComponentId",
                table: "CaseFiltersCollections",
                column: "ComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseFilters_CaseFiltersCollections_CaseFiltersCollectionBaseId",
                table: "CaseFilters",
                column: "CaseFiltersCollectionBaseId",
                principalTable: "CaseFiltersCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratio_Indicators_IndicatorBaseId",
                table: "Ratio",
                column: "IndicatorBaseId",
                principalTable: "Indicators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseFilters_CaseFiltersCollections_CaseFiltersCollectionBaseId",
                table: "CaseFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratio_Indicators_IndicatorBaseId",
                table: "Ratio");

            migrationBuilder.DropTable(
                name: "CaseFiltersCollections");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Indicators");

            migrationBuilder.DropColumn(
                name: "IsTotal",
                table: "Components");

            migrationBuilder.RenameColumn(
                name: "IndicatorBaseId",
                table: "Ratio",
                newName: "IndicatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Ratio_IndicatorBaseId",
                table: "Ratio",
                newName: "IX_Ratio_IndicatorId");

            migrationBuilder.RenameColumn(
                name: "CaseFiltersCollectionBaseId",
                table: "CaseFilters",
                newName: "ComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_CaseFilters_CaseFiltersCollectionBaseId",
                table: "CaseFilters",
                newName: "IX_CaseFilters_ComponentId");

            migrationBuilder.AddColumn<int>(
                name: "FacadeKind",
                table: "Indicators",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValueKind",
                table: "Indicators",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "CaseFilters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseFilters_Components_ComponentId",
                table: "CaseFilters",
                column: "ComponentId",
                principalTable: "Components",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratio_Indicators_IndicatorId",
                table: "Ratio",
                column: "IndicatorId",
                principalTable: "Indicators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
