using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyriuxFitnesApp.Migrations
{
    /// <inheritdoc />
    public partial class FixTrainerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainerService_Services_ServiceId",
                table: "TrainerService");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainerService_Trainers_TrainerId",
                table: "TrainerService");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainerService",
                table: "TrainerService");

            migrationBuilder.RenameTable(
                name: "TrainerService",
                newName: "TrainerServices");

            migrationBuilder.RenameIndex(
                name: "IX_TrainerService_ServiceId",
                table: "TrainerServices",
                newName: "IX_TrainerServices_ServiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainerServices",
                table: "TrainerServices",
                columns: new[] { "TrainerId", "ServiceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerServices_Services_ServiceId",
                table: "TrainerServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerServices_Trainers_TrainerId",
                table: "TrainerServices",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "TrainerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainerServices_Services_ServiceId",
                table: "TrainerServices");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainerServices_Trainers_TrainerId",
                table: "TrainerServices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainerServices",
                table: "TrainerServices");

            migrationBuilder.RenameTable(
                name: "TrainerServices",
                newName: "TrainerService");

            migrationBuilder.RenameIndex(
                name: "IX_TrainerServices_ServiceId",
                table: "TrainerService",
                newName: "IX_TrainerService_ServiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainerService",
                table: "TrainerService",
                columns: new[] { "TrainerId", "ServiceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerService_Services_ServiceId",
                table: "TrainerService",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerService_Trainers_TrainerId",
                table: "TrainerService",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "TrainerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
