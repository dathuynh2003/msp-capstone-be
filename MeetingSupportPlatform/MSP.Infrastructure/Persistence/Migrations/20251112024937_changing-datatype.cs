using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class changingdatatype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Packages""
                ALTER COLUMN ""BillingCycle"" TYPE integer
                USING ""BillingCycle""::integer;
            ");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "55cfce01-547a-4184-95f3-312a08eb409f", "AQAAAAIAAYagAAAAEF8ovOheqADfDQnwZUeqPErCi8p5+mf0LqfGiRc6ar1sVlD9xduBWpARM6swjZl5ww==", "b915a8c1-30bb-42b1-ac89-34730dfe024e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7d1abcbe-49e9-409d-99ed-6f3015fc53db", "AQAAAAIAAYagAAAAED7qVRw/2JQbw8HuvlmH/iVZ8ehGQh7bUPfcYb9JbaEldbNzi/8PdXhCmfmeO+fTNQ==", "1016009f-0fc2-484b-8661-cfeb00c432e0" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bde5693e-ca7d-4f75-bca7-218f6fc75b17", "AQAAAAIAAYagAAAAELNoq7X+gfPtQETZXWZVMm3FzF1OfqFkyck76E1LezZ4UD/qs6Jnwoj+1fNuBNJyvA==", "a6595510-76d8-4880-aa4a-fc1a6a5c831f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d2c805c6-5028-4b4d-96ee-29793097d026", "AQAAAAIAAYagAAAAEGNlSwDWOOzcfiTRkZ0cK9X+KhLt7I5/lIjCTGVTVkSamfCkyu/RuIFB2l9VAOCwgQ==", "dc8f466b-1ac3-4f51-a770-c23c2997ff79" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "85fc6204-e4c8-4522-b5f6-bcb317f9f1f9", "AQAAAAIAAYagAAAAEJexlgncqC6bSmLMVGjdKgS109dYZOvCqw7g+N+npTNgyd5Vm8gYZE8WNdAxuGphgg==", "e3c3047e-a1fb-49a9-9568-0cf9a791a7b2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3979e834-37f1-4bb2-85bd-5f345d4c60d3", "AQAAAAIAAYagAAAAEGjrVmkZLYOgScrzUkWxqAiV2B9x74sz93+HHzF4os/B6bPnY+98jm2tUjJG/LUyZw==", "90da4be2-e15f-4c4d-8c2b-d20025e3c014" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "25b97388-5c7b-492b-b08a-02bb69b03eef", "AQAAAAIAAYagAAAAEIdqIU5RZhOEb2RvvsbJ2mfykN6YfA4ImkR1ZvtkcpwU2dXl8FHvhAAYKp5Q19kwCg==", "9576bb23-ad90-4be2-b402-6dc1b108a84c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ecc8b701-1244-4204-b155-5ab841583ac3", "AQAAAAIAAYagAAAAEL9HAkG32foBic0IxE7xao+AwTzPoX9/oxoHSDX/VtyUqLs6Lx/afqMWCK6yZ8Gglg==", "5a96ff1c-a0d4-4de2-8e54-190ba080b1ef" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c2a6bee2-e552-45e3-89de-f79170c55d0b", "AQAAAAIAAYagAAAAEKsuN0u1jsz0Hq6DuciTIdv28NI3p+JNVz7oXrXwNWkJaOMYdKNtUTy+huwr88b7bA==", "04b4724b-db14-4c36-a78c-6507f40af561" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BillingCycle",
                table: "Packages",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4820fe9c-871e-4d28-adba-0e86e86e08c3", "AQAAAAIAAYagAAAAEFyXJbLzkK8A7E8H2fjDVaHCh8vEVnVnyNhDlERHZVYqZZ8oiHzyBC8IJGTM1vsz2w==", "ad811c79-f10e-40de-887b-53059ae5090b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "36b49e05-46fb-40fb-b035-6041e77b53c1", "AQAAAAIAAYagAAAAED6QqnHjYwpYUzZAoUAWFoiTGiFYwAdmWrp1q5L/re1mtjxYx4z0dRCykCdmG5bkDg==", "64831ea2-a171-426e-b4d3-119d4188b475" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f35d3654-6277-4ab5-a445-80f9d485ae74", "AQAAAAIAAYagAAAAECdSBsuSNxcNI/kgAlbEtx2m2MrB7Bbl+ZOfQ/GhQrZnWo3GyZllpmeLduoEPeIDRw==", "c6a3df81-ae91-45ad-a3e9-ba69c044740f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "88558d9a-de3e-4429-a64a-03e76291a713", "AQAAAAIAAYagAAAAEP3BphC3V1uYgHvt4/cS0oGGjR8E/SQY9ClsLXN2l1FO/Fumx8FH6XN/OnF+0tRWfg==", "0f1541d3-dfaa-46ac-a5cf-f401154036f7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6d3a3071-8afa-49e9-9570-65b89c2fba52", "AQAAAAIAAYagAAAAEPHWg7jv9W7OWXjk5RaITR++YWG1hTbbAlIq2OZ01Cgjm7pK+6oFx6YV6AwZRJ+JzA==", "803354f8-bb58-40e3-80db-61c577d8ee72" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "90d5347e-118d-4e78-b19b-5362af246dba", "AQAAAAIAAYagAAAAEKtF1ZvRoWtx8rBvPHUAaDk+JLcBLNsgn+f9eitLH2fgER0AujmkmcKmeU+RwTk8pA==", "ea8061c5-9e3b-410f-bea0-1d03ac1f8c5e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "99d9689d-0127-4a87-9ca3-aad136355d55", "AQAAAAIAAYagAAAAELG8FWYYYRv4Vb5jsObE+JMpjYViNT+OJZj96o2YBeqSfAYgJQTggM099LrO+qNa/g==", "c76ecc82-26c6-485f-a699-1c3a28907c25" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "73ba14b0-4110-4070-bf5c-a2e852169cf6", "AQAAAAIAAYagAAAAEPBohWsnnVj0a0KUlC6ffNqDockOtP+aTCzi1oHSqlJ7U6AYI9POO9DPZSHwjofP4Q==", "88f98561-9418-4bab-87c9-f5f22ad42c29" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "65f62f71-7dcb-429c-a60c-db1f0962a1a1", "AQAAAAIAAYagAAAAEL9qQmmzPdVZBlJY16ebGAM08ji3GZZzL465uwm5I2RIKHe+4x8Sb7jw+ZdvWeCQng==", "e9d40e22-396b-4714-b1fa-5cb1d545ef96" });
        }
    }
}
