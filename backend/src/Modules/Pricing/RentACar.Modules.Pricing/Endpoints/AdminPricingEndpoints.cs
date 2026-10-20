using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Pricing.Application.Commands.CreateSeason;
using RentACar.Modules.Pricing.Application.Commands.DeleteSeason;
using RentACar.Modules.Pricing.Application.Commands.SetDailyRate;
using RentACar.Modules.Pricing.Application.Commands.UpdateExtra;
using RentACar.Modules.Pricing.Application.Commands.UpdatePricingPolicy;
using RentACar.Modules.Pricing.Application.Commands.UpdateSeason;
using RentACar.Modules.Pricing.Application.Queries.GetPricingPolicy;
using RentACar.Modules.Pricing.Application.Queries.ListAdminExtras;
using RentACar.Modules.Pricing.Application.Queries.ListRatePlans;
using RentACar.Modules.Pricing.Application.Queries.ListSeasons;
using RentACar.Modules.Pricing.Endpoints.Requests;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Pricing.Endpoints;

internal static class AdminPricingEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAdminGroup("pricing", "Fiyatlandırma");

        group.MapGet("/rate-plans", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new ListRatePlansQuery(), ct));
        group.MapPut("/rate-plans/{categoryId:guid}", async (Guid categoryId, SetDailyRateRequest r, IDispatcher d, CancellationToken ct) =>
        {
            await d.SendAsync(new SetDailyRateCommand(categoryId, r.DailyRate), ct);
            return TypedResults.NoContent();
        });

        group.MapGet("/seasons", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new ListSeasonsQuery(), ct));
        group.MapPost("/seasons", CreateSeasonAsync);
        group.MapPut("/seasons/{id:guid}", async (Guid id, UpsertSeasonRequest r, IDispatcher d, CancellationToken ct) =>
        {
            await d.SendAsync(new UpdateSeasonCommand(id, r.Name, r.StartMonth, r.StartDay, r.EndMonth, r.EndDay, r.Multiplier), ct);
            return TypedResults.NoContent();
        });
        group.MapDelete("/seasons/{id:guid}", async (Guid id, IDispatcher d, CancellationToken ct) =>
        {
            await d.SendAsync(new DeleteSeasonCommand(id), ct);
            return TypedResults.NoContent();
        });

        group.MapGet("/extras", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new ListAdminExtrasQuery(), ct));
        group.MapPut("/extras/{code}", async (string code, UpdateExtraRequest r, IDispatcher d, CancellationToken ct) =>
        {
            await d.SendAsync(new UpdateExtraCommand(code, r.Name, r.Description, r.Price, r.MaxQuantity, r.MaxChargeDays, r.IsActive), ct);
            return TypedResults.NoContent();
        });

        group.MapGet("/policy", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new GetPricingPolicyQuery(), ct));
        group.MapPut("/policy", (UpdatePricingPolicyRequest r, IDispatcher d, CancellationToken ct) =>
            d.SendAsync(new UpdatePricingPolicyCommand(r.OneWayFee, r.OutOfHoursFee, r.YoungDriverMaxAge, r.YoungDriverDailyFee, r.GraceMinutes), ct));
    }

    private static async Task<Created<SeasonDto>> CreateSeasonAsync(UpsertSeasonRequest r, IDispatcher dispatcher, CancellationToken ct)
    {
        var season = await dispatcher.SendAsync(new CreateSeasonCommand(r.Name, r.StartMonth, r.StartDay, r.EndMonth, r.EndDay, r.Multiplier), ct);
        return TypedResults.Created($"/api/admin/pricing/seasons/{season.Id}", season);
    }
}
