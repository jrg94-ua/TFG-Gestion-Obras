using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionObras.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTareaJerarquia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TareaPadreId",
                table: "Tareas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Nivel",
                table: "Tareas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Prioridad",
                table: "Tareas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_TareaPadreId",
                table: "Tareas",
                column: "TareaPadreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_Tareas_TareaPadreId",
                table: "Tareas",
                column: "TareaPadreId",
                principalTable: "Tareas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_Tareas_TareaPadreId",
                table: "Tareas");

            migrationBuilder.DropIndex(
                name: "IX_Tareas_TareaPadreId",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "TareaPadreId",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "Nivel",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "Tareas");
        }
    }
}
