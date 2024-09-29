using AutoMapper;
using BE_EXE201.Common.Payloads.Requests.Home;
using BE_EXE201.Common.Payloads.Responses.Home;
using BE_EXE201.Dtos;
using BE_EXE201.Entities;
using BE_EXE201.Repositories;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;

namespace BE_EXE201.Services
{
    public class HomeService
    {
        private readonly IRepository<Home, int> _homeRepository;
        private readonly IRepository<HomeImage, int> _homeImageRepository;
        private readonly CloudService _cloudService;
        private readonly IMapper _mapper;

        public HomeService(IRepository<Home, int> homeRepository, IRepository<HomeImage, int> homeImageRepository,CloudService cloudService, IMapper mapper)
        {
            _homeRepository = homeRepository;
            _homeImageRepository = homeImageRepository;
            _cloudService = cloudService;
            _mapper = mapper;
        }

        // Get all homes
        public async Task<IEnumerable<HomeResonse>> GetAllHomes()
        {
            var homes = await _homeRepository.GetAll()
                .Include(h => h.HomeImages)
                .ThenInclude(hi => hi.Image)
                .ToListAsync();

            // Map to HomeResponse
            var homeResponses = homes.Select(home => new HomeResonse
            {
                HomeId = home.HomeId,
                Name = home.Name,
                Price = home.Price,
                Size = home.Size,
                Description = home.Description,
                Bathroom = home.Bathroom,
                Bedrooms = home.Bedrooms,
                //HouseStatus = home.HouseStatus,
                //Status = home.Status,
                //CreateDate = home.CreateDate,
                //ModifyDate = home.ModifyDate,
                //CreateBy = home.CreateBy,
                //ModifyBy = home.ModifyBy,
                //ApproveStatus = home.ApproveStatus,
                LocationId = home.LocationId,
                UtilitiesId = home.UtilitiesId,
                HomeImages = home.HomeImages.Select(hi => new HomeImageModel
                {
                    HomeImageId = hi.HomeImageId,
                    HomeId = hi.HomeId, // Assuming this is the foreign key in HomeImage
                    Image = new ImageModel
                    {
                        ImageId = hi.Image.ImageId,
                        ImageUrl = hi.Image.ImageUrl
                    },
                    ImageDescription = hi.ImageDescription // Or any other property from HomeImage
                }).ToList()
            }).ToList();

            return homeResponses;
        }

        // Get home by ID
        public async Task<HomeResonse> GetHomeById(int id)
        {
            var home = await _homeRepository.GetAll()
                .Include(h => h.HomeImages)
                .ThenInclude(hi => hi.Image)
                .FirstOrDefaultAsync(h => h.HomeId == id);

            if (home is not null)
            {
                // Map to HomeResponse
                var homeResponse = new HomeResonse
                {
                    HomeId = home.HomeId,
                    Name = home.Name,
                    Price = home.Price,
                    Size = home.Size,
                    Description = home.Description,
                    Bathroom = home.Bathroom,
                    Bedrooms = home.Bedrooms,
                    LocationId = home.LocationId,
                    UtilitiesId = home.UtilitiesId,
                    HomeImages = home.HomeImages.Select(hi => new HomeImageModel
                    {
                        HomeImageId = hi.HomeImageId,
                        HomeId = hi.HomeId, // Assuming this is the foreign key in HomeImage
                        Image = new ImageModel
                        {
                            ImageId = hi.Image.ImageId,
                            ImageUrl = hi.Image.ImageUrl
                        },
                        ImageDescription = hi.ImageDescription // Or any other property from HomeImage
                    }).ToList()
                };

                return homeResponse;
            }

            return null; // Or throw an exception if you prefer
        }

        // Create a new home
        public async Task<HomeModel> CreateNewHome(HomeModel newHome, List<IFormFile> imageFiles)
        {
            var homeEntity = _mapper.Map<Home>(newHome);

            // Make sure you don't set homeEntity.HomeId here.
            // homeEntity.HomeId = newHome.HomeId; // Remove this line if it exists

            // Upload images using CloudService
            List<ImageUploadResult> uploadResults = await _cloudService.UploadImagesAsync(imageFiles);

            // Handle adding new HomeImages with the uploaded URLs
            if (uploadResults.Any())
            {
                foreach (var uploadResult in uploadResults)
                {
                    var homeImageEntity = new HomeImage
                    {
                        Image = new Image
                        {
                            ImageUrl = uploadResult.SecureUrl.ToString()
                        },
                        ImageDescription = null // Optionally add descriptions here
                    };
                    homeEntity.HomeImages.Add(homeImageEntity);
                }
            }

            // Save the home entity with the images
            await _homeRepository.AddAsync(homeEntity);
            var result = await _homeRepository.Commit();

            if (result > 0)
            {
                newHome.HomeId = homeEntity.HomeId; // This will be set after the entity is saved
                return newHome;
            }
            else
            {
                return null;
            }
        }



        // Update an existing home and its images
        public async Task<HomeModel> UpdateHome(int homeId, UpdateHomeRequest req)
        {
            try
            {
                // Fetch the home with its associated images
                var homeEntity = await _homeRepository
                    .GetAll()
                    .Include(h => h.HomeImages)
                    .ThenInclude(hi => hi.Image)
                    .FirstOrDefaultAsync(h => h.HomeId == homeId);

                if (homeEntity != null)
                {
                    // Update basic home info
                    if (!string.IsNullOrEmpty(req.Name)) homeEntity.Name = req.Name;
                    if (req.Price.HasValue) homeEntity.Price = req.Price.Value;
                    if (req.Size.HasValue) homeEntity.Size = req.Size.Value;
                    if (!string.IsNullOrEmpty(req.Description)) homeEntity.Description = req.Description;
                    if (req.Bathroom.HasValue) homeEntity.Bathroom = req.Bathroom.Value;
                    if (req.Bedrooms.HasValue) homeEntity.Bedrooms = req.Bedrooms.Value;

                    // Update foreign key references
                    if (req.LocationId.HasValue) homeEntity.LocationId = req.LocationId.Value;
                    if (req.UtilitiesId.HasValue) homeEntity.UtilitiesId = req.UtilitiesId.Value;

                    // Handle HomeImage updates (add new or update existing)
                    if (req.HomeImages != null)
                    {
                        foreach (var updatedImage in req.HomeImages)
                        {
                            if (updatedImage.HomeImageId.HasValue)
                            {
                                // Update existing image
                                var existingImage = homeEntity.HomeImages.FirstOrDefault(hi => hi.HomeImageId == updatedImage.HomeImageId);
                                if (existingImage != null)
                                {
                                    existingImage.ImageDescription = updatedImage.ImageDescription;
                                    // If needed, you can also update the `ImageId` or related fields here
                                }
                            }
                            else
                            {
                                // Add a new image
                                var newHomeImage = new HomeImage
                                {
                                    ImageId = updatedImage.ImageId.Value, // Assuming ImageId is mandatory for new images
                                    ImageDescription = updatedImage.ImageDescription
                                };
                                homeEntity.HomeImages.Add(newHomeImage);
                            }
                        }
                    }

                    // Commit changes to repository
                    _homeRepository.Update(homeEntity);
                    var result = await _homeRepository.Commit();

                    if (result > 0)
                    {
                        return _mapper.Map<HomeModel>(homeEntity);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating home: {ex.Message}");
                return null;
            }
        }


        // Delete home (and its images)
        public async Task<bool> DeleteHome(int id)
        {
            var home = await _homeRepository.GetAll().Include(h => h.HomeImages).FirstOrDefaultAsync(h => h.HomeId == id);
            if (home is not null)
            {
                _homeRepository.Remove(home.HomeId);
                var result = await _homeRepository.Commit();
                return result > 0;
            }
            return false;
        }

        // Delete a specific image from a home
        public async Task<bool> DeleteHomeImage(int homeImageId)
        {
            var homeImage = await _homeImageRepository.GetByIdAsync(homeImageId);
            if (homeImage != null)
            {
                _homeImageRepository.Remove(homeImage.HomeImageId);
                var result = await _homeImageRepository.Commit();
                return result > 0;
            }
            return false;
        }
    }

}
