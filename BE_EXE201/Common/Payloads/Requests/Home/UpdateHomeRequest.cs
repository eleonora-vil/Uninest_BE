namespace BE_EXE201.Common.Payloads.Requests.Home
{
    public class UpdateHomeRequest
    {
        public string? Name { get; set; }
        public float? Price { get; set; }
        public float? Size { get; set; }
        public string? Description { get; set; }
        public int? Bathroom { get; set; }
        public int? Bedrooms { get; set; }

        // Foreign key updates for Location and Utilities
        public int? LocationId { get; set; }
        public int? UtilitiesId { get; set; }

        // Handling HomeImages for updating existing or adding new images
        public List<UpdateHomeImageRequest>? HomeImages { get; set; }
    }
}
