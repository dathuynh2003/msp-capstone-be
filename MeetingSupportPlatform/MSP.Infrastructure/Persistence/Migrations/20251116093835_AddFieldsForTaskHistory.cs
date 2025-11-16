using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsForTaskHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ToUserId",
                table: "TaskHistories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "TaskHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ChangedById",
                table: "TaskHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "FieldName",
                table: "TaskHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "TaskHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "TaskHistories",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a7237d28-874e-419a-939a-83501b18ca33", "AQAAAAIAAYagAAAAEJIlUrS4epydsgeBi2TZu6DgjTCzw6ynLYDA3AvLtNPOUMgf+Qo6f/tqXyhQv8rlRA==", "832b0079-e2ec-4217-9daf-cf2afdd77918" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "99f3c792-d349-43ae-bf19-b7876df692a3", "AQAAAAIAAYagAAAAELMa5yZymzsqMiltry8m79TTky1y3l1OcPcc6j2+3G1J/ewYotVpF56kGzDBHTd8yw==", "060e789a-3004-4e07-97c0-243587490393" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b4370a17-1616-49e5-ae01-87aba7c4c7eb", "AQAAAAIAAYagAAAAEDJ04ad26da+4Jlvel+3ismDh4YG9pFGfa/RreO9s3/05al3cPzgyeYDaCttB65Sow==", "cf38a86e-05ae-48ea-9084-e45e51828eca" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "33be5b23-4b70-4ffa-9b39-032c564040dc", "AQAAAAIAAYagAAAAEPgsU7dapy2mImVKn1qViHdmnmeVY1yX6R5uSndk1L+68eswoOYblQdwSf5EMSubBQ==", "565fde08-2b7f-46a5-a084-60a6f1d08ed5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "702541bf-1863-4815-8ad3-2031cb6d410a", "AQAAAAIAAYagAAAAEIKlHiQWa7nQVSfeF/oCzlx0FeL9vvEbP1rOsLG8hoC6r9MJSQtQ6jSs4gc+BfxdBg==", "170675fa-855c-4631-9859-a0a5b608ac51" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "479ea9fb-ee92-4862-b282-46219b37fc67", "AQAAAAIAAYagAAAAEDtJhZsBZiqodyS5d9x220ZZsPhZsWT9B7Ecis4JHAJDOltcFVD62eRz1OfouqHgGw==", "8f2cb858-bf00-4201-b92f-217029b9f85c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a088a8ae-8edb-49bc-8399-ae1536276f6c", "AQAAAAIAAYagAAAAENPTXlwLa7AzMW0PHaVo8l8KZfRHywolyw0ohpWsQWSAddKyHr/Yaux+YqFH7m/BCQ==", "b3cc8599-a22c-449b-b9ea-65289efaae3c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0bef7cfa-f791-4a93-b08a-0d01be81e9bd", "AQAAAAIAAYagAAAAEPOs6hqwcM/Pc/NQtfpTCAzKR3NRx0BNLp4Xl2nCmyU2fZ9Ioo+NFh8oKKJszUDD5g==", "7f9c2725-cb93-4965-9e3c-c4e9f9a6e0b9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c929702c-8345-44ee-9e4f-af33244d403d", "AQAAAAIAAYagAAAAEG5+8GhKzfzvSjeL5Fm/Vwie5ytmDDt7tbrR6Cpkfqt+y0lstC0qqd5Tn7H71HfJ3w==", "2ca3ccc1-19fe-4624-abec-c49fd5152366" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistories_ChangedById",
                table: "TaskHistories",
                column: "ChangedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_AspNetUsers_ChangedById",
                table: "TaskHistories",
                column: "ChangedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_AspNetUsers_ChangedById",
                table: "TaskHistories");

            migrationBuilder.DropIndex(
                name: "IX_TaskHistories_ChangedById",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "ChangedById",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "FieldName",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "TaskHistories");

            migrationBuilder.AlterColumn<Guid>(
                name: "ToUserId",
                table: "TaskHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "331c3feb-3362-4154-8cfc-3b42515a7e65", "AQAAAAIAAYagAAAAEGxtFoVa+vSDwcm2IGEX15z14tJGqLUqSijN/o1i13BMpD43c6Z175XmZqjbYfXo2A==", "6d08136a-d0bd-4b16-9a1e-ae93acaaabb0" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d2f0303b-13f8-4f93-b111-2b2162ff0a38", "AQAAAAIAAYagAAAAEIxcpmZ1LftStaoOEOKXx04QRUREE+m3xvf0yJYWIlLQn59VwY5ejkxOFgTto0Bbcg==", "ebb72f45-c268-4124-9fac-fcdae191962c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4330d59a-34f1-41f3-a87e-2cd75f395117", "AQAAAAIAAYagAAAAEMXFhiFgUDCxbvWlWcTVoysvkuFpvOooZJxtyOmsGnmpZdaqus2oMviwME7tPyIFvw==", "3e7d4314-5b65-456f-b0a3-feb9af135071" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "786e94af-b3d4-49c4-a7f1-cccf66706874", "AQAAAAIAAYagAAAAEMvQ46WhM2aZRBN3l2/succpsMI9Zhy8NUl0UFX5HkGjB1SXRouRXwNShEaATkNBeA==", "972b5162-4c7f-4b68-a6fd-bf342f4d9f1b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2fdf8358-4f1b-4221-a0db-3df9c2fbde30", "AQAAAAIAAYagAAAAEGIYPKlsYC6gM3UU0b2cUyIkkvtoJ3puhJtu+20u7SLvV+ZcPVdNMDQbZEhhlwsYug==", "53eb3a83-f8ad-46ee-81b0-1d76bcc37d97" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e3282074-7cac-4af2-aefd-22d5763d143a", "AQAAAAIAAYagAAAAEMN2VIz4RNymezt0ONQ7NZFBwSgAF+UlPOaSVGnz9mO1FULBr6ZHu30TevHk8zNOCg==", "a5c5f705-cbc4-4443-9eb1-6dc095342362" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "080c5712-f2ad-40fc-8595-ecc790076819", "AQAAAAIAAYagAAAAEOu0249uHLVIvxavYY3+vzSFx6kf+WnCTlEHwrZLNSAlPy96qF+Eg+eD4RwniwolEQ==", "9edfa389-99f0-492b-8b61-eef0ce6c73e9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "db8d2397-1042-492d-b891-bfd06bd6e122", "AQAAAAIAAYagAAAAEL0ThIEd58659/iOMmF912qmk6Hqc4+umkIfq9s+BoDc2Cc8N3wIXOIYI5HIv/VQtw==", "5a2ba238-aea0-4bcd-9220-f95891177aae" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d06faf3e-9422-4f4a-9fc8-8973e0311caf", "AQAAAAIAAYagAAAAEO2LU4Aa5buq36ZeBYC9nH0UZ2myQYH/OrVS8vVsbs/lQtlhXCIDloRGAr7/AQZzOQ==", "c5493611-fdb5-4a57-b1e8-8e7ebcb0c1dd" });
        }
    }
}
