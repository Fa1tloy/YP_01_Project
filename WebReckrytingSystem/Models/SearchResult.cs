namespace WebReckrytingSystem.Models
{
    public class SearchResult<T>
    {
        public ICollection<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;

        // Backward-compatible alias for older call sites
        public int Page
        {
            get => PageNumber;
            set => PageNumber = value;
        }
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}