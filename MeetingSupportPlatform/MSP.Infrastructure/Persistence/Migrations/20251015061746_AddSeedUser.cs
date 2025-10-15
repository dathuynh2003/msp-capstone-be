using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f02214d6-742c-4a76-8fa1-1e5e7fdbade5", "AQAAAAIAAYagAAAAEGrMEfsciNDniuUB6hka6lDPXbUrBgi99hWs5PdOdePWQgWebfZROIrtqHXBKf0RDQ==", "e5740e10-ecbd-48c4-a049-765960931eb5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e02db42d-503f-4092-8047-791d9fbe052b", "AQAAAAIAAYagAAAAEDQiML+WhusstqXcQMJNmYayV8rVw1zDiA2rHMca6z4Cn6dqN6KSld+GwpSj24eBgg==", "038d7f5c-690c-49a7-9dcf-72a3e8b9afc0" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ae78eadf-2b70-4dd4-8d2c-dc836b07ca85", "AQAAAAIAAYagAAAAEEmISFVHNiGBdaCb+wJ64Bk6gGvTtIJ77xjfB/941TRe2Hn/n3s0mwxdbmqP1l5zpg==", "21fadaf3-98e2-4b9a-80c8-fb1c2fb54944" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "IsApproved", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f12528ae-2a29-41df-a944-98c4c41bc902", true, "AQAAAAIAAYagAAAAEIRvKVCI/w21IszVYMguOXXOIziLOOdnFIEypr+UqwayOi7TJzPBlBIKFHCyBHi2jA==", "dcd78a6e-bd54-4728-aa5f-a87d685b83db" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f569a62a-4787-4a6a-98ec-c40131f23484", "AQAAAAIAAYagAAAAEEPyMBm10St/WgfMPIWV/Qhbh8sF3l3ltdCl2eo3CDJqCc0nMl7WL3xPTAw5NJ+oBw==", "912a1d6d-4140-41ac-8200-8f5fa70706ce" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3f613ce8-be77-4b6e-b95c-83a80192fa85", "AQAAAAIAAYagAAAAEB7snfo1yy6a9YYd+XqjQ2bPke9s/bgpYNA7nqpTFP72cKpT4bJAR2Ga7BchISzEJQ==", "50237393-6c57-408f-8b22-939f4dde575d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b7bc9781-9a55-41b9-a798-dc306234201c", "AQAAAAIAAYagAAAAEBW9N7aPu4pBJhsEGY2VsiZMKnWPK5Ag/iTo860cZZ8HfmhZhnoexbC1yTEVGzub1Q==", "39672d68-d085-41f7-b077-aeaa1fc11b7e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "IsApproved", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b99ace0b-40c1-4411-9d7b-72d993541d58", false, "AQAAAAIAAYagAAAAEGrtYnk65npo3hVAOu9shWi4rttD8QhuFhKZpOQxVYQGPUjpPEvWwUbs3hNy0BO0Dg==", "b3316ae8-d2ba-4e91-951d-2d5dda50f3ea" });
        }
    }
}
