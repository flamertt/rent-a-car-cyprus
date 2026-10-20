using NetTopologySuite.Geometries;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Branches.Domain.Entities;

internal sealed class Branch
{
    private Branch()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>İnsan okunur kısa kod, örn. GRN-OFC.</summary>
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public BranchType Type { get; private set; }

    public District District { get; private set; }

    public string Address { get; private set; } = null!;

    public Point Location { get; private set; } = null!;

    public string? Phone { get; private set; }

    public TimeOnly OpensAt { get; private set; }

    public TimeOnly ClosesAt { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>Bu noktada teslim/iade için alınan sabit ücret (TRY). Ofislerde 0.</summary>
    public decimal DeliveryFee { get; private set; }

    public static Branch Create(
        string code, string name, BranchType type, District district, string address,
        GeoPoint location, string? phone, TimeOnly opensAt, TimeOnly closesAt, decimal deliveryFee)
    {
        var branch = new Branch { Id = Guid.CreateVersion7(), IsActive = true };
        branch.Update(code, name, type, district, address, location, phone, opensAt, closesAt, deliveryFee);
        return branch;
    }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    public void Update(
        string code, string name, BranchType type, District district, string address,
        GeoPoint location, string? phone, TimeOnly opensAt, TimeOnly closesAt, decimal deliveryFee)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("branch.name_required", "Şube kodu ve adı zorunlu.");
        }

        if (!location.IsInsideKktcBounds)
        {
            throw new DomainException("branch.outside_kktc", "Şube konumu KKTC sınırları dışında.");
        }

        if (deliveryFee < 0)
        {
            throw new DomainException("branch.negative_fee", "Teslim ücreti negatif olamaz.");
        }

        Code = NormalizeCode(code);
        Name = name.Trim();
        Type = type;
        District = district;
        Address = address.Trim();
        Location = location.ToPoint();
        Phone = phone;
        OpensAt = opensAt;
        ClosesAt = closesAt;
        DeliveryFee = deliveryFee;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
