using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriuxFitnesApp.Migrations
{
    /// <inheritdoc />
    public partial class TrainerSaatTipiDuzelt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Önce sorun çıkaran eski int sütunları siliyoruz
            migrationBuilder.DropColumn(
                name: "WorkStartHour",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "WorkEndHour", // Eğer modelinde bu da varsa
                table: "Trainers");

            // 2. Şimdi Time (TimeSpan) tipinde sıfırdan ekliyoruz
            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkStartHour",
                table: "Trainers",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(9, 0, 0)); // Varsayılan 09:00

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkEndHour",
                table: "Trainers",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(18, 0, 0)); // Varsayılan 18:00
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WorkStartHour",
                table: "Trainers",
                type: "int",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<int>(
                name: "WorkEndHour",
                table: "Trainers",
                type: "int",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");
        }
    }
}
