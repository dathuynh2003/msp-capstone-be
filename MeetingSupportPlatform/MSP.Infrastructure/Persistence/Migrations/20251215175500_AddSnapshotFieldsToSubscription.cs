using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSnapshotFieldsToSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SnapshotLimitationsJson",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotPackageJson",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c356e9a3-a8b0-44e9-b374-1d3c2c024207", "AQAAAAIAAYagAAAAEMipzMt9i74zyKVAw9dfMasD0nGd+dD/2InKUthKaRQgUpaVW9hXYEpx0tIqjLMsVA==", "4b4a0180-163e-47a4-9ed0-1dd7f98ac3c7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3cd22763-dc94-47d0-a9f0-7b47e1b7ec1d", "AQAAAAIAAYagAAAAEAmk5xxfTchvxtlO8KHUNBldjKxBnNG41jafTmWM37o+QozO2/v3H6MWI0fz04P8Qw==", "06dfee73-a297-4047-851c-f630d631daeb" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bd0bd33b-dc1d-467a-a221-278634fde241", "AQAAAAIAAYagAAAAEMnBZQo00uMmiV2St6NHxLxEW0QO1G96g1dfsdeaWSaA+y9w9+FYmerRtWbGM5Frsw==", "545d4aa8-87d8-48d5-acb8-2eeba0bdc45e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7bed211a-811d-4007-9e85-6f588214ab90", "AQAAAAIAAYagAAAAEP7OSZGhk84vODtz90VpfXCY9W864eR0OvjjpycqYtTPKjmUW3ZtsOUpG3iNKuj7PQ==", "b745e1f4-bd39-4385-b68d-35e15e99e991" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8d28f2cb-c10f-41a6-bcfe-8bb32fb4bc85", "AQAAAAIAAYagAAAAEOzMNQ4IPhHyqOGIgGYNyiHlgtmmAHetszB/LBcmiZvsVbMfEfgMG8U4OMqo6sab6g==", "44b88387-00e0-4154-8c0c-2f8235081832" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "44268863-0850-4ab1-90cf-e05c6fe83c4d", "AQAAAAIAAYagAAAAEF+X61t1q0RbE0XtKtpXA/lYLFWFDjo8hnSP1kf9LFaWg7QMq2lLGbqy6Gb3L1rJmw==", "4d0e6fd2-f34d-4861-b7a8-c38a380bddf4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b7e29442-7f85-433c-ab54-b4061ea0158f", "AQAAAAIAAYagAAAAECE+b1hg2WueeCDw7FCtnURyGIz/DVTfW4rJ+ybDAbJWS4TzT3yuj1wudTcOl6F71Q==", "757e1ffa-4d99-41b1-9412-97c5e35a56aa" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "daeb4bee-9a29-4e37-8ffd-a5f2135eea66", "AQAAAAIAAYagAAAAEAF+Ud1/viJOwU8+uMV6LOatOpsFDeahdNomkvMDDJIr3om+67b68WGJDQ/n7TMKnA==", "618e7677-9771-4780-8385-a7ae2d0ce3dd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a73ff125-8335-4c6e-970d-02de09b2c2e1", "AQAAAAIAAYagAAAAEGVz3VVu4iuG+M5JoVpzJ+DdbnhTcvbHe01MsWtxhC5792LCfpaf3VshiUvbECSBjg==", "89e7015a-6884-407c-860d-fa60111fd92b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SnapshotLimitationsJson",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SnapshotPackageJson",
                table: "Subscriptions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "143cf9b0-4ce7-4f51-84f6-0febfbe1554e", "AQAAAAIAAYagAAAAEHB/e4cUcSndG4qgJKNHJ0YNaT0oWiNrPZcDcJBna5+2fQcoD6fnwgNFvJO6aNbHOA==", "09b46a22-7096-4bc4-85e7-347125a557fe" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fc9963c9-8fa4-4c86-8c22-44c7bd60ccdf", "AQAAAAIAAYagAAAAEEzd7WdCVPWIB44EwbfOGViy2MDdS9rt4lKE5zZeByAVlSIp7OpDof+GIc02fDPahA==", "e4b929ec-a8fb-4d28-b6fb-0da3a35be22c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a9e0e473-488b-411f-841a-baeb896c7081", "AQAAAAIAAYagAAAAEOfpmGeZL3lEQk+GWm/cbCdhI8jd4RKFT6D1EJNxqCgFPfWWih7MsYMqIAuL72OIbA==", "ca882193-559c-4188-9181-4ec4b8b987c9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7297d72e-8519-48f4-9fbd-b4f1f5e42d81", "AQAAAAIAAYagAAAAEOS2LHh4OJcIPlIJmxkNcjxI6xWafOQiBmvthTZAuctguf2F1ODq1IKo+a6eB8kehQ==", "82030b1b-2429-4779-81ca-3aa006b5386b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8d8e9219-c860-45a8-b890-6bd29e7d4f71", "AQAAAAIAAYagAAAAELUk7TBL9JutAQlfpM8dxe4kuJqBOIiHcuzi9/1iCJmflfMqXEnRFNkFAZBusqmBNw==", "64510fab-bfa8-4ea5-b134-ae538969725b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4958f269-f48e-42cc-b506-9d3405a50e16", "AQAAAAIAAYagAAAAEOwAbtTF3IPmHnMf9pkzIEqk7TvZ2s5HIAwRi+JU2toglejr8drROJsfVTkn42FTuw==", "11b4b5f8-ba8d-480a-9dae-13539fdf02d4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e20d7687-6974-478c-b145-4537650da971", "AQAAAAIAAYagAAAAEDDyMPmN13nwdHWpFEXStwQVfs7iPB2tLE0LwiDJLnYsC1GvL4K4Bc6+S0RjJrsjyw==", "5ff1c7b5-3be9-43b9-90a2-9376fcd81487" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c09a45cf-870b-442d-a3a1-b9f7ebad4c70", "AQAAAAIAAYagAAAAEJjus1WPBjpcazujdcbF+gmR8cBRCFYSU8kh3C3q4skNnvcwzFQBZ8w1I8sowcL/FQ==", "5a7fc76b-a0d9-4366-a992-129e1ace167c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "71d5e7b7-6f78-42be-845e-f5212ae45f45", "AQAAAAIAAYagAAAAEBFgem1IMpoj8tIE1Pj6IsORH+877RqiLCVrqUUNbWhZD9xWPsPxPg55fTkVMN2fVQ==", "5ede1b27-3041-427d-8a69-8ec28304d1e9" });
        }
    }
}
