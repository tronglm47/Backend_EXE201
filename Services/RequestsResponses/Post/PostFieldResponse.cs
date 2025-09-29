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
            "postid", "userid", "posttypeid", "propertytypeid", "propertyformid", 
            "locationid", "title", "content", "images", "price", "status", "createdat", "views"
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
                case "propertytypeid":
                    result["propertyTypeId"] = (item as PostResponse.PostGetAllResponse)?.PropertyTypeId
                                                ?? (item as PostResponse.PostGetByIdResponse)?.PropertyTypeId;
                    break;
                case "propertyformid":
                    result["propertyFormId"] = (item as PostResponse.PostGetAllResponse)?.PropertyFormId
                                                ?? (item as PostResponse.PostGetByIdResponse)?.PropertyFormId;
                    break;
                case "locationid":
                    result["locationId"] = (item as PostResponse.PostGetAllResponse)?.LocationId
                                            ?? (item as PostResponse.PostGetByIdResponse)?.LocationId;
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

        //public static object SelectFields(OrderResponses.OrderGetAllResponses item, List<string> selectedFields)
        //{
        //    if (!selectedFields.Any())
        //        return item;

        //    var result = new Dictionary<string, object?>();

        //    foreach (var field in selectedFields.Where(f => ValidFields.Contains(f)))
        //    {
        //        switch (field.ToLowerInvariant())
        //        {
        //            case "orderid":
        //                result["orderid"] = item.OrderId;
        //                break;
        //            case "userid":
        //                result["userid"] = item.UserId;
        //                break;
        //            case "orderdate":
        //                result["orderdate"] = item.OrderDate;
        //                break;
        //            case "status":
        //                result["status"] = item.Status;
        //                break;
        //        }
        //    }
        //    return result;
        //}
        //public static object SelectFields(OrderResponses.OrderGetByIdResponses item, List<string> selectedFields)
        //{
        //    if (!selectedFields.Any())
        //        return item;

        //    var result = new Dictionary<string, object?>();

        //    foreach (var field in selectedFields.Where(f => ValidFields.Contains(f)))
        //    {
        //        switch (field.ToLowerInvariant())
        //        {
        //            case "orderid":
        //                result["orderid"] = item.OrderId;
        //                break;
        //            case "userid":
        //                result["userid"] = item.UserId;
        //                break;
        //            case "orderdate":
        //                result["orderdate"] = item.OrderDate;
        //                break;
        //            case "status":
        //                result["status"] = item.Status;
        //                break;
        //        }
        //    }
        //    return result;
        //}


    }
}
