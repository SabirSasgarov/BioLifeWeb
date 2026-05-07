using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;

namespace BioLife.MVC.Helpers
{
	public class PaginatedList<T> : List<T>
	{
		public int PageIndex { get; private set; }
		public int TotalPages { get; private set; }
		public int TotalCount { get; private set; }
		public int PageSize { get; private set; }

		private PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
		{
			PageIndex = pageIndex;
			TotalPages = (int)Math.Ceiling(count / (double)pageSize);
			TotalCount = count;
			PageSize = pageSize;
			this.AddRange(items);
		}
		public bool HasPreviousPage => PageIndex > 1;
		public bool HasNextPage => PageIndex < TotalPages;
		public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
		{
			var count = await source.CountAsync();
			var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
			return new PaginatedList<T>(items, count, pageIndex, pageSize);
		}

		public IEnumerable<int> PageNumbers(int windowsSize = 5)
		{
			var half = windowsSize / 2;
			var start = Math.Max(2, PageIndex - half);
			var end = Math.Min(TotalPages - 1, PageIndex + half);

			if(PageIndex - half < 2)
			{
				end = Math.Min(TotalPages - 1, 1 + windowsSize);
			}
			if(PageIndex + half > TotalPages - 1)
			{
				start = Math.Max(2, TotalPages - windowsSize);
			}
			yield return 1;

			if(start > 2)
				yield return -1;
			
			for(int i = start; i <= end; i++)
				yield return i;
			
			if (end < TotalPages - 1)
				yield return -1;
			
			if(TotalPages > 1)
				yield return TotalPages;
		}

	}
}
