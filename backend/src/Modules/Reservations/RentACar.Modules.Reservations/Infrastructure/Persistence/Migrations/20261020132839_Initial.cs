using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Modules.Reservations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reservations");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(130)", maxLength: 130, nullable: false),
                    customer_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_plate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    pickup_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    return_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pickup_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    return_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    blocked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    picked_up_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    returned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    actual_return_branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    start_odometer_km = table.Column<int>(type: "integer", nullable: true),
                    end_odometer_km = table.Column<int>(type: "integer", nullable: true),
                    start_fuel_level = table.Column<double>(type: "double precision", nullable: true),
                    end_fuel_level = table.Column<double>(type: "double precision", nullable: true),
                    days = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    lines = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reservations_code",
                schema: "reservations",
                table: "reservations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reservations_customer_id",
                schema: "reservations",
                table: "reservations",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_status_pickup_at",
                schema: "reservations",
                table: "reservations",
                columns: new[] { "status", "pickup_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reservations_status_return_at",
                schema: "reservations",
                table: "reservations",
                columns: new[] { "status", "return_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reservations_vehicle_id",
                schema: "reservations",
                table: "reservations",
                column: "vehicle_id");

            // Aynı araca zaman aralığı örtüşen iki onaylı/aktif rezervasyon olamaz (yarış durumunda da).
            // EF Core exclusion constraint modellemediği için elle; ad ReservationsDbContext.NoOverlapConstraint ile aynı.
            migrationBuilder.Sql("""
                ALTER TABLE reservations.reservations
                ADD CONSTRAINT ex_reservations_vehicle_no_overlap
                EXCLUDE USING gist (vehicle_id WITH =, tstzrange(pickup_at, blocked_until, '[)') WITH &&)
                WHERE (status IN ('Confirmed', 'Active'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE reservations.reservations DROP CONSTRAINT IF EXISTS ex_reservations_vehicle_no_overlap;");

            migrationBuilder.DropTable(
                name: "reservations",
                schema: "reservations");
        }
    }
}
