using Services.RequestsResponses.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestsResponses.PropertyType
{
    public class PropertyTypeFieldResponse : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "propertytypeid", "name", "description", "createdat"
        };

        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch(field)
            {
                case "propertytypeid":
                    result["propertytypeid"] = (item as PropertyTypeResponse.PropertyTypeGetAllResponse)?.PropertyTypeId
                                        ?? (item as PropertyTypeResponse.PropertyTypeGetByIdResponse)?.PropertyTypeId;
                    break;
                case "name":
                    result["name"] = (item as PropertyTypeResponse.PropertyTypeGetAllResponse)?.Name
                                        ?? (item as PropertyTypeResponse.PropertyTypeGetByIdResponse)?.Name;
                    break;
                case "description":
                    result["description"] = (item as PropertyTypeResponse.PropertyTypeGetAllResponse)?.Description
                                        ?? (item as PropertyTypeResponse.PropertyTypeGetByIdResponse)?.Description;
                    break;
                case "createdat":
                    result["createdat"] = (item as PropertyTypeResponse.PropertyTypeGetAllResponse)?.CreatedAt
                                        ?? (item as PropertyTypeResponse.PropertyTypeGetByIdResponse)?.CreatedAt;
                    break;
                default:
                    throw new ArgumentException($"Field '{field}' is not a valid field for PropertyType.");
            }
        }
    }
}
