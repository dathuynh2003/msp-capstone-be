using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessOwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationInvitations_AspNetUsers_BusinessOwnerId",
                        column: x => x.BusinessOwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationInvitations_AspNetUsers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "db534b55-fd97-45f4-bd74-789e25511cd9", "AQAAAAIAAYagAAAAECrA3BOZYcl99Z2J39OL/ra/WZJNb7K7drFapOQYo0AejU9PX7H2OVOTZhij39Eu4Q==", "3516f4f7-a6a2-4523-b4c8-c6ffe2af4e95" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f137afa1-4efb-45b2-ac7e-415075f45ddc", "AQAAAAIAAYagAAAAEO7PKUuk/VhRghafrxegIa/W1E4oGrJa2TZiZfXgzM4bjMMGPyIqqaD77tfUfrHHyg==", "cf483dd8-b600-4e12-b6d8-1c651019a981" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a198e953-4c26-4250-82b2-c0b36da71f5f", "AQAAAAIAAYagAAAAEPqN4xLFnZi5weJREwfyAn55KSN1gMygd7Z6/vr4IsTnGeMwrKGtZTZE/PvCcZSKxA==", "de944b84-baa1-4392-9523-7419101eb27d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a2d0e35a-7850-4920-80b9-27e8c5931f47", "AQAAAAIAAYagAAAAEOL7UMtzYAwJRN/H3Z27+wpq14RcFESj+wZ+LrQJ9uJ15WlbGlVHauZbDbkpvMyzRQ==", "91e2cad6-c450-454b-8173-c837b3b5f0cd" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_BusinessOwnerId_Status",
                table: "OrganizationInvitations",
                columns: new[] { "BusinessOwnerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_MemberId_Status",
                table: "OrganizationInvitations",
                columns: new[] { "MemberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_Type",
                table: "OrganizationInvitations",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationInvitations");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6eb81eb8-a4ab-4ffd-a979-5369495acc3a", "AQAAAAIAAYagAAAAEPeM1+LUlSP+EWEank2y4WKT1O8Cqy6J33kUozLEvVW29wNLsz7ZmsaW2y9UqwnP2g==", "3e2ebe62-3d43-4a2b-9060-72f56f03d9a6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7910f51a-b1f4-48d6-905e-de68f1bbcbc1", "AQAAAAIAAYagAAAAEHb7veoDvzikjRNlmRyp+XKz6Wt7LfmZ8S9RoP/K8rAaoh18MTx5UMyX+d7/ynx/aw==", "dd7a710c-17ce-4e87-aeb8-70c5fe1e0735" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8a198368-4c87-4eed-a836-2e1233510f35", "AQAAAAIAAYagAAAAEK/7m4m/VtmOHdzI1EBJq0YfnpEhuf9splfNNP4sC9qvWNdbPSzZReGdJQGF7/FmQA==", "bb1f6b4d-1636-44ae-86d0-98b95345debb" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e57f754d-014b-4b1c-9a58-52e82eb586ba", "AQAAAAIAAYagAAAAEAllkBk5bjQ0rez8U1DYYep2jJLXwIKfIEmzTrH31FoVebAsuGdEjAxg2gVjfo/JTA==", "b78b8580-e089-4a6f-848a-72b749686f62" });
        }
    }
}
