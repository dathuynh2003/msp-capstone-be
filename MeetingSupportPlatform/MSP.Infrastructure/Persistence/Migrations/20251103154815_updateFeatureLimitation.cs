using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateFeatureLimitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageFeatures");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.CreateTable(
                name: "Limitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsUnlimited = table.Column<bool>(type: "boolean", nullable: false),
                    LimitValue = table.Column<int>(type: "integer", nullable: true),
                    LimitUnit = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageLimitations",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    LimitationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageLimitations", x => new { x.PackageId, x.LimitationId });
                    table.ForeignKey(
                        name: "FK_PackageLimitations_Limitations_LimitationId",
                        column: x => x.LimitationId,
                        principalTable: "Limitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageLimitations_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "45ad9ff3-e3fc-4972-a397-f7c7251abefb", "AQAAAAIAAYagAAAAEFBxddc/NGJBhOsRZgskgpAq4LbpnQkvsXm88IqPns5oY94TYMeeGf7KvBjTeHRuSg==", "0a1d6df1-2c8e-471e-a846-fbbde679e61f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "49012698-4daa-41d7-9dd0-eb6b487b4875", "AQAAAAIAAYagAAAAEGlG2wB3ZiWGjwcA5UE5JBJn0B2HijzUjlIFUoA2+zfY01A8u2/hf8Wgyf6w99gDEA==", "25ed74ba-858d-4a08-b4ba-2cd37368d89b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8678bbd3-7d00-498b-b408-02b4e4d93818", "AQAAAAIAAYagAAAAEKYmA6Ca3FzGAIWqgeBXxALgY/hUGZr0ZmSbEP7nInSDBPsr93BqXU8dGOJ/3JD4CA==", "edb203e5-543e-40e4-b273-35f1929278ec" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0c2adcf4-a5f0-4b72-affe-e8cbed007b0c", "AQAAAAIAAYagAAAAEC6Xe/O83IV3DHtsTU79NOSquslq4W4egIHnS86KEBcUVgMXwxMcuv9pB1nNWuVgxQ==", "c77a87e6-c1de-4cb3-bd5b-508c954400d2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5b069eae-ade0-4fdd-97e1-1447988d6f32", "AQAAAAIAAYagAAAAEM7z7MCXBPHOA0fApasw9Xqupl9MWkwKj/oW2bRbEoUwnqHvpWqrhe+IkXgEZo/ilQ==", "f5cbb2db-88da-47b3-a5a3-e87d73a9f34d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c3d02f6b-0da5-4a21-8d73-4e03a06a852c", "AQAAAAIAAYagAAAAEDeNZ1i1rkS9Sks8INOulSe3WzNb17jt59lWARGFWp1hRQ/0YHtADWmRGuG/9KmVnA==", "dc38aa00-b206-410e-8617-6ff0c912e47c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "94bbb440-075e-4ed0-a020-932f4b34ec6c", "AQAAAAIAAYagAAAAEJA8JRmul3bJ9R2Rxm+45RKJsfScSflHOPc4F1qA9G3uBb2Iru9jXO2lMCaJ2szGVQ==", "04aa4abb-8e97-41ff-91d0-d9bcf1a03a2d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4d0aa7d9-903b-411b-ba16-3f60cc1b4799", "AQAAAAIAAYagAAAAEF+JgiFtg8mwoc6Lr6EZiOyPVMlX3xVgzYiFC+PjS29Zzf8Va4BMOE48fxZqiv5Kyg==", "730ba803-3a76-49cc-b6b6-5405edc393f9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2019436f-49e2-479b-83cc-3df56232ab2e", "AQAAAAIAAYagAAAAEHmCPAldaLTRE9KXG/F8PJfO96kEvNjgd1MRpqUoJJFqKLXvpgRPffizFxa4B2LPaQ==", "bb4e138e-640a-4744-b29f-078227dcdbc5" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageLimitations_LimitationId",
                table: "PackageLimitations",
                column: "LimitationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageLimitations");

            migrationBuilder.DropTable(
                name: "Limitations");

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsUnlimited = table.Column<bool>(type: "boolean", nullable: false),
                    LimitUnit = table.Column<string>(type: "text", nullable: true),
                    LimitValue = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageFeatures",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageFeatures", x => new { x.PackageId, x.FeatureId });
                    table.ForeignKey(
                        name: "FK_PackageFeatures_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageFeatures_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "063a84f9-facf-4c32-86de-1974ad9daaa1", "AQAAAAIAAYagAAAAEIjOiFBt6msS43Mew1YqtiyhcxJDgX+9IQg9coH75JBNwKAeUx0k/F73DY/e8X1IdQ==", "2781feb1-8736-4808-9cc6-bce628c56a09" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "82a50549-6f7f-4ce4-a0b4-7b7b2358c248", "AQAAAAIAAYagAAAAECtgR2Yoz/LyjJ0NZpzVU/7f4f/ZOR95jD6Ri2QPE9dFLYsgsadXV2KTweZKLdnbzw==", "8bcbfc61-3373-46c7-8063-505a419600d9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "884508e8-d3b6-452b-968a-a5dc3c9fdd12", "AQAAAAIAAYagAAAAEKe5eooKnng2HUR6AKwWcci/0LZeKYUm2IFbFhbMO39T0b2MFzcrPmWSNPDd+J+dmg==", "47207a18-b468-4282-9775-7b28ebc12ad3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dd527856-e4c8-461d-99d0-13841242929f", "AQAAAAIAAYagAAAAEEBMUuHshGNCEaoE+x70bfmf5jd7XPrwrxwJjnqvgHOCV8sXeusgTP0K8PxEZIuUgQ==", "6b2c9540-6c4d-4cd2-b846-51ca6dca92fc" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "64f6fd4d-8939-44f2-b325-f3d7ac071320", "AQAAAAIAAYagAAAAEEZi99ZdTaw0AXsa4kqQDae7DbfdzaoqLM3/L+iVVwXj/afmhJ+jI+4QEQtKxtVd3g==", "30757fd6-25c3-4272-a85d-599318ec19ad" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d798eee3-002a-44e7-827c-eb46f3769af1", "AQAAAAIAAYagAAAAEOW2BWouD45+L4X/RqBSuJ0ZIbaQwRXB60Uv16yrVqIkW8BRLaeDfYDTkvFz/dQpcw==", "40da04c2-414c-4c33-b7e1-e2f3c76ee286" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b7ddc14c-1d40-437d-89b8-90c0e505479f", "AQAAAAIAAYagAAAAEB5ngMxGuE34kOIrrfgZKU1Rxcvjlf6ChHR6u9Vgj64JRPOqn+9nfNTCb0yLg0fUkw==", "718577a8-a6b6-4436-b346-83c1b4471fb6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a404ef19-c6e8-4be3-9084-3e898674410b", "AQAAAAIAAYagAAAAEDi5aOMpNZu6j2q61zzWyAswAFqPeD23Ncsqni//Ph94PixJLAS1C0el3vb9E8kV5g==", "c99424da-b08d-40ee-82eb-2276bed32bc8" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "291bbf80-71fd-4564-90b8-bb82c7d82707", "AQAAAAIAAYagAAAAECH+N3NUqbDoN/pZ3Ir9DeX89e0iUag/pYtsfOOzeEQe3zqvgmb+XZiOghJgj8DulA==", "ab1299e0-22fb-4e45-823c-c752a46e7a5a" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageFeatures_FeatureId",
                table: "PackageFeatures",
                column: "FeatureId");
        }
    }
}
