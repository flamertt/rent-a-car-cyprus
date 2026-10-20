using RentACar.Api.OpenApi;
using RentACar.Modules.Auth;
using RentACar.Modules.Branches;
using RentACar.Modules.Customers;
using RentACar.Modules.Fleet;
using RentACar.Modules.Pricing;
using RentACar.Modules.Reservations;
using RentACar.Modules.Simulation;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure;
using RentACar.Shared.Infrastructure.Modules;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Modül listesi tek yerde: yeni modül = buraya bir satır + proje referansı.
IReadOnlyList<IModule> modules =
[
    new AuthModule(),
    new BranchesModule(),
    new FleetModule(),
    new PricingModule(),
    new CustomersModule(),
    new ReservationsModule(),
    new SimulationModule(),
];

builder.Services
    .AddSharedInfrastructure(builder.Configuration)
    .AddModules(builder.Configuration, modules)
    .AddOpenApi(o => o
        .AddDocumentTransformer<BearerSecuritySchemeTransformer>()
        .AddOperationTransformer<BearerSecurityRequirementTransformer>());

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(o => o
            .WithTitle("Mert Peker Rent a Car API")
            .AddPreferredSecuritySchemes(BearerSecuritySchemeTransformer.SchemeId))
        .AllowAnonymous();
}

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription().AllowAnonymous();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).ExcludeFromDescription().AllowAnonymous();
app.MapModules();

if (app.Environment.ShouldAutoMigrate(app.Configuration))
{
    await app.InitializeModulesAsync();
}

await app.RunAsync();

// WebApplicationFactory ile entegrasyon testleri için.
public partial class Program;
