using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeftAtToProjectMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LeftAt",
                table: "ProjectMembers",
                type: "timestamp with time zone",
                nullable: true);

            // Các lệnh update data cho AspNetUsers (tùy context, có thể giữ lại hoặc xóa nếu không dùng)
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b1f15491-068b-4e9d-896e-1e7c0aa48d08", "AQAAAAIAAYagAAAAEDZKgkPFQds3PUaVtSslye7zQ+5ecyNqotlG0YT96C3tcsIR1XfHSLcFLQGiNDP/kQ==", "2d684221-47e9-4990-8059-8b9f15f3914e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8df1c91a-9c42-4890-a10b-d472f5b5a0d2", "AQAAAAIAAYagAAAAEJvP8+lSWu35KTtb/qCSH5TLBsRwGJFJdc4/PwDSz5p9wCkJ2WtOfrh2ePEmA+4gSw==", "b90c8663-f3bc-42ca-a722-fb8950210319" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ab104e80-08f5-4300-875f-e9738156f588", "AQAAAAIAAYagAAAAELtyTiFmiRjPO8QgtmhqOjrnlXe4fRksqKEsqtYJFgNaDNu4z+L1wzyqe9T5Nm5s9w==", "d04bc051-3345-4532-9024-ad4236a0c8ec" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "94cf52e0-247a-4e6b-b52d-0cae1220a5ad", "AQAAAAIAAYagAAAAEMkJG0p9RUEJN3zvqsHhsF4YIg1Fh5E0DgoIkDU3ha/pF1Ds1tKIBVLLt++xh4ac/Q==", "1ec65ad9-061c-4f4d-84da-4f92c674f908" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeftAt",
                table: "ProjectMembers");

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
