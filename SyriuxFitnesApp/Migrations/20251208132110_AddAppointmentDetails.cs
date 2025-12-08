using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriuxFitnesApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoredDuration",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "StoredPrice",
                table: "Appointments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoredDuration",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "StoredPrice",
                table: "Appointments");
        }
    }
}
