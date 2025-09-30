using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestsResponses.Post
{
    public class PostFieldResponse : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "postid", "userid", "posttypeid", "posttype", "propertytypeid", "propertytype", 
            "propertyformid", "propertyform", "locationid", "amenitiesid", "amenities", 
            "title", "content", "images", "price", "status", "createdat", "views"
        };

        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "postid":
                    result["postId"] = (item as PostResponse.PostGetAllResponse)?.PostId
                                        ?? (item as PostResponse.PostGetByIdResponse)?.PostId;
                    break;
                case "userid":
                    result["userId"] = (item as PostResponse.PostGetAllResponse)?.UserId
                                        ?? (item as PostResponse.PostGetByIdResponse)?.UserId;
                    break;
                case "posttypeid":
                    result["postTypeId"] = (item as PostResponse.PostGetAllResponse)?.PostTypeId
                                            ?? (item as PostResponse.PostGetByIdResponse)?.PostTypeId;
                    break;
                case "posttype":
                    result["postType"] = (item as PostResponse.PostGetByIdResponse)?.PostType;
                    break;
                case "propertytypeid":
                    result["propertyTypeId"] = (item as PostResponse.PostGetAllResponse)?.PropertyTypeId
                                                ?? (item as PostResponse.PostGetByIdResponse)?.PropertyTypeId;
                    break;
                case "propertytype":
                    result["propertyType"] = (item as PostResponse.PostGetByIdResponse)?.PropertyType;
                    break;
                case "propertyformid":
                    result["propertyFormId"] = (item as PostResponse.PostGetAllResponse)?.PropertyFormId
                                                ?? (item as PostResponse.PostGetByIdResponse)?.PropertyFormId;
                    break;
                case "propertyform":
                    result["propertyForm"] = (item as PostResponse.PostGetByIdResponse)?.PropertyForm;
                    break;
                case "locationid":
                    result["locationId"] = (item as PostResponse.PostGetAllResponse)?.LocationId
                                            ?? (item as PostResponse.PostGetByIdResponse)?.LocationId;
                    break;
                case "amenitiesid":
                    result["amenitiesId"] = (item as PostResponse.PostGetByIdResponse)?.AmenitiesId;
                    break;
                case "amenities":
                    result["amenities"] = (item as PostResponse.PostGetByIdResponse)?.Amenities;
                    break;
                case "title":
                    result["title"] = (item as PostResponse.PostGetAllResponse)?.Title
                                      ?? (item as PostResponse.PostGetByIdResponse)?.Title;
                    break;
                case "content":
                    result["content"] = (item as PostResponse.PostGetAllResponse)?.Content
                                        ?? (item as PostResponse.PostGetByIdResponse)?.Content;
                    break;
                case "images":
                    result["images"] = (item as PostResponse.PostGetAllResponse)?.Images
                                       ?? (item as PostResponse.PostGetByIdResponse)?.Images;
                    break;
                case "price":
                    result["price"] = (item as PostResponse.PostGetAllResponse)?.Price
                                      ?? (item as PostResponse.PostGetByIdResponse)?.Price;
                    break;
                case "status":
                    result["status"] = (item as PostResponse.PostGetAllResponse)?.Status
                                       ?? (item as PostResponse.PostGetByIdResponse)?.Status;
                    break;
                case "createdat":
                    result["createdAt"] = (item as PostResponse.PostGetAllResponse)?.CreatedAt
                                          ?? (item as PostResponse.PostGetByIdResponse)?.CreatedAt;
                    break;
                case "views":
                    result["views"] = (item as PostResponse.PostGetAllResponse)?.Views
                                      ?? (item as PostResponse.PostGetByIdResponse)?.Views;
                    break;
            }
        }
    }
}
