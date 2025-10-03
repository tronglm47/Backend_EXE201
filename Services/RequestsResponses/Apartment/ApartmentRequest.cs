using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Apartment
{
    public class ApartmentRequest
    {
        public class ApartmentCreate
        {
            [Required]
            public int BuildingId { get; set; }
            [Required]
            public string ApartmentCode { get; set; }
            public int Floor { get; set; }
            public double Area { get; set; }
            public string ApartmentType { get; set; }
            public string Status { get; set; }
            public int NumberOfBedrooms { get; set; }
        }
        public class ApartmentUpdate
        {
            public string ApartmentCode { get; set; }
            public int Floor { get; set; }
            public double Area { get; set; }
            public string ApartmentType { get; set; }
            public string Status { get; set; }
        }
    }
}
