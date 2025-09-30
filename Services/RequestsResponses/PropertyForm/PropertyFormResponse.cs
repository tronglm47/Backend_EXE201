namespace Services.RequestsResponses.PropertyForm
{
    public class PropertyFormResponse
    {
        public class PropertyFormGetAllResponse
        {
            public int PropertyFormId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
        public class PropertyFormGetByIdResponse
        {
            public int PropertyFormId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
    }
}
