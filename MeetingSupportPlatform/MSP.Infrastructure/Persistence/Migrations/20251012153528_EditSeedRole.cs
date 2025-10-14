using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EditSeedRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "40cf9cc1-e934-443a-bd67-f238823f013e", "AQAAAAIAAYagAAAAEKizMIOtnglBV63sQmoAWrg2T/DP2N54skdXELLqQJ5p0IGAPcp1KKqEQjg76vv5MA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "d5a9b8e5-ffa5-42bf-8f87-708c91816ae0", "AQAAAAIAAYagAAAAEI2P0BsvL8CrrGdBP2cInY/tYtlj7ez1mjgzo7JC8qYNw0Z84NRI7rvq97kpvRYm0A==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "37646e3d-e1a6-4646-bda0-5cf220680c96", "AQAAAAIAAYagAAAAELsNlxwaEb6RkA9Uj3EdjjpAnYpp9Pz9Rlpp3ynNoMr3yw07IvgydfdYpxBphEU60Q==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3b856b77-67ef-4f1c-8cdc-996fce960989", "AQAAAAIAAYagAAAAEIllD2pcDOGSm+bAHNOrpY2bjFwQnwGFhAlN2E4z+vjVgGDtj1kg+cQ0NTOnUQgfkw==" });
        }
    }
}
