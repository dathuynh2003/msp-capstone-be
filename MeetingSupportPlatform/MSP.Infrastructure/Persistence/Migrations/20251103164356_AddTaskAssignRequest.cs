using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAssignRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskReassignRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskReassignRequests");

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
        }
    }
}
