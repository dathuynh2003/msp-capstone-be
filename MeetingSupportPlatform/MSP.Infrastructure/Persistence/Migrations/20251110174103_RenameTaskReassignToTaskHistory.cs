using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameTaskReassignToTaskHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskReassignRequests");

            migrationBuilder.CreateTable(
                name: "TaskHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskHistories_AspNetUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskHistories_AspNetUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskHistories_ProjectTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "ProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistories_FromUserId",
                table: "TaskHistories",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistories_TaskId",
                table: "TaskHistories",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistories_ToUserId",
                table: "TaskHistories",
                column: "ToUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskHistories");

            migrationBuilder.CreateTable(
                name: "TaskReassignRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseMessage = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReassignRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReassignRequests_AspNetUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskReassignRequests_AspNetUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskReassignRequests_ProjectTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "ProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_TaskReassignRequests_FromUserId",
                table: "TaskReassignRequests",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReassignRequests_TaskId",
                table: "TaskReassignRequests",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReassignRequests_ToUserId",
                table: "TaskReassignRequests",
                column: "ToUserId");
        }
    }
}
