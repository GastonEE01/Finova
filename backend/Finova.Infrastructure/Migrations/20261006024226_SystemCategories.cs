using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SystemCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Categories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Supermercado", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Transporte", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Salud", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "Vivienda", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "Servicios", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "Educación", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "Entretenimiento", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "Restaurantes", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "Ropa", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "Ahorro", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000011"), "Otros", 2, null },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "Sueldo", 1, null },
                    { new Guid("10000000-0000-0000-0000-000000000013"), "Freelance", 1, null },
                    { new Guid("10000000-0000-0000-0000-000000000014"), "Ventas", 1, null },
                    { new Guid("10000000-0000-0000-0000-000000000015"), "Otros", 1, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("10000000-0000-0000-0000-000000000001"),
                    new Guid("10000000-0000-0000-0000-000000000002"),
                    new Guid("10000000-0000-0000-0000-000000000003"),
                    new Guid("10000000-0000-0000-0000-000000000004"),
                    new Guid("10000000-0000-0000-0000-000000000005"),
                    new Guid("10000000-0000-0000-0000-000000000006"),
                    new Guid("10000000-0000-0000-0000-000000000007"),
                    new Guid("10000000-0000-0000-0000-000000000008"),
                    new Guid("10000000-0000-0000-0000-000000000009"),
                    new Guid("10000000-0000-0000-0000-000000000010"),
                    new Guid("10000000-0000-0000-0000-000000000011"),
                    new Guid("10000000-0000-0000-0000-000000000012"),
                    new Guid("10000000-0000-0000-0000-000000000013"),
                    new Guid("10000000-0000-0000-0000-000000000014"),
                    new Guid("10000000-0000-0000-0000-000000000015")
                });

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
