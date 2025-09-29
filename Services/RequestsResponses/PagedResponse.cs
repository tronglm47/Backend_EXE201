using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
