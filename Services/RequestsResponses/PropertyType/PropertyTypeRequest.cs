using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestsResponses.PropertyType
{
    public class PropertyTypeRequest
    {
        public class CreateRequest
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }
        public class UpdateRequest
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
    }
}
