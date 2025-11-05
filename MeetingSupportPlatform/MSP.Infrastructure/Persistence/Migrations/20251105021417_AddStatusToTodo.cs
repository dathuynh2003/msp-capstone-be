using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToTodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Todos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "02c58386-d942-44c2-83e4-97a3c3394983", "AQAAAAIAAYagAAAAEJOYYhJ3CNKbQkwGyhokQr2TIQD2qiUt68CyWBYE9k+8PzLfwWz781+OY3XmRXv8jA==", "3e7c1c2d-799a-44a5-9a6e-c0ead1dc3df8" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "79ed22b3-20da-4a6c-aea8-824cf7a9ca43", "AQAAAAIAAYagAAAAEHTQOGUIBxcGZ1S217+RIZHwCrwHRrUqTpjIBV595fegNvEn2INzMYywZDd77Kf8vQ==", "1c53c29f-0c84-4708-a101-1bdcdae526da" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4f7c633a-2566-4d15-a790-9859c4885fbc", "AQAAAAIAAYagAAAAEADIaJFPBJrXZ1aMxj+fOzX1X5UodscC3ZNHcrFxlcmAnxOAQ87gzh8IyuorthjaFQ==", "d2f55af0-63a1-4da6-86d2-e93fe5241c6d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "023a38fb-d056-44ce-932c-ae48c02d1920", "AQAAAAIAAYagAAAAEOYwW6z1CIdALtMo4eY/+X9lDl39aEhxv3ceVzVsE1pC8dqOdFR4+p4X/NHSNQDGFA==", "472bd3f4-cdc3-41df-a750-dd49be2e90eb" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6b85ec49-e16e-4933-8f56-2299981b0e33", "AQAAAAIAAYagAAAAEHDWd+XI6jCopT3rWid7qsjroCdad26giSXxsfdpAohDJEIeT4NNN9c8+Nos76iaOw==", "83895dec-c0f3-4690-9b1f-4770b7434a5c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "93556e36-b5d6-4c80-9164-ce16f2da1113", "AQAAAAIAAYagAAAAECH2G6smCdBcZ/AkUfvJzHOZ9SuowK0TIC+FDWaCPf65ouCqnHWxeXQGOZ8Hae/1Uw==", "08460551-58f6-4658-b7d7-effd3a05a242" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d5207be6-302c-42b3-bbcf-535292b5414c", "AQAAAAIAAYagAAAAEMRhGopJfJ3VfoBayoonhyZAOparu7CZ95AD728dHxqedEaQM8+NSBBAm+P7v8taag==", "f8245348-2575-4f1e-9066-bfbb4ac0166c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "41dc357e-1c47-4690-b8f9-9fefbbac240c", "AQAAAAIAAYagAAAAEADlvf74UIVHWPq26uSzPugedzMEB2mQFoLQwyxANYu4HslvnumI14yHIgyPN6dWDQ==", "ca289fba-c813-4a21-acaf-cacd1efaa7a9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "361518f5-29b1-4529-b618-ff43abea5e2f", "AQAAAAIAAYagAAAAELuz2CD4SN5tWn5+roWbdMUSJG0csitCm0HBHZLmRCsDWGU124OC9w1zAsvaSKtRJQ==", "3c568207-465d-4b86-b3f9-ccd6e5543d57" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Todos");

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
        }
    }
}
