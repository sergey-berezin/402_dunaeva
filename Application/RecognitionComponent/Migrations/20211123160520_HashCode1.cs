using Microsoft.EntityFrameworkCore.Migrations;

namespace RecognitionComponent.Migrations
{
    public partial class HashCode1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HashCode",
                table: "ImageEntities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashCode",
                table: "ImageEntities");
        }
    }
}
