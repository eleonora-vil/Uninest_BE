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
        private readonly IRepository<Location, int> _locationRepository;
        private readonly IRepository<Utilities, int> _utilitiesRepository;
        private readonly CloudService _cloudService;
        private readonly IMapper _mapper;

        public HomeService(IRepository<Home, int> homeRepository,
            IRepository<HomeImage, int> homeImageRepository,
            IRepository<Location,int> locationRepository,
            IRepository<Utilities,int> utilitiesRepository,
            CloudService cloudService, IMapper mapper)
        {
            _homeRepository = homeRepository;
            _homeImageRepository = homeImageRepository;
            _locationRepository = locationRepository;
            _utilitiesRepository = utilitiesRepository;
            _cloudService = cloudService;
            _mapper = mapper;
        }

        // Get all homes
        public async Task<IEnumerable<HomeResonse>> GetAllHomes()
        {
            var homes = await _homeRepository.GetAll()
                .Include(h => h.HomeImages)
                .ThenInclude(hi => hi.Image)
                .Include(h => h.Location) // Include Location
                .Include(h => h.Utilities) // Include Utilities
                .ToListAsync();

            // Map to HomeResponse
            var homeResponses = homes.Select(home => new HomeResonse
            {
                HomeId = home.HomeId,
                UserId = home.UserId,
                Name = home.Name,
                Price = home.Price,
                Size = home.Size,
                Description = home.Description,
                Bathroom = home.Bathroom,
                Bedrooms = home.Bedrooms,

                // Map Location
                Location = home.Location != null ? new LocationModel
                {
                    LocationId = home.Location.LocationId,
                    Province = home.Location.Province,
                    District = home.Location.District,
                    Town = home.Location.Town,
                    Street = home.Location.Street,
                    HouseNumber = home.Location.HouseNumber
                } : null,

                // Map Utilities
                Utilities = home.Utilities != null ? new UtilitiesModel
                {
                    UtilitiesId = home.Utilities.UtilitiesId,
                    Elevator = home.Utilities.Elevator,
                    SwimmingPool = home.Utilities.SwimmingPool,
                    Gym = home.Utilities.Gym,
                    TV = home.Utilities.TV,
                    Refrigerator = home.Utilities.Refrigerator,
                    Parking = home.Utilities.Parking,
                    Balcony = home.Utilities.Balcony,
                    AirConditioner = home.Utilities.AirConditioner
                } : null,

                // Map Home Images
                HomeImages = home.HomeImages.Select(hi => new HomeImageModel
                {
                    HomeImageId = hi.HomeImageId,
                    HomeId = hi.HomeId,
                    Image = new ImageModel
                    {
                        ImageId = hi.Image.ImageId,
                        ImageUrl = hi.Image.ImageUrl
                    },
                    ImageDescription = hi.ImageDescription
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
                .Include(h => h.Location) // Include Location
                .Include(h => h.Utilities) // Include Utilities
                .FirstOrDefaultAsync(h => h.HomeId == id);

            if (home is not null)
            {
                // Map to HomeResponse
                var homeResponse = new HomeResonse
                {
                    HomeId = home.HomeId,
                    UserId = home.UserId,
                    Name = home.Name,
                    Price = home.Price,
                    Size = home.Size,
                    Description = home.Description,
                    Bathroom = home.Bathroom,
                    Bedrooms = home.Bedrooms,

                    // Map Location
                    Location = home.Location != null ? new LocationModel
                    {
                        LocationId = home.Location.LocationId,
                        Province = home.Location.Province,
                        District = home.Location.District,
                        Town = home.Location.Town,
                        Street = home.Location.Street,
                        HouseNumber = home.Location.HouseNumber
                    } : null,

                    // Map Utilities
                    Utilities = home.Utilities != null ? new UtilitiesModel
                    {
                        UtilitiesId = home.Utilities.UtilitiesId,
                        Elevator = home.Utilities.Elevator,
                        SwimmingPool = home.Utilities.SwimmingPool,
                        Gym = home.Utilities.Gym,
                        TV = home.Utilities.TV,
                        Refrigerator = home.Utilities.Refrigerator,
                        Parking = home.Utilities.Parking,
                        Balcony = home.Utilities.Balcony,
                        AirConditioner = home.Utilities.AirConditioner
                    } : null,

                    // Map Home Images
                    HomeImages = home.HomeImages.Select(hi => new HomeImageModel
                    {
                        HomeImageId = hi.HomeImageId,
                        HomeId = hi.HomeId,
                        Image = new ImageModel
                        {
                            ImageId = hi.Image.ImageId,
                            ImageUrl = hi.Image.ImageUrl
                        },
                        ImageDescription = hi.ImageDescription
                    }).ToList()
                };

                return homeResponse;
            }

            return null; // Or throw an exception if preferred
        }


        // Create a new home
        public async Task<HomeModel> CreateNewHome(HomeModel newHome, List<IFormFile> imageFiles, int userId)
        {
            var homeEntity = _mapper.Map<Home>(newHome);
            homeEntity.UserId = userId; // Assign the UserId to the home entity
            // Create new Location entity
            var locationEntity = new Location
            {
                Province = newHome.Province, // Assuming these properties exist in HomeModel
                District = newHome.District,
                Town = newHome.Town,
                Street = newHome.Street,
                HouseNumber = newHome.HouseNumber
            };

            // Create new Utilities entity from user input
            var utilitiesEntity = new Utilities
            {
                Elevator = newHome.Elevator,
                SwimmingPool = newHome.SwimmingPool,
                Gym = newHome.Gym,
                TV = newHome.TV,
                Refrigerator = newHome.Refrigerator,
                Parking = newHome.Parking,
                Balcony = newHome.Balcony,
                AirConditioner = newHome.AirConditioner
            };

            try
            {
                // Save Location
                await _locationRepository.AddAsync(locationEntity);
                var locationResult = await _locationRepository.Commit();
                if (locationResult <= 0)
                {
                    throw new Exception("Failed to save the location.");
                }

                // Save Utilities
                await _utilitiesRepository.AddAsync(utilitiesEntity);
                var utilitiesResult = await _utilitiesRepository.Commit();
                if (utilitiesResult <= 0)
                {
                    throw new Exception("Failed to save the utilities.");
                }

                // Assign IDs to HomeEntity
                homeEntity.LocationId = locationEntity.LocationId; // Get the ID of the newly created location
                homeEntity.UtilitiesId = utilitiesEntity.UtilitiesId; // Get the ID of the newly created utilities
                homeEntity.UserId = userId; // Assign the UserId to the home entity

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
                    throw new Exception("Failed to save the home entity.");
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it accordingly
                throw new Exception("An error occurred while creating a new home: " + ex.Message);
            }
        }




        // Update an existing home and its images
        public async Task<HomeModel> UpdateHome(int homeId, UpdateHomeRequest req)
        {
            try
            {
                // Fetch the home with its associated images, location, and utilities
                var homeEntity = await _homeRepository
                    .GetAll()
                    .Include(h => h.HomeImages)
                    .ThenInclude(hi => hi.Image)
                    .Include(h => h.Location) // Include Location for updates
                    .Include(h => h.Utilities) // Include Utilities for updates
                    .FirstOrDefaultAsync(h => h.HomeId == homeId);

                if (homeEntity != null)
                {
                    // Update basic home info
                    if (!string.IsNullOrEmpty(req.Name)) homeEntity.Name = req.Name;
                    if (!string.IsNullOrEmpty( req.Price)) homeEntity.Price = req.Price;
                    if (!string.IsNullOrEmpty(req.Size)) homeEntity.Size = req.Size;
                    if (!string.IsNullOrEmpty(req.Description)) homeEntity.Description = req.Description;
                    if (req.Bathroom.HasValue) homeEntity.Bathroom = req.Bathroom.Value;
                    if (req.Bedrooms.HasValue) homeEntity.Bedrooms = req.Bedrooms.Value;

                    // Update Location properties if LocationId is provided
                    if (homeEntity.Location != null)
                    {
                        // Update location properties from request
                        if (!string.IsNullOrEmpty(req.Province)) homeEntity.Location.Province = req.Province;
                        if (!string.IsNullOrEmpty(req.District)) homeEntity.Location.District = req.District;
                        if (!string.IsNullOrEmpty(req.Town)) homeEntity.Location.Town = req.Town;
                        if (!string.IsNullOrEmpty(req.Street)) homeEntity.Location.Street = req.Street;
                        if (!string.IsNullOrEmpty(req.HouseNumber)) homeEntity.Location.HouseNumber = req.HouseNumber;
                    }

                    // Update Utilities properties if UtilitiesId is provided
                    if (homeEntity.Utilities != null)
                    {
                        // Update utility properties from request
                        if (req.Elevator.HasValue) homeEntity.Utilities.Elevator = req.Elevator.Value;
                        if (req.SwimmingPool.HasValue) homeEntity.Utilities.SwimmingPool = req.SwimmingPool.Value;
                        if (req.Gym.HasValue) homeEntity.Utilities.Gym = req.Gym.Value;
                        if (req.TV.HasValue) homeEntity.Utilities.TV = req.TV.Value;
                        if (req.Refrigerator.HasValue) homeEntity.Utilities.Refrigerator = req.Refrigerator.Value;
                        if (req.Parking.HasValue) homeEntity.Utilities.Parking = req.Parking.Value;
                        if (req.Balcony.HasValue) homeEntity.Utilities.Balcony = req.Balcony.Value;
                        if (req.AirConditioner.HasValue) homeEntity.Utilities.AirConditioner = req.AirConditioner.Value;
                    }

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
                                    //existingImage.ImageDescription = updatedImage.ImageDescription;
                                    // Update additional image properties if needed
                                }
                            }
                            else
                            {
                                // Add a new image
                                var newHomeImage = new HomeImage
                                {
                                    ImageId = updatedImage.ImageId.Value, // Assuming ImageId is mandatory for new images
                                  //  ImageDescription = updatedImage.ImageDescription
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
                // Consider logging the exception or rethrowing a custom exception if needed
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


        // Get homes by user ID
        public async Task<IEnumerable<HomeResonse>> GetHomesByUserId(int userId)
        {
            var homes = await _homeRepository.GetAll()
                .Where(h => h.UserId == userId)
                .Include(h => h.HomeImages)
                .ThenInclude(hi => hi.Image)
                .Include(h => h.Location)
                .Include(h => h.Utilities)
                .ToListAsync();

            // Map to HomeResponse
            var homeResponses = homes.Select(home => new HomeResonse
            {
                HomeId = home.HomeId,
                UserId = home.UserId,
                Name = home.Name,
                Price = home.Price,
                Size = home.Size,
                Description = home.Description,
                Bathroom = home.Bathroom,
                Bedrooms = home.Bedrooms,

                // Map Location
                Location = home.Location != null ? new LocationModel
                {
                    LocationId = home.Location.LocationId,
                    Province = home.Location.Province,
                    District = home.Location.District,
                    Town = home.Location.Town,
                    Street = home.Location.Street,
                    HouseNumber = home.Location.HouseNumber
                } : null,

                // Map Utilities
                Utilities = home.Utilities != null ? new UtilitiesModel
                {
                    UtilitiesId = home.Utilities.UtilitiesId,
                    Elevator = home.Utilities.Elevator,
                    SwimmingPool = home.Utilities.SwimmingPool,
                    Gym = home.Utilities.Gym,
                    TV = home.Utilities.TV,
                    Refrigerator = home.Utilities.Refrigerator,
                    Parking = home.Utilities.Parking,
                    Balcony = home.Utilities.Balcony,
                    AirConditioner = home.Utilities.AirConditioner
                } : null,

                // Map Home Images
                HomeImages = home.HomeImages.Select(hi => new HomeImageModel
                {
                    HomeImageId = hi.HomeImageId,
                    HomeId = hi.HomeId,
                    Image = new ImageModel
                    {
                        ImageId = hi.Image.ImageId,
                        ImageUrl = hi.Image.ImageUrl
                    },
                    ImageDescription = hi.ImageDescription
                }).ToList()
            }).ToList();

            return homeResponses;
        }
    }

}
