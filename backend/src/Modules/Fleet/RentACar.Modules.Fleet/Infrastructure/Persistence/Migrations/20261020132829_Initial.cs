using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RentACar.Modules.Fleet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fleet");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "fleet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    example_models = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    body = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    transmission = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    fuel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    seats = table.Column<int>(type: "integer", nullable: false),
                    doors = table.Column<int>(type: "integer", nullable: false),
                    bags = table.Column<int>(type: "integer", nullable: false),
                    min_driver_age = table.Column<int>(type: "integer", nullable: false),
                    deposit = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                schema: "fleet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    make = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    model = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    color = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position = table.Column<Point>(type: "geography (point, 4326)", nullable: false),
                    heading_deg = table.Column<double>(type: "double precision", nullable: false),
                    speed_kmh = table.Column<double>(type: "double precision", nullable: false),
                    odometer_km = table.Column<int>(type: "integer", nullable: false),
                    fuel_level = table.Column<double>(type: "double precision", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    parking_slot = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicles_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "fleet",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_code",
                schema: "fleet",
                table: "categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_category_id_status",
                schema: "fleet",
                table: "vehicles",
                columns: new[] { "category_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_parking_slot",
                schema: "fleet",
                table: "vehicles",
                column: "parking_slot",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_plate",
                schema: "fleet",
                table: "vehicles",
                column: "plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_position",
                schema: "fleet",
                table: "vehicles",
                column: "position")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicles",
                schema: "fleet");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "fleet");
        }
    }
}
