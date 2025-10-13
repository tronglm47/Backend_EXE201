namespace Services.RequestsResponses.Booking
{
    public class BookingField : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "bookingid", "renterid", "rentername", "renteremail", "renterphone",
            "postid", "posttitle", "postdescription", "postprice", "posttype",
            "landlordid", "landlordname", "landlordemail", "landlordphone",
            "apartmentid", "apartmentcode", "floor", "area", "numberbathroom",
            "buildingid", "buildingname", "blockcode",
            "subdivisionid", "subdivisionname", "subdivisiontype", "subdivisiondescription",
            "meetingtime", "placemeet", "status", "createdat"
        };

        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "bookingid":
                    result["bookingid"] = (item as BookingResponse.BookingInfo)?.BookingId 
                                        ?? (item as BookingResponse.BookingForRenter)?.BookingId
                                        ?? (item as BookingResponse.BookingForLandlord)?.BookingId
                                        ?? (item as BookingResponse.BookingDetail)?.BookingId;
                    break;
                case "renterid":
                    result["renterid"] = (item as BookingResponse.BookingInfo)?.RenterId 
                                        ?? (item as BookingResponse.BookingForLandlord)?.RenterId
                                        ?? (item as BookingResponse.BookingDetail)?.RenterId;
                    break;
                case "rentername":
                    result["rentername"] = (item as BookingResponse.BookingInfo)?.RenterName 
                                          ?? (item as BookingResponse.BookingForLandlord)?.RenterName
                                          ?? (item as BookingResponse.BookingDetail)?.RenterName;
                    break;
                case "renteremail":
                    result["renteremail"] = (item as BookingResponse.BookingInfo)?.RenterEmail 
                                           ?? (item as BookingResponse.BookingForLandlord)?.RenterEmail
                                           ?? (item as BookingResponse.BookingDetail)?.RenterEmail;
                    break;
                case "renterphone":
                    result["renterphone"] = (item as BookingResponse.BookingInfo)?.RenterPhone 
                                           ?? (item as BookingResponse.BookingForLandlord)?.RenterPhone
                                           ?? (item as BookingResponse.BookingDetail)?.RenterPhone;
                    break;
                case "postid":
                    result["postid"] = (item as BookingResponse.BookingInfo)?.PostId 
                                     ?? (item as BookingResponse.BookingForRenter)?.PostId
                                     ?? (item as BookingResponse.BookingForLandlord)?.PostId
                                     ?? (item as BookingResponse.BookingDetail)?.PostId;
                    break;
                case "posttitle":
                    result["posttitle"] = (item as BookingResponse.BookingInfo)?.PostTitle 
                                         ?? (item as BookingResponse.BookingForRenter)?.PostTitle
                                         ?? (item as BookingResponse.BookingForLandlord)?.PostTitle
                                         ?? (item as BookingResponse.BookingDetail)?.PostTitle;
                    break;
                case "postdescription":
                    result["postdescription"] = (item as BookingResponse.BookingForRenter)?.PostDescription
                                               ?? (item as BookingResponse.BookingDetail)?.PostDescription;
                    break;
                case "postprice":
                    result["postprice"] = (item as BookingResponse.BookingForRenter)?.PostPrice
                                         ?? (item as BookingResponse.BookingDetail)?.PostPrice;
                    break;
                case "posttype":
                    result["posttype"] = (item as BookingResponse.BookingForRenter)?.PostType;
                    break;
                case "landlordid":
                    result["landlordid"] = (item as BookingResponse.BookingForRenter)?.LandlordId
                                          ?? (item as BookingResponse.BookingDetail)?.LandlordId;
                    break;
                case "landlordname":
                    result["landlordname"] = (item as BookingResponse.BookingForRenter)?.LandlordName
                                            ?? (item as BookingResponse.BookingDetail)?.LandlordName;
                    break;
                case "landlordemail":
                    result["landlordemail"] = (item as BookingResponse.BookingForRenter)?.LandlordEmail
                                             ?? (item as BookingResponse.BookingDetail)?.LandlordEmail;
                    break;
                case "landlordphone":
                    result["landlordphone"] = (item as BookingResponse.BookingForRenter)?.LandlordPhone
                                             ?? (item as BookingResponse.BookingDetail)?.LandlordPhone;
                    break;
                case "apartmentid":
                    result["apartmentid"] = (item as BookingResponse.BookingForRenter)?.Apartment?.ApartmentId
                                           ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.ApartmentId;
                    break;
                case "apartmentcode":
                    result["apartmentcode"] = (item as BookingResponse.BookingForRenter)?.Apartment?.ApartmentCode
                                             ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.ApartmentCode;
                    break;
                case "floor":
                    result["floor"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Floor
                                    ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Floor;
                    break;
                case "area":
                    result["area"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Area
                                   ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Area;
                    break;
                case "numberbathroom":
                    result["numberbathroom"] = (item as BookingResponse.BookingForRenter)?.Apartment?.NumberBathroom
                                             ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.NumberBathroom;
                    break;
                case "buildingid":
                    result["buildingid"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Building?.BuildingId
                                         ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Building?.BuildingId;
                    break;
                case "buildingname":
                    result["buildingname"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Building?.BuildingName
                                            ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Building?.BuildingName;
                    break;
                case "blockcode":
                    result["blockcode"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Building?.BlockCode
                                         ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Building?.BlockCode;
                    break;
                case "subdivisionid":
                    result["subdivisionid"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Building?.Subdivision?.SubdivisionId
                                             ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Building?.Subdivision?.SubdivisionId;
                    break;
                case "subdivisionname":
                    result["subdivisionname"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Building?.Subdivision?.SubdivisionName
                                               ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Building?.Subdivision?.SubdivisionName;
                    break;
                case "subdivisiontype":
                    result["subdivisiontype"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Building?.Subdivision?.Type
                                               ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Building?.Subdivision?.Type;
                    break;
                case "subdivisiondescription":
                    result["subdivisiondescription"] = (item as BookingResponse.BookingForRenter)?.Apartment?.Building?.Subdivision?.Description
                                                      ?? (item as BookingResponse.BookingForLandlord)?.Apartment?.Building?.Subdivision?.Description;
                    break;
                case "meetingtime":
                    result["meetingtime"] = (item as BookingResponse.BookingInfo)?.MeetingTime 
                                          ?? (item as BookingResponse.BookingForRenter)?.MeetingTime
                                          ?? (item as BookingResponse.BookingForLandlord)?.MeetingTime
                                          ?? (item as BookingResponse.BookingDetail)?.MeetingTime;
                    break;
                case "placemeet":
                    result["placemeet"] = (item as BookingResponse.BookingInfo)?.PlaceMeet 
                                        ?? (item as BookingResponse.BookingForRenter)?.PlaceMeet
                                        ?? (item as BookingResponse.BookingForLandlord)?.PlaceMeet
                                        ?? (item as BookingResponse.BookingDetail)?.PlaceMeet;
                    break;
                case "status":
                    result["status"] = (item as BookingResponse.BookingInfo)?.Status 
                                     ?? (item as BookingResponse.BookingForRenter)?.Status
                                     ?? (item as BookingResponse.BookingForLandlord)?.Status
                                     ?? (item as BookingResponse.BookingDetail)?.Status;
                    break;
                case "createdat":
                    result["createdat"] = (item as BookingResponse.BookingInfo)?.CreatedAt 
                                        ?? (item as BookingResponse.BookingForRenter)?.CreatedAt
                                        ?? (item as BookingResponse.BookingForLandlord)?.CreatedAt
                                        ?? (item as BookingResponse.BookingDetail)?.CreatedAt;
                    break;
                default:
                    break;
            }
        }
    }
}