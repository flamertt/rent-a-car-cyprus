using NetArchTest.Rules;

namespace RentACar.UnitTests.Architecture;

/// <summary>
/// Clean Architecture katman kuralları (modül içinde): Domain hiçbir katmana ve altyapıya bağlı değil;
/// Application yalnız Domain'e (+ EF Core sorgu API'si) bağlı, HTTP ve Infrastructure bilmez; Endpoints
/// Infrastructure'a doğrudan erişmez (komut/sorgu üzerinden gider).
/// </summary>
public sealed class LayerTests
{
    public static TheoryData<string> Modules => new(ModuleCatalog.Names);

    [Theory]
    [MemberData(nameof(Modules))]
    public void Domain_is_independent(string assemblyName)
    {
        var result = Types.InAssembly(ModuleCatalog.ByName(assemblyName))
            .That().ResideInNamespace($"{assemblyName}.Domain")
            .ShouldNot().HaveDependencyOnAny(
                $"{assemblyName}.Application",
                $"{assemblyName}.Infrastructure",
                $"{assemblyName}.Endpoints",
                "RentACar.Shared.Infrastructure",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Npgsql")
            .GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Domain_does_not_use_other_modules_even_their_contracts(string assemblyName)
    {
        var others = ModuleCatalog.Names.Where(n => n != assemblyName).ToArray();
        var result = Types.InAssembly(ModuleCatalog.ByName(assemblyName))
            .That().ResideInNamespace($"{assemblyName}.Domain")
            .ShouldNot().HaveDependencyOnAny(others)
            .GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Application_does_not_know_infrastructure_or_http(string assemblyName)
    {
        var result = Types.InAssembly(ModuleCatalog.ByName(assemblyName))
            .That().ResideInNamespace($"{assemblyName}.Application")
            .ShouldNot().HaveDependencyOnAny(
                $"{assemblyName}.Infrastructure",
                $"{assemblyName}.Endpoints",
                "Microsoft.AspNetCore",
                "Npgsql")
            .GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Endpoints_go_through_application(string assemblyName)
    {
        var result = Types.InAssembly(ModuleCatalog.ByName(assemblyName))
            .That().ResideInNamespace($"{assemblyName}.Endpoints")
            .ShouldNot().HaveDependencyOnAny($"{assemblyName}.Infrastructure", $"{assemblyName}.Domain", "Microsoft.EntityFrameworkCore")
            .GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Handlers_follow_naming_convention(string assemblyName)
    {
        var result = Types.InAssembly(ModuleCatalog.ByName(assemblyName))
            .That().ImplementInterface(typeof(Shared.Abstractions.Messaging.ICommandHandler<,>))
            .Or().ImplementInterface(typeof(Shared.Abstractions.Messaging.IQueryHandler<,>))
            .Should().HaveNameEndingWith("Handler")
            .GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(NetArchTest.Rules.TestResult result) =>
        "İhlal: " + string.Join(", ", result.FailingTypeNames ?? []);
}
