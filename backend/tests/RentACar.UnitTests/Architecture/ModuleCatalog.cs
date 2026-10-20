using System.Reflection;
using RentACar.Modules.Auth;
using RentACar.Modules.Branches;
using RentACar.Modules.Customers;
using RentACar.Modules.Fleet;
using RentACar.Modules.Pricing;
using RentACar.Modules.Reservations;
using RentACar.Modules.Simulation;

namespace RentACar.UnitTests.Architecture;

internal static class ModuleCatalog
{
    public static readonly Assembly[] Assemblies =
    [
        typeof(AuthModule).Assembly,
        typeof(BranchesModule).Assembly,
        typeof(FleetModule).Assembly,
        typeof(PricingModule).Assembly,
        typeof(CustomersModule).Assembly,
        typeof(ReservationsModule).Assembly,
        typeof(SimulationModule).Assembly,
    ];

    public static readonly HashSet<string> Names = Assemblies.Select(a => a.GetName().Name!).ToHashSet();

    public static Assembly ByName(string name) => Assemblies.Single(a => a.GetName().Name == name);
}
