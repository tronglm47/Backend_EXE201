using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestsResponses.PropertyType
{
    public class PropertyTypeResponse
    {
        public class PropertyTypeGetAllResponse
        {
            public int PropertyTypeId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
        public class PropertyTypeGetByIdResponse
        {
            public int PropertyTypeId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
    }
}
