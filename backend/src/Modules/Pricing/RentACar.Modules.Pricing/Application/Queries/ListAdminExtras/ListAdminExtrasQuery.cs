using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.ListAdminExtras;

/// <summary>Pasifler dahil tüm ekstralar (yönetim).</summary>
internal sealed record ListAdminExtrasQuery : IQuery<IReadOnlyList<AdminExtraDto>>;
