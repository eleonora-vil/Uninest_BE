namespace BE_EXE201.Dtos
{
    public class HomeImageModel
    {
        public int HomeImageId { get; set; }
        public int HomeId { get; set; }
        public ImageModel Image { get; set; }
        public string? ImageDescription { get; set; }
    }

}
