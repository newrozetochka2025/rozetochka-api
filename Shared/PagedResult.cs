namespace rozetochka_api.Shared
{
    public class PagedResult<T>
    {
        public List<T> Items { get; init; } = new();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalItems { get; init; }

        public int TotalPages
        {
            get
            {
                if (PageSize <= 0) return 0;
                    var pages = TotalItems / PageSize;
                if (TotalItems % PageSize > 0) 
                    pages++;
                return pages;
            }
        }
    }
}
