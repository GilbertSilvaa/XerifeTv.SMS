namespace BuildingBlocks.Core;

public sealed class PagedList<T> where T : class
{
	public PagedList(int currentPage, int totalPageCount, IEnumerable<T> items)
	{
		CurrentPage = currentPage;
		TotalPageCount = totalPageCount;
		Items = items;
	}

	public int PageSize => Items.Count();
	public int CurrentPage { get; private set; }
	public int TotalPageCount { get; private set; }
	public IEnumerable<T> Items { get; private set; } = [];
	public bool HasPrevious => CurrentPage > 1;
	public bool HasNext => CurrentPage < TotalPageCount;
}