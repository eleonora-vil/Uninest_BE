namespace BE_EXE201.Common.Payloads.Requests.Home
{
    public class UpdateHomeImageRequest
    {
        public int? HomeImageId { get; set; } // This is optional, null if adding a new image
        public int? ImageId { get; set; } // The ImageId references the existing image or the one to be added
        public string? ImageDescription { get; set; } // Optional description for the image
    }
}
