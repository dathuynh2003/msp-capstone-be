using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRoleConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("d1e2f3a4-b5c6-4789-1234-56789abcdef3"), new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6") },
                    { new Guid("d1e2f3a4-b5c6-4789-1234-56789abcdef3"), new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7") },
                    { new Guid("b1c2d3e4-f5a6-4789-1234-56789abcdef1"), new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3") },
                    { new Guid("b1c2d3e4-f5a6-4789-1234-56789abcdef1"), new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4") },
                    { new Guid("b1c2d3e4-f5a6-4789-1234-56789abcdef1"), new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5") }
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8a965953-3f85-4b50-811d-1b22f742f1e6", "AQAAAAIAAYagAAAAELip1XLJ/cXAirlCCB3a+rjxhThkr8IkutCOHQn6fkIo2ENyp/V9j3mzh4VugoTirA==", "47867e5c-9e89-4d31-b30d-12ada9f7734a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7086aead-fffa-4e16-a2f5-04affbc60932", "AQAAAAIAAYagAAAAEKiepfVA10sjUD00nM0QQc9lTzLmwFoVNzXVZSLo9feF0whe0uTjMz52v3S48C//UQ==", "304954e5-fd02-47bf-8463-68edba63e55d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bb50ce17-51b3-461a-adeb-e064a212e927", "AQAAAAIAAYagAAAAEHqlCryKYrPrtBpJD9pQvEc7FhSLuBvUOJ+Zc8pWDyWxvHF7C2uACHmrkIWaBI6odQ==", "e57b1652-3a01-48e9-8be9-658eec550b0e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9343d291-a1df-4bbb-b6a5-cfd4f071ac4a", "AQAAAAIAAYagAAAAELHzFpwdkeJo48sT4nR3Ar8d3RqL3vUcl1Bq4YsvRsrWTm4ZT9QbaHkKMSpiZKPfhg==", "f9bdbf07-309b-4922-8597-0abc186213ea" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "48544c5f-ddfc-45e8-bc00-73554e926997", "AQAAAAIAAYagAAAAEHO7Ic7IjRAgvF6J3w09SCzJ09M4mKILlwZrErpYFdBQSkQV9GzJRqDuvqNAV13VyQ==", "451bbb35-4cb0-48e3-8aae-1b623352fb0e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "57699dae-715c-43dc-b30b-71da4a0cd2d1", "AQAAAAIAAYagAAAAEI5VCLjtglTlV0K2Ob3gjvxNznEGz5RzSisc74kO7GaMogHT6kv8Ui/uNKFL5+PXew==", "d792c385-061c-4dee-bbfd-f23130eb0f51" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5f102e24-f635-4261-b026-2b5198e2e169", "AQAAAAIAAYagAAAAEK2b4bd5GSUNtVkn3egd3rysXNnaRvo0OVemLIhcCu78Gs7lpAwifMyn6JU2oD/DSA==", "c28d6662-225b-4723-8cd3-fd464efdf1a7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "NormalizedEmail", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7fb9af59-1584-4f86-85ff-ef618d103620", "MEMBER3@GMAIL.COM", "AQAAAAIAAYagAAAAENFQmWvNRIX0J9ty76y7v6AM8/jVbHZclNQ1cYWaX1dtDgFj9boQxvV14PjYUg9RRg==", "f57db8dd-3c47-4d90-8a0f-d7004ebecf72" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "39472ae8-2b66-40af-be83-7df98325a42e", "AQAAAAIAAYagAAAAEJXihz5kSeq+dHsGoqXmaY7/PdZTFmohbO7YuVG1anCjj09pbf223iDjOmN26gLbxw==", "34779c97-48b6-47e2-b962-c2ac6407fa46" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("d1e2f3a4-b5c6-4789-1234-56789abcdef3"), new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("d1e2f3a4-b5c6-4789-1234-56789abcdef3"), new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("b1c2d3e4-f5a6-4789-1234-56789abcdef1"), new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("b1c2d3e4-f5a6-4789-1234-56789abcdef1"), new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("b1c2d3e4-f5a6-4789-1234-56789abcdef1"), new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5") });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ff1dc087-a2cf-49bf-9fe8-86e62c5b05f8", "AQAAAAIAAYagAAAAEPyMA9f7NLEKfh9ZKZ7YiQD+Lwddt04vLCII83SkfXirwSZGLXou+d8Crko0aUeEnw==", "0a5f876e-2cee-4433-8503-a9db630d4241" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f4766e29-5774-42c1-ba83-8712d6c5c15f", "AQAAAAIAAYagAAAAEODkZhK3yLDSQ/sT5eCOjjFXMMwzKM1HcBzVC1bNiG3Uh7A33e+Ks+wj5V6RuM98Pg==", "9022d9c3-1058-42ee-bdb0-a07db82297ae" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3238fb1a-b007-4953-b8cc-82bdab38e8fd", "AQAAAAIAAYagAAAAEPNCU8BBf7EuRqKtDpS1pmlUnYOr7Xyz4Mc3zK1U8021GxYhiAxjIKeaT90O5lv6LQ==", "bdcae727-2b1c-4cc3-a6ef-38cf32bb605f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7c44a979-b19e-48bc-94a2-d39ad8745b24", "AQAAAAIAAYagAAAAEC9QKVI6hdJf59wyqhLhZ7p4FCDmWjpCbg1bLa5fA1qqlls94MjNA8tqPwtcNH/Djw==", "56da9d5f-c531-45d6-a2f9-add531d12e94" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "abd81991-6990-46d0-86ff-19f71ad7cc04", "AQAAAAIAAYagAAAAEDny9jH3AAyMsMRwYF62SL2Z42WCNRHkRcV4hiGWqACX3cycVSEqtHDYqHV/s+ZhEg==", "dcf9852d-e308-4930-ab2e-58f9a173e385" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8d0142a8-c858-4d8d-ae95-fec6ea309416", "AQAAAAIAAYagAAAAELbdLaJK4M6oe7TARzOsZFiLjDGydQ/Wj4/R8xBYrrHPugvUEi3Q99GSe+wcUS6onw==", "cb135bcc-697a-4de9-a0c1-4c7e15dd77e2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "18ab6242-fed8-44e0-8aee-e08ce7786412", "AQAAAAIAAYagAAAAECp3+BTFNQQy4tu8tomDKEu4FUS9mWanGOgK9YjQp+PN5CN5tn6FSqNjbDaPNns+CA==", "6147423c-bef0-4b82-986f-76f9c87f6df5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "NormalizedEmail", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a210a82e-1cd2-45bb-9641-590f4b1743d6", "MEMBER2@GMAIL.COM", "AQAAAAIAAYagAAAAECsIr5V3KpF7PwNyzIE8MwfsDBcHxtq6/TAbqmW20B9x5VM1IuIUw52MBic29SkmzA==", "20dc2eac-694f-4fae-9e72-997ba6e69a13" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3a325766-b140-4af8-95b7-9a7623a7ad17", "AQAAAAIAAYagAAAAELUNhKjvnZfJeHouOY3nURC4Lnp6ZQc9CjCpNkgFnn3TGJxP9kRZS2hcmStGqtZNkQ==", "136bfe9d-349d-420d-9dc7-9840e12e889f" });
        }
    }
}
