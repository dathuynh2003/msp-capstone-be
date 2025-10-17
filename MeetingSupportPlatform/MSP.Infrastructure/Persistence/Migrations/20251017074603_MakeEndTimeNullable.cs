using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeEndTimeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Meetings",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "40711b20-4934-4340-8c46-a7bbb7babfde", "AQAAAAIAAYagAAAAEAw6VVHH1rbEcgbkRyOHvYQT7imddPTsDvtNDjqycWk7Ncqj+s/Jo6ByFr42+X2wTA==", "b9596d22-15a3-4580-b083-010ce44dbe39" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "055ce557-e033-46e2-a2c0-ab72e170041b", "AQAAAAIAAYagAAAAEEHqvc+yUT/JT0WNg79rE9NxQlAluevblZfMifd15ZYisTKI9LHYz2Q8CrUvHAz2wg==", "ad6273f5-1fc2-47ca-bf90-700830ff2451" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "65c82eb4-9ee0-43fc-a099-cc6129281712", "AQAAAAIAAYagAAAAEFmYlHTLV+OAwnrFsBeooIs6R2V7U/COkzZgX1HCzN6Lvqa+t2hqrv+g7l3NTkzS4Q==", "0402d243-6916-4a10-81cc-1a3518188bb7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f8150d49-d1a4-4a06-836e-d0f3d1ab74df", "AQAAAAIAAYagAAAAEDiT0ThwoV1WAXCagyaDqOEaux11uDVR3x9sVUz7AGM5lmlrtevKvw+aTdxkYcpI9Q==", "4cf951f9-bee2-47ce-9fb3-d5a3a6ecf69b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Meetings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ee516da4-9365-468c-a59f-94791ce1badd", "AQAAAAIAAYagAAAAEICJxt5yxKSTvQGPKP+uzeFFjLqCxTZThi3W3AB/kB+lf8O30nxJ/s/aqkxIX8hlrA==", "3a1d5b41-8068-4416-9c41-cb5584ffe50b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "519a2ea4-d5f1-4cc6-bf55-388dbaae380a", "AQAAAAIAAYagAAAAEKscvr27Gt6Y4mf9iyeJEGETQccvEC9T9/RRASiUQb4ZtYamgfGo9fmJM8rJbKYQOQ==", "59c88299-c59b-4f13-92e6-0effd4d6b533" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d6abc91e-7ea1-47c5-83e2-91a4a8097ebb", "AQAAAAIAAYagAAAAELe4Hf/vhUkKTX7vN5rW9UaHd7DDeJFMpsuL8mqJTJBAE9/rFgydo611dZ4MRwQ5/g==", "51fa33e4-3ab1-4527-8d68-9984e98d6d94" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3138c5c7-32a3-44bd-8e1d-4799eefb7644", "AQAAAAIAAYagAAAAEB3XInWD7TL6+xaTOHZzWT4eKjQJfpf6Kk748Xe/BGN7cSOe2Dc90809bvO0Bi3Cbw==", "9bacd85f-d14b-4154-8d64-a4057ccf227d" });
        }
    }
}
