using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentMaterialProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StudentMaterialProgresses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StudentMaterialProgresses");
        }
    }
}
