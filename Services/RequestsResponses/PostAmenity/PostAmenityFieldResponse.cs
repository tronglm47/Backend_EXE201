using Services.RequestsResponses.PropertyForm;

namespace Services.RequestsResponses.PostAmenity
{
    public class PostAmenityFieldResponse : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "postid", "amenityid", "note"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "postid":
                    result["postid"] = (item as PostAmenityResponse.GetAll)?.PostId
                                        ?? (item as PostAmenityResponse.GetById)?.PostId;
                    break;
                case "amenityid":
                    result["amenityid"] = (item as PostAmenityResponse.GetAll)?.AmenityId
                                        ?? (item as PostAmenityResponse.GetById)?.AmenityId;
                    break;
                case "note":
                    result["note"] = (item as PostAmenityResponse.GetAll)?.Notes
                                        ?? (item as PostAmenityResponse.GetById)?.Notes;
                    break;
                default:
                    throw new ArgumentException($"Field '{field}' is not a valid field for PropertyForm.");
            }
        }
    }
}
