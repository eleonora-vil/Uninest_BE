namespace BE_EXE201.Common.Payloads.Requests.Home
{
    public class UpdateHomeRequest
    {
        public string? Name { get; set; }
        public string? Price { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public int? Bathroom { get; set; }
        public int? Bedrooms { get; set; }

        // Foreign key updates for Location and Utilities
        public int? LocationId { get; set; }
        public int? UtilitiesId { get; set; }

        // Location properties for updating
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Town { get; set; }
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }

        // Utilities properties for updating
        public bool? Elevator { get; set; }
        public bool? SwimmingPool { get; set; }
        public bool? Gym { get; set; }
        public bool? TV { get; set; }
        public bool? Refrigerator { get; set; }
        public bool? Parking { get; set; }
        public bool? Balcony { get; set; }
        public bool? AirConditioner { get; set; }

        // Handling HomeImages for updating existing or adding new images
        public List<UpdateHomeImageRequest>? HomeImages { get; set; }
    }

}
