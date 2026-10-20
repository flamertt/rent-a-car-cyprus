using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Modules.Fleet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ParkingSlotPerBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_vehicles_parking_slot",
                schema: "fleet",
                table: "vehicles");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_branch_id_parking_slot",
                schema: "fleet",
                table: "vehicles",
                columns: new[] { "branch_id", "parking_slot" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_vehicles_branch_id_parking_slot",
                schema: "fleet",
                table: "vehicles");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_parking_slot",
                schema: "fleet",
                table: "vehicles",
                column: "parking_slot",
                unique: true);
        }
    }
}
