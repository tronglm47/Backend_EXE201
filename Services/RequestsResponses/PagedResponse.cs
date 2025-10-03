namespace Services.RequestsResponses
{
    public class PagedResponse<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}
