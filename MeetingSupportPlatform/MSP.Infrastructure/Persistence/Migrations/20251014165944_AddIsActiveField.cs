using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true 
            );

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "5b846257-39fe-4179-8523-9f9b97ac2284", "AQAAAAIAAYagAAAAEDrbbGslw0xzm5Be1193rOk5MHt+ob4xTelOhomsxNSZTYciNdB3hdK83Upkh+oChQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "56a30d78-a7ea-44cc-861d-7b7d1b469f62", "AQAAAAIAAYagAAAAEF30GgemtrODKKA2jMVRjeAYt9+eFz0W7YeAbdxll2V8IhipW+KbW61R+jU8u5RxMg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "28548a6d-e9f6-4749-8063-6028fc87dc8f", "AQAAAAIAAYagAAAAELtVNaDnvPb7H14f56Y/N33DDcNwzij44p+vmVq04/4tr8M6pZ1FQtHRG5mY7dNvgw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "503b8b1f-6cd0-4039-ad40-40b0384d4b4e", "AQAAAAIAAYagAAAAEKRtzyvEA5DCHZ/8EjjrqYVCv8quY2DXJ9qaY6Evx+8IZUzF8+ywZotYtQdJK+up/g==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ❌ Xóa cột IsActive khi rollback
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers"
            );

            // Giữ nguyên phần rollback dữ liệu seed
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3345a07a-cd3c-4ae1-9463-de6e48237d4c", "AQAAAAIAAYagAAAAEE8viP36v2VxHb54YN4wRq/OY8ipM2kKYXc4FFYlrIAccEZvo7QwoNmQmtGg0Y8wJw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "6fce7f2a-5481-4048-afbf-a5ab65c2fab6", "AQAAAAIAAYagAAAAEMfFmt51TF95arXGQFGrrbOBa8aMkVtJ0dU1Uo67jF1Rhl7Rxjz6N9xxUvzswpkJMg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "51e7accf-6d7b-40e7-a021-5fd5e128210a", "AQAAAAIAAYagAAAAEC60/kbdm9uLgTRCOYJ0UH3fqLlfhNQRMJftctO9yitqw03bczhCewYsgHzj479cBQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "70d4e2bf-a488-46ea-bdbb-bf073474dd37", "AQAAAAIAAYagAAAAEDnIpXeMSeyEm9ZwWXexJpKapnHM52rfP5svcFpXdKqQcA+PyLQRYC4hqXchpjSDwQ==" });
        }
    }
}
