using System.Reflection;
using RentACar.Shared.Abstractions.Modules;

namespace RentACar.UnitTests.Architecture;

/// <summary>
/// Modular monolith sınırları: modül birbirinin içine (Domain/Application/Infrastructure) erişemez, yalnız
/// Contracts'a başvurur; dışarıya tek public tipi IModule sınıfıdır.
/// </summary>
public sealed class ModuleBoundaryTests
{
    public static TheoryData<string> Modules => new(ModuleCatalog.Names);

    [Theory]
    [MemberData(nameof(Modules))]
    public void Module_references_other_modules_only_through_contracts(string assemblyName)
    {
        var forbidden = ModuleCatalog.ByName(assemblyName).GetReferencedAssemblies()
            .Select(r => r.Name!)
            .Where(ModuleCatalog.Names.Contains)
            .ToList();
        Assert.Empty(forbidden);
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Module_exposes_only_its_module_class(string assemblyName)
    {
        var exported = ModuleCatalog.ByName(assemblyName).GetExportedTypes()
            .Where(t => t.Namespace?.EndsWith(".Migrations", StringComparison.Ordinal) != true) // EF'in ürettiği migration sınıfları
            .ToList();
        var module = Assert.Single(exported);
        Assert.True(typeof(IModule).IsAssignableFrom(module));
    }

    [Fact]
    public void Contracts_do_not_depend_on_module_implementations_or_infrastructure()
    {
        var contracts = ModuleCatalog.Assemblies
            .SelectMany(m => m.GetReferencedAssemblies())
            .Where(r => r.Name!.EndsWith(".Contracts", StringComparison.Ordinal))
            .Select(Assembly.Load)
            .Distinct()
            .ToList();
        Assert.NotEmpty(contracts);
        foreach (var contract in contracts)
        {
            var references = contract.GetReferencedAssemblies().Select(r => r.Name!).ToList();
            Assert.DoesNotContain(references, ModuleCatalog.Names.Contains);
            Assert.DoesNotContain("RentACar.Shared.Infrastructure", references);
            Assert.DoesNotContain(references, r => r.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Every_module_has_a_unique_name()
    {
        var names = ModuleCatalog.Assemblies
            .Select(a => (IModule)Activator.CreateInstance(a.GetExportedTypes().Single(t => typeof(IModule).IsAssignableFrom(t)))!)
            .Select(m => m.Name)
            .ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }
}
