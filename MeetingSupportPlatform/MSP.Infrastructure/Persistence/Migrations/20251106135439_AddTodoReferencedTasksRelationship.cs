using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoReferencedTasksRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectTaskTodo",
                columns: table => new
                {
                    ReferencedTasksId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferencingTodosId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTaskTodo", x => new { x.ReferencedTasksId, x.ReferencingTodosId });
                    table.ForeignKey(
                        name: "FK_ProjectTaskTodo_ProjectTasks_ReferencedTasksId",
                        column: x => x.ReferencedTasksId,
                        principalTable: "ProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTaskTodo_Todos_ReferencingTodosId",
                        column: x => x.ReferencingTodosId,
                        principalTable: "Todos",
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
                name: "IX_ProjectTaskTodo_ReferencingTodosId",
                table: "ProjectTaskTodo",
                column: "ReferencingTodosId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTaskTodo");

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
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7fb9af59-1584-4f86-85ff-ef618d103620", "AQAAAAIAAYagAAAAENFQmWvNRIX0J9ty76y7v6AM8/jVbHZclNQ1cYWaX1dtDgFj9boQxvV14PjYUg9RRg==", "f57db8dd-3c47-4d90-8a0f-d7004ebecf72" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "39472ae8-2b66-40af-be83-7df98325a42e", "AQAAAAIAAYagAAAAEJXihz5kSeq+dHsGoqXmaY7/PdZTFmohbO7YuVG1anCjj09pbf223iDjOmN26gLbxw==", "34779c97-48b6-47e2-b962-c2ac6407fa46" });
        }
    }
}
