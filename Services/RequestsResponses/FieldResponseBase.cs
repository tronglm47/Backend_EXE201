using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestsResponses
{
    public abstract class FieldResponseBase
    {
        protected abstract List<string> ValidFields { get; }

        /// <summary>
        /// Map field → giá trị (được override từ class con)
        /// </summary>
        protected abstract void MapField<T>(string field, T item, Dictionary<string, object?> result);

        public object SelectFields<T>(T item, List<string> selectedFields)
        {
            if (!selectedFields.Any())
                return item;

            var result = new Dictionary<string, object?>();

            foreach (var field in selectedFields.Where(f => ValidFields.Contains(f, StringComparer.OrdinalIgnoreCase)))
            {
                MapField(field.ToLowerInvariant(), item, result);
            }

            return result;
        }
    }
}
