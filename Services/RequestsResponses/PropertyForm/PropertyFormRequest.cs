namespace Services.RequestsResponses.PropertyForm
{
    public class PropertyFormRequest
    {
        public class CreatePropertyForm
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }
        public class UpdatePropertyForm
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
    }
}
