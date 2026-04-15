namespace BuildingBlocks.Core.Pagination;

public record PagedQuery(int Page = 1, int PageSize = 10);