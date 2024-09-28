using BE_EXE201.Entities;
using BE_EXE201.Repositories;

namespace BE_EXE201.Services
{
    public class ImageService
    {
        private readonly IRepository<Image, int> _imageRepository;

        public ImageService(IRepository<Image, int> imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public async Task<bool> SaveImageAsync(string imageUrl)
        {
            var image = new Image { ImageUrl = imageUrl };

            await _imageRepository.AddAsync(image);
            var result = await _imageRepository.Commit();
            return result > 0;
        }
    }
}
