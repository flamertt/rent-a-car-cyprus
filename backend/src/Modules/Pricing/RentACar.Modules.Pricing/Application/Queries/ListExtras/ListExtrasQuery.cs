using RentACar.Modules.Pricing.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.ListExtras;

/// <summary>Satıştaki (aktif) ekstralar.</summary>
internal sealed record ListExtrasQuery : IQuery<IReadOnlyList<ExtraDto>>;
