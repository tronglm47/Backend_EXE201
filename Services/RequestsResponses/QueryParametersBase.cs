using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestsResponses
{
    public abstract class QueryParametersBase
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
        
        public string? SearchField { get; set; } = null;
        public string? Search { get; set; }

        // default sort field sẽ do class con override
        public abstract string DefaultSortBy { get; }

        private string? _sortBy;
        public string? SortBy
        {
            get => string.IsNullOrWhiteSpace(_sortBy) ? DefaultSortBy : _sortBy;
            set => _sortBy = value;
        }

        public bool IsDescending { get; set; } = false; // true = desc, false = asc

        public string? Select { get; set; }

        public List<string> GetSortByFields()
        {
            return SortBy!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(x => x.Trim())
                          .ToList();
        }

        public List<string> GetSelectFields()
        {
            if (string.IsNullOrWhiteSpace(Select))
                return new List<string>();

            return Select.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim().ToLowerInvariant())
                         .ToList();
        }
    }
}
