using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminSeedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FVAGXVcS0AirO77oQQhYCccw9UMGHI6sTDKtLUGv7sU=" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 15, 8, 30, 56, 992, DateTimeKind.Utc).AddTicks(8700), "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=" });
        }
    }
}
