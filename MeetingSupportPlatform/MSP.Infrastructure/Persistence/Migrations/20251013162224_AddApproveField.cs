using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApproveField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "IsApproved", "PasswordHash" },
                values: new object[] { "a97092d5-17ac-4924-9ef5-737b03cd0ef1", false, "AQAAAAIAAYagAAAAEKhhtiqvFD0Fn/0P9xiSnh5s744hNGm6n/jXrcGYrdY73tziviOWLacMTrAURufwkw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "IsApproved", "PasswordHash" },
                values: new object[] { "e25dbc82-644a-4cfb-b316-bd0ebdc97917", false, "AQAAAAIAAYagAAAAEKilJyKD80MAi7SLz/aCyLfIs/XWRQpYUQ+/Oa50vGse638REugr6HGe+6G8talRKQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "IsApproved", "PasswordHash" },
                values: new object[] { "7d6a8caa-f950-4638-b114-4b25be619ecd", false, "AQAAAAIAAYagAAAAELC8t3mjnrmCVzd6mCSLN3KCga2jI9h5SFLp6rG2Rqovdk1ZqDnyc047dRnwV9Onbg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "IsApproved", "PasswordHash" },
                values: new object[] { "61e7ac4a-a39f-4397-a839-7fd787ac0cf7", false, "AQAAAAIAAYagAAAAEPvobHVV74cbCQ6z6KuzHBXnnrMdLOuP0k+CZXdfml5AX5gLddML+0bqpEcx+YSKAw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "d857712d-0680-4677-8ad9-15e68677a489", "AQAAAAIAAYagAAAAEEeA8LSnwWYhyXPpDZnDDwkYmMC+eTRSU532ByeCUZQOs7yc8fVdFJobHzvAujDMsw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "929d4f54-d1e8-4614-ba7c-cd574a0cb3fe", "AQAAAAIAAYagAAAAEFL6d7xQfcZ1XGbGNJGXlUfYT7fvSyOgyyw2SrNGopo+lrI98P6avhcmAO4Te8cr/g==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "f423c7dc-c2ea-4cd5-9c2f-983d11be20dd", "AQAAAAIAAYagAAAAEN9UegLm1SeXi98//Al9KFsNkv/tKd7fdRQCfjFZ/+siXeLAlBaVd0GT8OncIhGX7w==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "1fdcb697-48bb-4753-8f93-6babf1bfcd10", "AQAAAAIAAYagAAAAEOmN1uQEmX3WtAFvxgGho53Il+PsKOuhQFj1+BqGHoBD8BKxWgIUWQpWtaJYmPr2Cw==" });
        }
    }
}
