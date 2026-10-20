using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RentACar.Modules.Simulation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "simulation");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "events",
                schema: "simulation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reservation_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "routes",
                schema: "simulation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    provider = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    path = table.Column<LineString>(type: "geography (linestring, 4326)", nullable: false),
                    segment_durations = table.Column<double[]>(type: "double precision[]", nullable: false),
                    segment_distances = table.Column<double[]>(type: "double precision[]", nullable: false),
                    distance_meters = table.Column<double>(type: "double precision", nullable: false),
                    duration_seconds = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_routes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "state",
                schema: "simulation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    running = table.Column<bool>(type: "boolean", nullable: false),
                    speed = table.Column<double>(type: "double precision", nullable: false),
                    sim_now = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_demand_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reservations_created = table.Column<int>(type: "integer", nullable: false),
                    lost_demand = table.Column<int>(type: "integer", nullable: false),
                    pickups = table.Column<int>(type: "integer", nullable: false),
                    returns = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                schema: "simulation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    state = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reservation_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    customer_name = table.Column<string>(type: "character varying(130)", maxLength: 130, nullable: true),
                    target_branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    return_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    stay_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    stay_lon = table.Column<double>(type: "double precision", nullable: true),
                    stay_lat = table.Column<double>(type: "double precision", nullable: true),
                    purpose = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    destination_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    from_lon = table.Column<double>(type: "double precision", nullable: false),
                    from_lat = table.Column<double>(type: "double precision", nullable: false),
                    to_lon = table.Column<double>(type: "double precision", nullable: false),
                    to_lat = table.Column<double>(type: "double precision", nullable: false),
                    depart_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: true),
                    leg_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    leg_duration_seconds = table.Column<double>(type: "double precision", nullable: false),
                    leg_distance_meters = table.Column<double>(type: "double precision", nullable: false),
                    last_attraction = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    odometer_km = table.Column<double>(type: "double precision", nullable: false),
                    fuel_level = table.Column<double>(type: "double precision", nullable: false),
                    start_fuel_level = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trips", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_events_at",
                schema: "simulation",
                table: "events",
                column: "at");

            migrationBuilder.CreateIndex(
                name: "ix_routes_key",
                schema: "simulation",
                table: "routes",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trips_reservation_id",
                schema: "simulation",
                table: "trips",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_state",
                schema: "simulation",
                table: "trips",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_trips_vehicle_id_state",
                schema: "simulation",
                table: "trips",
                columns: new[] { "vehicle_id", "state" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events",
                schema: "simulation");

            migrationBuilder.DropTable(
                name: "routes",
                schema: "simulation");

            migrationBuilder.DropTable(
                name: "state",
                schema: "simulation");

            migrationBuilder.DropTable(
                name: "trips",
                schema: "simulation");
        }
    }
}
