using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskReassignField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponseMessage",
                table: "TaskReassignRequests",
                type: "text",
                nullable: true);

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
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a210a82e-1cd2-45bb-9641-590f4b1743d6", "AQAAAAIAAYagAAAAECsIr5V3KpF7PwNyzIE8MwfsDBcHxtq6/TAbqmW20B9x5VM1IuIUw52MBic29SkmzA==", "20dc2eac-694f-4fae-9e72-997ba6e69a13" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3a325766-b140-4af8-95b7-9a7623a7ad17", "AQAAAAIAAYagAAAAELUNhKjvnZfJeHouOY3nURC4Lnp6ZQc9CjCpNkgFnn3TGJxP9kRZS2hcmStGqtZNkQ==", "136bfe9d-349d-420d-9dc7-9840e12e889f" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseMessage",
                table: "TaskReassignRequests");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c521f2c4-9901-4214-bad5-4ee448fa3834", "AQAAAAIAAYagAAAAEAQkySneQ7feM7ByZUzIsnQQBhxZ59tyCzHUf1wJGRh5fTrUdy6DFRciTwCF9uj+cA==", "fd666ec1-ab5d-4fbc-8d0b-eaa20079dafb" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7f966620-6a5f-4e52-ae8b-aa0805e880da", "AQAAAAIAAYagAAAAEPHxC7BHQeg9txRVRCTbWaFdPxE30k541NeaFpik8gv5ojTSdsY4jAdJIysYxJjx/w==", "ac5f1973-8682-4454-b229-10356bd00ad5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d6b1ab3c-a219-4876-aafe-e0425a01c2fe", "AQAAAAIAAYagAAAAEC3hCYkMMs4TQGWwKjx8FLk2/2JFX9QpfZOSs6B8cyeMO8SsmqiBwEkZquWAsRWNCw==", "3fdc4863-fed6-451c-b54e-86bf54b36b32" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9cadf0aa-c076-4e5e-9058-5c1af076c6bb", "AQAAAAIAAYagAAAAEGSaS36xe2BkSSVVVgtnq/RB4JSu4PsndOIN1GTTQSgqZStCHcVkgxS2pxbnGXL2sg==", "f0eaa6fb-41dd-4945-ace2-c9a437f47a9f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "32cd2936-234c-4ed0-af97-eeb22aa951af", "AQAAAAIAAYagAAAAEKcOGNPQtGhx5ooAR5Gs7TtFkFAMHt6Btc8pBpTGZsRJnsmqYX9vAKLN9APCYQJqtA==", "76000b65-0515-435d-923a-970d161d011b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "db958469-7857-4d70-b79f-6955f06eaf2e", "AQAAAAIAAYagAAAAEEmMXfhjm9pxNLYtGdp0lHPLYRAakZC+Gbh+0/vonnbQX8C14kJoyPnWbIr3B5xMFg==", "f84f302f-fbb2-42ad-b522-aad4d9db90b9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4c482689-d9ee-4ffc-9a5e-0ab094a4c003", "AQAAAAIAAYagAAAAEF65giOE/iGgud/5oWw/vS5OxaRG0WT6EqC+MqT+FAO48ydHLRezIN+mCGNuaQ0Txw==", "5df71632-d00a-436d-b3a7-142fd3492a67" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5e50188c-48eb-4ca4-a790-e6b9eecece6a", "AQAAAAIAAYagAAAAEHTUwFBDx693A9v9rNsppJaj4vASLrmnatTLoCMmTTBmQQ5I4baZQtd3n916rlQ7kg==", "38cd04ba-448b-427c-8cc9-258f2295e294" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b1a3de8a-2eac-4c2e-8e52-0572ec8615d1", "AQAAAAIAAYagAAAAEJOseoHKDZkUH25zVi8Jahn3ACofHr7z+sh+FHJd5w12HzacnqrB7dBR3JzMyWeycQ==", "dbf335f3-042b-452e-bf30-af51097c201f" });
        }
    }
}
