using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriuxFitnesApp.Migrations
{
    /// <inheritdoc />
    public partial class TrainerCalismaSaatGuncellemesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkEndHour",
                table: "Trainers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkStartHour",
                table: "Trainers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkEndHour",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "WorkStartHour",
                table: "Trainers");
        }
    }
}
