using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDevicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FCMToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f2996245-bc5f-45f2-a3a2-2ee0282cf2a4", "AQAAAAIAAYagAAAAEJ9P5uEVpmoMa6950s1iSGDEmFJaTqZrkhBUPVPs8RzCvU3VUXD6v3j80JqaMMD4mw==", "c8e1df5e-5ac1-4004-ada7-a602a0f62ccc" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5dd056da-c683-4647-950f-884778332efd", "AQAAAAIAAYagAAAAEOUvKu1usc555r33Fl105c2+Y2BOVQbgMuTrl77C9NerqRZfxKyvmPTxhkL3Z9at3Q==", "d3ba5100-4834-4311-a624-9ac2a11e9659" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1dac9191-b448-40d7-b03e-96d6149762c9", "AQAAAAIAAYagAAAAEFxQ0y2sSpxIDZ2R+jwAIaR4flvKEeKnD+rUih+D5/q5zPhYEHQDN8Mfckzd8wsyMw==", "668cb7d3-04f3-4fed-9da9-f5c782e76bec" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "217d2aef-0f55-4e72-b6d4-ed7dcbc6529a", "AQAAAAIAAYagAAAAEG/TsMYMFYloVbH/oq02kDNTNhip6/ig2UAka7PptFifYGQCPz7dXrM6Sh9Cy3/yyg==", "13551c31-1e4c-421e-9d10-eb1940ef2251" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "cb9ab1b0-37dd-431f-b234-67587004beeb", "AQAAAAIAAYagAAAAEIIWurJ3sJuvnejJVxGGFfZp15HVnDdR7VwgEwWAUeR8pN2I6ftZ4sPlJwqPIZ+M6Q==", "95fc7151-3d1a-4370-b80c-519efe31d1c8" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "48a00da7-f3be-4c9f-a074-ce96aebfd708", "AQAAAAIAAYagAAAAEA5rZfUqE7qlca3qSuVtwY0LgLWwz/oeOY6eB+NzpF7PQ4irpeietwT2KYpyvMBuSA==", "c9b6cd98-4d71-4027-997a-ede5b8d2ae17" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "53f5506e-3f1a-429a-9357-8de966084271", "AQAAAAIAAYagAAAAENdR1qiCmPOiS3W8/jXLRv5Tp0FqUNHfCS0P41STpsPauNcR2qrlnIQYiXjHJ/94zg==", "a25d3944-66ca-474e-9322-08acb022733b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b7c4c32f-303a-4b48-a945-ccbe2a7b2571", "AQAAAAIAAYagAAAAEHS41ov/CcQTFl0F2mqYZTqd73UyBz8KxM2Ob5P+KsRdsJ7+ktcYoaDWVNfPdm3PGg==", "7523bc3a-eeca-43f5-9cb6-fc38f6f4db7c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fdf2d046-52f8-46cf-a9fb-b7b02693d535", "AQAAAAIAAYagAAAAEDwp7ghwK/ZBPNAQ08a6VLG5Qx0zOM5L01/KnOaxG3yBUuRcGerioOmI+sj43xRt4w==", "08d04ab1-29ea-4a97-8bc7-a103ff58f7e2" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_IsActive",
                table: "UserDevices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId",
                table: "UserDevices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_UserDevices_FCMToken",
                table: "UserDevices",
                column: "FCMToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6209cea5-24d4-43ed-bc02-206d586ebb71", "AQAAAAIAAYagAAAAEFyId01Z8KOcL4yRH2hedYaTurZvxGV6s0LSHparASLC8YaaNl3Vu8xHSbczuErlLA==", "15394275-451b-40a0-a5d8-331792d0ed01" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "323dc14b-cb60-4652-b7aa-5ce5a9740eca", "AQAAAAIAAYagAAAAEEzpSQE5w+/R6dUKA9vmWrmdIVg8Q9qprz8Nb+T4eBzwNwsPKJI2e0bUuaLG+l+ufw==", "d5d4809d-7079-48e8-97c4-726a63abe2f4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bae913d6-e850-4b5d-8f51-7cb4d4bddb8a", "AQAAAAIAAYagAAAAECTLmxDyE7Fi1PJficaZLAgLlrAtb85tlv4MFiYrPbGulYbB2XH0R38EyF7+IiG+FQ==", "da0904f8-df67-45c4-bdce-420d2693122d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a538ee82-5095-483b-950b-496ef3d788a8", "AQAAAAIAAYagAAAAEAqYiiN5iIspHuLOvGsO86VsBl9H4vBitu00F9T8kZ9r54qT6fBwQ6FKJ1NzJwam1A==", "e8924353-860d-4349-9a06-8bd1fa064f5c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6837227e-52ed-44c1-bb0c-8e823569ba65", "AQAAAAIAAYagAAAAEJ5Alc8229ZUPrEL5AiGNo8ac+D59hoG/CYJhgW8BlHlPsiVs1kH2gYIc6Zlg5RF/g==", "9f14f8af-5dac-4173-9848-f5c6d94beb65" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "10fddb3d-4618-4d0b-9364-634dacfff570", "AQAAAAIAAYagAAAAEA28XwK2dXsFhCst8cifSQlcRNZOjoc0OhFltucr3TuEq+k9NwyYXu9FRMMB9+W0nA==", "de48b501-a17d-4e33-88e7-25a9460c0495" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "aaf85cde-ddcb-4b9d-94b7-4be0393ae737", "AQAAAAIAAYagAAAAED8gVf52aJeunG66wDJUVO+t3v3mwsp8nJM8UzyjO4keuctWFQ6oHlbm5g0G/08gdg==", "137a363c-d3e3-47d8-822a-f290714eb138" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a081189a-45df-483b-bca7-dc09963483ea", "AQAAAAIAAYagAAAAEMIiPp35/J6VRh6+nMEyDtbHs9PnVhLiWNqmbwkEUNXHf3p2+ie04ry9CcIy0iEZrA==", "75cae239-ca0c-4d61-ada4-3f891595151a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4b3eaf52-72db-4597-bb1d-f445ae1df7c6", "AQAAAAIAAYagAAAAEGVgHExfenXIg5xOJceGtU25j270NzV8vgGdIm4e9/HtRhLlYIQlr+9JWcp7t/G9hQ==", "d08a6f9e-7212-477b-b0fe-c7c1bdca53fa" });
        }
    }
}
