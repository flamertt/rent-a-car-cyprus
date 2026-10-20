namespace RentACar.Shared.Abstractions.Common;

public static class Paging
{
    public const int MaxPageSize = 200;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize) =>
        (Math.Max(1, page ?? 1), Math.Clamp(pageSize ?? 25, 1, MaxPageSize));
}
