using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestsResponses.Basic
{
    public class PaginationResult<T> where T : class
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        public IEnumerable<T> Items { get; set; } = new List<T>();

        public static PaginationResult<T> Create(IEnumerable<T> source, int page, int pageSize)
        {
            var totalItems = source.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var items = source.Skip((page - 1) * pageSize).Take(pageSize);

            return new PaginationResult<T>
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Items = items
            };
        }
    }
}
