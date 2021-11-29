using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RecognitionComponent.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageEntities",
                columns: table => new
                {
                    ImageEntityId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Image = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageEntities", x => x.ImageEntityId);
                });

            migrationBuilder.CreateTable(
                name: "ResultEntities",
                columns: table => new
                {
                    ResultEntityId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(type: "TEXT", nullable: true),
                    Confidence = table.Column<float>(type: "REAL", nullable: false),
                    ImageEntityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultEntities", x => x.ResultEntityId);
                    table.ForeignKey(
                        name: "FK_ResultEntities_ImageEntities_ImageEntityId",
                        column: x => x.ImageEntityId,
                        principalTable: "ImageEntities",
                        principalColumn: "ImageEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BBoxes",
                columns: table => new
                {
                    BBoxId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    X1 = table.Column<float>(type: "REAL", nullable: false),
                    Y1 = table.Column<float>(type: "REAL", nullable: false),
                    X2 = table.Column<float>(type: "REAL", nullable: false),
                    Y2 = table.Column<float>(type: "REAL", nullable: false),
                    ResultEntityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BBoxes", x => x.BBoxId);
                    table.ForeignKey(
                        name: "FK_BBoxes_ResultEntities_ResultEntityId",
                        column: x => x.ResultEntityId,
                        principalTable: "ResultEntities",
                        principalColumn: "ResultEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BBoxes_ResultEntityId",
                table: "BBoxes",
                column: "ResultEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultEntities_ImageEntityId",
                table: "ResultEntities",
                column: "ImageEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BBoxes");

            migrationBuilder.DropTable(
                name: "ResultEntities");

            migrationBuilder.DropTable(
                name: "ImageEntities");
        }
    }
}
