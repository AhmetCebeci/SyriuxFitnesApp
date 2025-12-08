using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriuxFitnesApp.Migrations
{
    /// <inheritdoc />
    public partial class salonEklemesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Salons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ClosingTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Salons",
                columns: new[] { "Id", "ClosingTime", "OpeningTime", "SalonName" },
                values: new object[] { 1, new TimeSpan(0, 22, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), "Syriux Fitness" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Salons");
        }
    }
}
