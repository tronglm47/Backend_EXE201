using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestsResponses.PropertyType
{
    public class PropertyTypeQueryParameters : QueryParametersBase
    {
        public override string DefaultSortBy => "propertytypeid";
    }
}
