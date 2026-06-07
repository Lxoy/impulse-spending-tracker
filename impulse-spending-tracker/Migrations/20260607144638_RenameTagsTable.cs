using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace impulse_spending_tracker.Migrations
{
    /// <inheritdoc />
    public partial class RenameTagsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Tags_TriggerTypeId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseTags_Tags_TagsId",
                table: "PurchaseTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.RenameColumn(
                name: "TagsId",
                table: "PurchaseTags",
                newName: "TriggerTypesId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseTags_TagsId",
                table: "PurchaseTags",
                newName: "IX_PurchaseTags_TriggerTypesId");

            migrationBuilder.CreateTable(
                name: "TriggerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ColorHex = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggerTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_TriggerTypes_TriggerTypeId",
                table: "Purchases",
                column: "TriggerTypeId",
                principalTable: "TriggerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseTags_TriggerTypes_TriggerTypesId",
                table: "PurchaseTags",
                column: "TriggerTypesId",
                principalTable: "TriggerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_TriggerTypes_TriggerTypeId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseTags_TriggerTypes_TriggerTypesId",
                table: "PurchaseTags");

            migrationBuilder.DropTable(
                name: "TriggerTypes");

            migrationBuilder.RenameColumn(
                name: "TriggerTypesId",
                table: "PurchaseTags",
                newName: "TagsId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseTags_TriggerTypesId",
                table: "PurchaseTags",
                newName: "IX_PurchaseTags_TagsId");

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ColorHex = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Tags_TriggerTypeId",
                table: "Purchases",
                column: "TriggerTypeId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseTags_Tags_TagsId",
                table: "PurchaseTags",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
