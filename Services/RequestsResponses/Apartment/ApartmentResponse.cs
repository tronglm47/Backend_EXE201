using Services.RequestsResponses.Building;

namespace Services.RequestsResponses.Apartment
{
    public class ApartmentResponse
    {
        public class ApartmentGetAll
        {
            public int ApartmentId { get; set; }
            public int BuildingId { get; set; }
            public string ApartmentCode { get; set; }
            public int Floor { get; set; }
            public double Area { get; set; }
            public string ApartmentType { get; set; }
            public string Status { get; set; }
            public int NumberOfBedrooms { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        public class ApartmentDetail
        {
            public int ApartmentId { get; set; }
            public int BuildingId { get; set; }
            public BuildingResponse.BuildingDetail Building { get; set; }
            public string ApartmentCode { get; set; }
            public int Floor { get; set; }
            public double Area { get; set; }
            public string ApartmentType { get; set; }
            public string Status { get; set; }
            public int NumberOfBedrooms { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
