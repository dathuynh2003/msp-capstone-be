using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReadAtToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d32a6990-ac0c-4dcb-92cf-b1ce001d184b", "AQAAAAIAAYagAAAAEI1FaE2+LEJzshEJdOhA/ClIgjpKdQ/WVVqdxArsEh00e1tvE8BtSAIMaT5m9Cw67g==", "18581b48-5b2c-4f2a-8645-4bc2b12a0b32" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "68c09198-a495-4aad-94d5-fb61e91cde5d", "AQAAAAIAAYagAAAAEIGqHATgFE0JFTe5mN8aacLNM/g6m597s3lhl3/wHzt3cryune4rwsVglA0NMaLoNA==", "aa592c1f-c3b5-4a2c-b3cb-c116d6e990f8" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "01bce75d-6b2e-447c-b7de-3a36d7dd2119", "AQAAAAIAAYagAAAAEDGiCWwO/paPu985tS29CHcdDNjwF2UbvTFEpvcJ06tdMzPbTp8md9QOCIheSoD2lQ==", "0c611c1a-e780-44d8-bba0-d0802a895e8d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "84008e52-fe50-48ef-b6b7-89e56f7f5821", "AQAAAAIAAYagAAAAEMRzFdimFgEQ7gL5FTUc+haOFua35VpbyQeNP8velYUNF3MBOtZH8AqXySjrVqIlAQ==", "ac6936d3-fbb4-4395-9271-d65e1f43975f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d6a03aba-f0fd-4459-95aa-aa229fc47584", "AQAAAAIAAYagAAAAEKBKdjXjNB6HJDyxmUsrvVwJx2GrQDTo9Ah8GC5R5R+LqDQfQj5LcRg68vBQsPHJFA==", "c0b3cc24-ef69-492e-adea-17ce5bbe9f62" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "48ee709a-b208-4c97-a5c5-775cd4f386a4", "AQAAAAIAAYagAAAAEABTb9Izue6Xr33M/5yVuI5bfT7GzFjSUHvhO+ubj6xB4jM0hxsinzYBa8iVcGOApg==", "966a4cac-3204-409d-a191-1399d7279a4b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "96efec70-729f-4cb1-829f-05ac21ba144a", "AQAAAAIAAYagAAAAEF/1BOjtDeeZjSMJZcBt6yA89DcFoP/Tvuaat2NbK3CUlcGuyzMGWVVsUmH/MD6saQ==", "d9fbea7c-5cb3-4312-8b20-6c2248cfae46" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "173b4e5d-7f42-47cf-856e-a75b452c06b6", "AQAAAAIAAYagAAAAELK/HdMPl1cZdXtVquXRwudW3nAqLo8RQZEydWPo4qhUdj8Cuh6+B2LVkZ0OXt7SEg==", "4ae61998-9c64-4d91-8a8d-210211163553" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4c2e1299-4b9f-426d-883b-e21ba3ccf2fb", "AQAAAAIAAYagAAAAEPVnc9FJmgYL5iJvkwQ0LUGTxQHsJu2eFvp1sc/bwpkWt+btMFF5+YAgacHfYnXIXw==", "bc6b1bd0-b806-4be2-a21a-d2477440781f" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Notifications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "87d83425-b2e9-4c1f-b448-feaf31e37055", "AQAAAAIAAYagAAAAEGL4ylaSYUAqFmM18wD6IAPNW0PJf67pU/MnGrja7kxtO0XIw8AH7wqVcZ/XYEuFfg==", "7c76f18b-b31b-4d90-a178-272b1fd5775d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2b23594e-58be-4574-b428-ad4a60474176", "AQAAAAIAAYagAAAAEMBYBk3jm+t8dfAIc+p2bXMxAlYze44by0PShBgThCztD67hZUeKflWQmTPwgoIQkw==", "e9103a87-4f2a-4766-b4f7-60a7fe7cc4d6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9b3d8634-c939-492b-9d02-39ad6e4536eb", "AQAAAAIAAYagAAAAENeuCcuElvvgTBQUMpq7aF/GQH5UwVsGunZ2h0xXvlOEghtv19ZypRQSoNkZx9MLBw==", "423e7dff-8f93-43f4-8b02-aee78d4bf9b2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f271fdb8-eb4d-439f-9695-14cd1ab0b03e", "AQAAAAIAAYagAAAAEKj21kaMXT/Pr/Z6Iqi1wiuVX3zqe5vrOK3/aCx3vlouIm8tfgPVF9DFzVSRHBmu1Q==", "3b6f8835-959b-4a36-b692-066591538cb2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f6571332-d958-4f91-9178-899e89117038", "AQAAAAIAAYagAAAAEMYwezp36Ps2DOuB6hFgV5yWwwd3Y307le3vYWjU1uM5G7f0z+t4hipcnvI+gE1u3Q==", "cb6e3402-89a8-4de4-854a-edefc2130959" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6d66b8e6-031b-4793-bd01-9756c24f22d4", "AQAAAAIAAYagAAAAEGbztBsDZ+cq8F+Cqp28+OB/DNIBEPyDMuDXnB/gECZa6IdiZIAABOAeJn+xyLSeqA==", "a2541c61-efe1-4ac8-b816-9f8254466f6b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "373e8467-81e4-477f-b62f-c8996a9c0b31", "AQAAAAIAAYagAAAAEFmTBOIBCDBsmUgykmk7RCKJ/MNOg/tVMtbQ8y5xUpjirJ7XbRQuMQs13bSxnyX9DQ==", "71df7da9-6a1c-4a34-8c9b-03970716eb1f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "119a9f8d-07a3-4435-b0c3-6a8da59dec88", "AQAAAAIAAYagAAAAELaiPcTyXuJis11u12vKY/Pi22ma0VgcF1/w169e1N6KuhIGaymdv87ufa6pLJj4Lg==", "70e3bd15-7b5e-4e1a-a453-1e6f4a3d14f0" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "eae89de6-3eed-4750-b494-970d8298f8f2", "AQAAAAIAAYagAAAAEKCO8Wm+bvm+GChA+RMAOCLyTg9mzbY5Sya7UwQ2ZlbeHqvpsTsydYWH4CwJxELT7A==", "13be3f77-9f8a-4175-97c3-db4886cc6acc" });
        }
    }
}
