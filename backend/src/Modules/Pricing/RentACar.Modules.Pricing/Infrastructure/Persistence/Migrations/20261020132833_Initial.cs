using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Modules.Pricing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pricing");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "duration_discounts",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_days = table.Column<int>(type: "integer", nullable: false),
                    percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_duration_discounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "extras",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    max_quantity = table.Column<int>(type: "integer", nullable: false),
                    max_charge_days = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_extras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "policy",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    one_way_fee = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    out_of_hours_fee = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    young_driver_max_age = table.Column<int>(type: "integer", nullable: false),
                    young_driver_daily_fee = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    grace_minutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rate_plans",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    daily_rate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rate_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "seasons",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    start_month = table.Column<int>(type: "integer", nullable: false),
                    start_day = table.Column<int>(type: "integer", nullable: false),
                    end_month = table.Column<int>(type: "integer", nullable: false),
                    end_day = table.Column<int>(type: "integer", nullable: false),
                    multiplier = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seasons", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_duration_discounts_min_days",
                schema: "pricing",
                table: "duration_discounts",
                column: "min_days",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_extras_code",
                schema: "pricing",
                table: "extras",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rate_plans_category_id",
                schema: "pricing",
                table: "rate_plans",
                column: "category_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "duration_discounts",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "extras",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "policy",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "rate_plans",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "seasons",
                schema: "pricing");
        }
    }
}
