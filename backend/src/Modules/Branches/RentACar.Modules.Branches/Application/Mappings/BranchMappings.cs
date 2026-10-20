using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Domain.Entities;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Branches.Application.Mappings;

internal static class BranchMappings
{
    /// <summary>LINQ projeksiyonunun son adımında çağrılır: EF kolonları çeker, <c>ToGeoPoint</c> istemcide çalışır.</summary>
    public static BranchDto ToDto(this Branch b) => new(
        b.Id, b.Code, b.Name, b.Type, b.District, b.Address, b.Location.ToGeoPoint(),
        b.Phone, b.OpensAt, b.ClosesAt, b.IsActive, b.DeliveryFee);
}
