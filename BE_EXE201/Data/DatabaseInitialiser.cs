using Microsoft.EntityFrameworkCore;
using BE_EXE201.Entities;
using BE_EXE201.Helpers;
using Microsoft.CodeAnalysis;
using Org.BouncyCastle.Utilities;
using Location = BE_EXE201.Entities.Location;

namespace BE_EXE201.Data
{
    public interface IDataaseInitialiser
    {
        Task InitialiseAsync();
        Task SeedAsync();
        Task TrySeedAsync();
    }

    public class DatabaseInitialiser : IDataaseInitialiser
    {
        public readonly AppDbContext _context;

        public DatabaseInitialiser(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                // Migration Database - Create database if it does not exist
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            // Check if the database is already seeded
            if (_context.UserRoles.Any() || _context.Homes.Any() || _context.Users.Any())
            {
                return; // Database already seeded
            }

            // Seed UserRoles
            var adminRole = new UserRole { RoleName = "Admin" };
            var ownerRole = new UserRole { RoleName = "Owner" };
            var tenantRole = new UserRole { RoleName = "Tenant" };

            List<UserRole> userRoles = new()
            {
                adminRole,ownerRole,tenantRole
            };
           
            // Seed Users directly
            var adminUser = new User
            {
                UserName = "admin",
                Password = SecurityUtil.Hash("123456"), // Adjust the password hashing as needed
                FullName = "Admin User",
                Email = "admin@gmail.com",
                Gender = "Male",
                Address = "123 Admin St, City, Country",
                BirthDate = new DateTime(1985, 1, 1),
                PhoneNumber = "1234567890",
                CreateDate = DateTime.UtcNow,
                Status = "Active",
                UserRole = adminRole,
                AvatarUrl = "",
                Wallet = 1000
            };

            var ownerUser = new User
            {
                UserName = "owner",
                Password = SecurityUtil.Hash("123456"),
                FullName = "Owner User",
                Email = "owner@gmail.com",
                Gender = "Female",
                Address = "456 Owner Ave, City, Country",
                BirthDate = new DateTime(1990, 6, 15),
                PhoneNumber = "0987654321",
                CreateDate = DateTime.UtcNow,
                Status = "Active",
                UserRole=ownerRole,
                AvatarUrl = "",
                Wallet = 500
            };

            var tenantUser = new User
            {
                UserName = "tenant",
                Password = SecurityUtil.Hash("123456"),
                FullName = "Tenant User",
                Email = "tenant@gmail.com",
                Gender = "Other",
                Address = "789 Tenant Rd, City, Country",
                BirthDate = new DateTime(1995, 9, 25),
                PhoneNumber = "1122334455",
                CreateDate = DateTime.UtcNow,
                Status = "Active",
                UserRole=tenantRole,
                AvatarUrl = "",
                Wallet = 200
            };

            List<User> users = new List<User>()
            {
                adminUser,ownerUser,tenantUser
            };
            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
            // List of locations
            List<Location> locations = new()
    {
        new Location
        {
            Province = "Hà Nội",//1
            District = "Quận Cầu Giấy",
            Town = "Phường Quan Hoa",
            Street = "Ngõ 58 Đường Nguyễn Khánh Toàn",
            HouseNumber = "26"
        },

      new Location
        {
            Province = "Hồ Chí Minh",//2
            District = "Quận 1",
            Town = "Phường Bến Nghé",
            Street = "Đường Nguyễn Huệ",
            HouseNumber = "20"
        },
       new Location 
        {
            Province = "Hà Nội",//3
            District = "Quận Long Biên",
            Town = "Phường Long Biên",
            Street = "Phố Trạm",
            HouseNumber = null // No specific house number provided
        },
       new Location 
        {
            Province = "Tp Hồ Chí Minh",//4
            District = "Quận 2",
            Town = "Phường Cát Lái",
            Street = "Thành phố Thủ Đức",
            HouseNumber = "22"
        },
       new Location // New location for homeId 5
        {
            Province = "Tp Hồ Chí Minh",
            District = "Quận Bình Thạnh",
            Town = "Phường 11",
            Street = "Hẻm 308 Lê Quang Định",
            HouseNumber = null 
        },
        new Location // New location for homeId //6
        {
            Province = "Hồ Chí Minh",
            District = "Quận 9",
            Town = "Phường Đông Tăng Long",
            Street = "Nguyễn Xiển", 
            HouseNumber = null // Specify house number if available
        },
        new Location // New location for homeId 7
        {
            Province = "Hồ Chí Minh",
            District = "Quận 4",
            Town = null,
            Street = "Nguyễn Tất Thành",
            HouseNumber = null
        },
        new Location // New location for homeId 8
        {
            Province = "Hồ Chí Minh",
            District = "Quận Gò Vấp",
            Town = "Phường 6",
            Street = "đường số 30",
            HouseNumber = null
        },
        new Location // New location for homeId 9
        {
            Province = "Hồ Chí Minh",
            District = "Quận Gò Vấp",
            Town = "Phường 18",
            Street = "đường số 2",
            HouseNumber = null
        },
         new Location // New location for homeId 10
        {
            Province = "Hồ Chí Minh",
            District = "Quận 7",
            Town = "Phường Tân Thuận Đông",
            Street = "Đường Bùi Văn Ba",
            HouseNumber = null
        }
    };


            // List of utilities
            var utilities = new List<Utilities>
    {
        new Utilities
        {
            Elevator = true,
            SwimmingPool = true,
            Gym = true,
            TV = false,
            Refrigerator = false,
            Parking = true,
            Balcony = true,
            AirConditioner = false
        },
        new Utilities
        {
            Elevator = true,
            SwimmingPool = false,
            Gym = false,
            TV = true,
            Refrigerator = true,
            Parking = false,
            Balcony = false,
            AirConditioner = true
        },
         new Utilities
        {
            Elevator = false,
            SwimmingPool = false,
            Gym = false,
            TV = true,
            Refrigerator = true,
            Parking = false,
            Balcony = true,
            AirConditioner = true
        },
         new Utilities
        {
            Elevator = false,
            SwimmingPool = false,
            Gym = false,
            TV = true,
            Refrigerator = true,
            Parking = false,
            Balcony = true,
            AirConditioner = true
        },
          new Utilities
        {
            Elevator = true,
            SwimmingPool = false,
            Gym = false,
            TV = true,
            Refrigerator = true,
            Parking = false,
            Balcony = true,
            AirConditioner = true
        }, new Utilities
        {
            Elevator = true,
            SwimmingPool = false,
            Gym = false,
            TV = true,
            Refrigerator = true,
            Parking = false,
            Balcony = true,
            AirConditioner = true
        }, new Utilities
        {
            Elevator = true,
            SwimmingPool = false,
            Gym = false,
            TV = true,
            Refrigerator = true,
            Parking = false,
            Balcony = true,
            AirConditioner = true
        },
            new Utilities
        {
            Elevator = true,
            SwimmingPool = false,
            Gym = false,
            TV = false,
            Refrigerator = true,
            Parking = false,
            Balcony = false,
            AirConditioner = true
        },
            new Utilities
        {
            Elevator = true,
            SwimmingPool = false,
            Gym = false,
            TV = false,
            Refrigerator = true,
            Parking = false,
            Balcony = false,
            AirConditioner = true
        },
            new Utilities
        {
            Elevator = true,
            SwimmingPool = false,
            Gym = false,
            TV = false,
            Refrigerator = true,
            Parking = false,
            Balcony = false,
            AirConditioner = true
        }

    };


            // List of homes
            var homes = new List<Home>
    {
        new Home
        {
            Name = "Căn hộ DV full đồ 1K1N 40m2 phố Nguyễn Khách Toàn",
            UserId = ownerUser.UserId,
            Price = "6.5 triệu",
            Size = " 80 m2",
            Description = "Tiện ích: có khoá cổng vân tay, giờ giấc tự do. co máy giặt chung NỘI THẤT: máy lạnh, kệ bếp, kệ chén, bàn bếp",
            Bathroom = 1,
            Bedrooms = 1,
            Location = locations[0],  // Reference to location
            Utilities = utilities[0]  // Reference to utilities
        },
        new Home
        {
            Name = "Căn hộ 2 phòng ngủ 80m2 tại Quận 1",
           UserId = ownerUser.UserId,
            Price = "4,2 triệu",
            Size = "80 m2",
            Description = "Tiện ích: có khoá cổng vân tay, giờ giấc tự do. co máy giặt chung NỘI THẤT: máy lạnh, kệ bếp, kệ chén, bàn bếp",
            Bathroom = 2,
            Bedrooms = 2,
            Location = locations[1],
            Utilities = utilities[1]
        },
        new Home // New homeId 3
        {
            Name = "CHO THUÊ NHÀ NGUYÊN CĂN 4 X 20 M -KC 3 LẦU -AN PHÚ -27 triệu",
            UserId = ownerUser.UserId,
            Price = "20 triệu",
            Size = "80 m2" ,
            Description = "- Kết cấu: trệt + 2 lầu + sân thượng. - 5 phòng làm việc, 5WC, phòng trống suốt.. 4 điều hòa. - đường rộng 18m Vỉa hè rộng. Vỉa hè tận dụng để xe thoải mái.  Con đường sầm uất Quận 2. Đường rộng 20 m.",
            Bathroom = 2,
            Bedrooms = 2,
            Location = locations[2], // Reference to the new location
            Utilities = utilities[2] // No specific utilities provided
        },
        new Home // New homeId 4
        {
            Name = "PHÒNG TRỌ ĐIỀU HOÀ FULL NỘI THẤT",
           UserId = ownerUser.UserId,
            Price = "1,5 triệu" , 
            Size = "20 m2" ,
            Description = "- Kết cấu: trệt + 2 lầu + sân thượng. - 5 phòng làm việc, 5WC, phòng trống suốt.. 4 điều hòa. - đường rộng 18m Vỉa hè rộng. Vỉa hè tận dụng để xe thoải mái.  Con đường sầm uất Quận 2. Đường rộng 20 m.",
            Bathroom = 1,
            Bedrooms = 1,
            Location = locations[3], // Reference to the new location
            Utilities = utilities[3]// No specific utilities provided
        },
        new Home
        {
            Name = "Phòng trọ giá rẻ quận 2 đẹp như khách sạn chỉ 3,9 triệu",
           UserId = ownerUser.UserId,
            Price = "3,9 triệu",
            Size = "30 m2 ",
            Description = "- Gần cảng Cát Lái chợ xóm mới gần trường UMT bán kính 500m. - Khu dân cư đông đúc. - Giờ giấc tự do - Camera và khoá từ - Q2 siêu thị Emart 3km. - chợ dân sinh cách 300m - siêu thị 3 cái khu ăn uống, quán ăn, quán nhậu",
            Bathroom = 1,
            Bedrooms = 1,
            Location = locations[4], // Reference to the new location for homeId 5
            Utilities = utilities[4]
        },
         new Home // New homeId 6
        {
            Name = "Phòng Thật 100%, Ban Công mát, Tại KđT Đông Tăng Long,Gần Vinhome,FPT ",
          UserId = ownerUser.UserId,
            Price = "3,7 triệu",
            Size = "35 m2",
            Description = "Ban công thoáng mát + Gần khu Công Nghệ Cao, các trường Đại Học. + khép kín tiện lợi: wc, bếp, đèn điện, quạt trần, cửa sổ ánh sáng, thoáng mát + Miễn phí: quản lý, bảo trì + An ninh, văn minh + Đồng hồ điện nước riêng + Điện 3.8k/số, nước 20k/số",
            Bathroom = 1,
            Bedrooms = 1,
            Location = locations[5], // Reference to the new location for homeId 6
            Utilities = utilities[5] // No specific utilities provided
        },
          new Home // New homeId 7
        {
            Name = "Ở ĐƯỢC 3-4 BẠN GẦN TRƯỜNG NTTU VÀ ĐẠI HỌC UFM ,NGUYỄN TẤT THÀNH QUẬN 4",
            UserId = ownerUser.UserId,
            Price = "4,9 triệu",
            Size = "40 m2" ,
            Description = "• Gần Trục đường lớn , Tôn Thất Huyết , Nguyễn Tất Thành , Tiện Qua Quận 1 và Trung Tâm Sài Gòn  • Full nội thất • Diện tích rộng • Thang máy tiện nghi • Giờ giấc tự do • Ra vào vân tay • Hầm xe rộng\"",
            Bathroom = 1,
            Bedrooms = 1,
            Location = locations[6], // Reference to the new location for homeId 7
            Utilities = utilities[6]
        },
           new Home // New homeId 8
        {
            Name = "TRỌ GÁC RỘNG 30M2 MỚI XÂY Ở ĐƯỜNG SỐ 30",
            UserId = ownerUser.UserId,
            Price = "3,3 triệu" ,
            Size = "30 m2" ,
            Description = "Tiện ích: có khoá cổng vân tay, giờ giấc tự do. co máy giặt chung NỘI THẤT: máy lạnh, kệ bếp, kệ chén, bàn bếp CHI PHÍ: điện 3k số nước 100 người dịch vụ 150K phòng",
            Bathroom = 1,
            Bedrooms = 1,
            Location =  locations[7], // Set to null as locationId is not specified
            Utilities = utilities[7] // Set to null as utilitiesId is not specified
        },
            new Home// New homeId 9
        {
            Name = "PHÒNG TRỌ MỚI XÂY Ở DƯỜNG SỐ 21 ( THUỘC NHÁNH QUANG TRUNG)",
            UserId = ownerUser.UserId,
            Price = "3 triệu",
            Size = "15 m2",
            Description = "Tiện ích toà nhà: có thang máy, camera an ninh, máy giặt chung, giờ giấc tự do khoá cổng vân tay, ko chung chủ ko giới hạn người ở nội thất: có máy lạnh mới. kệ bếp, bồn rửa mặt... chi phí: + điện 3k5/số + nước 100k/ng + dịch vụ 150k/ng\"",
            Bathroom = 1,
            Bedrooms = 1,
            Location =  locations[8], 
            Utilities = utilities[8]
        },
            new Home
        {
            
            Name = "CHDV Lê Quang Định, Bình Thạnh, giáp Phú Nhuận, Q1",
            UserId = ownerUser.UserId,
            Price = "7 triệu",
            Size = "25 m2" ,
            Description = "• Gác cao k đụng đầu • Hầm xe thang máy tiện nghi • Ra vào vân tay giờ giấc tự do • Hầm xe rộng , bảo vệ 24/7 • Đạt chuẩn PCCC",
            Bathroom = 1,
            Bedrooms = 1,
            Location =  locations[9],
            Utilities = utilities[9]
        }

    };


            // List of images
            var images = new List<Image>
    {
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728546242/zglupgu2mocwga25fb8n.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728546244/pxn18j8z8nckjhvu5xfh.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728546246/dosboh4t4ean0c818any.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550453/rx50g6umi0rlnerzzwch.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550453/rx50g6umi0rlnerzzwch.png" }, // Image for homeId 3
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550456/j5rolk0eadzuyjkw4ts6.png" }, // Image for homeId 3
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550458/liwsyepc8ntzc1mkwecz.png" }, // Image for homeId 3
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550461/t3e3qespba1r2bj9lda4.png" }, // Image for homeId 3
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550739/zlybi7wkv4pewnrqh62f.png" }, // Image for homeId 4
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550742/hnnvzgpfmo0lmwqhya7d.png" }, // Image for homeId 4
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550744/udzouoiw1zgcaaofsu1t.png" }, // Image for homeId 4
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728550745/lo5pirxgqypvciggkxyj.png" }, // Image for homeId 4
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728551169/gfflbt45o25e37tflz08.png" }, // Image for homeId 5
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728551170/ah7y1jowro9upo32iuyw.png" }, // Image for homeId 5
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728551171/jsn0a3sgoszp1ojavnny.png" }, // Image for homeId 5
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728551173/ruwlz1mtc0si8rt3kcqj.png" }, // Image for homeId 5
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728551687/oqztx3497scppforxsqa.png" }, // Image for homeId 6
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728551688/asqrgrjwzgypd8a0ozoe.png" }, // Image for homeId 6
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728551689/l4lmcvednkvruxn9gzmy.png" }, // Image for homeId 6
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728551690/x41iaagjw6cwts7m8x5l.png" }, // Image for homeId 6
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728552049/x3bekgyfwe6v0d7n6wkt.png" }, // Image for homeId 7
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728552049/jai5jnskynw7hm1kzzcz.png" }, // Image for homeId 7
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728552050/n7ewiymcqyhol1dq9sp2.png" }, // Image for homeId 7
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728552051/ikmpgx30ds5euz4rgxl2.png" }, // Image for homeId 7
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728552582/ridep52xi9eh4nmeh7hp.png" }, // Image for homeId 8
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728552585/ql4hdqtwloviaohc7iu5.png" }, // Image for homeId 8
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728552586/jbbmxt1krmsvdpa2mymn.png" }, // Image for homeId 8
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728552588/ptic9xezoqvlomacqwg9.png" }, // Image for homeId 8
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728553105/pinsj9ll0hl0tymiebpm.png" }, // Image for homeId 9
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728553105/f89vpmihibld8p2ujjzg.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728553106/tlduvhvfwvnbt9ujqibl.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728553107/ugd97mhkicggcribbgtb.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728553454/ib3hr4rkwy42wkcwmavv.png" }, // Image for homeId 10
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728553456/wmrhfydescabrzhylxex.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728553458/yulll8ugxetgpesogy2y.png" },
        new Image { ImageUrl = "https://res.cloudinary.com/dnwqhc5th/image/upload/v1728553459/hxegru4kjnvm5flcdsps.png" }





    };


            // List of HomeImages
            var homeImages = new List<HomeImage>
    {
        new HomeImage { Home = homes[0], Image = images[0] },
        new HomeImage { Home = homes[0], Image = images[1] },
        new HomeImage { Home = homes[1], Image = images[2] },
        new HomeImage { Home = homes[1], Image = images[3] },
        new HomeImage { Home = homes[2], Image = images[4] }, // New images for homeId 3
        new HomeImage { Home = homes[2], Image = images[5] },
        new HomeImage { Home = homes[2], Image = images[6] },
        new HomeImage { Home = homes[2], Image = images[7] },
        new HomeImage { Home = homes[3], Image = images[8] }, // New images for homeId 4
        new HomeImage { Home = homes[3], Image = images[9] },
        new HomeImage { Home = homes[3], Image = images[10] },
        new HomeImage { Home = homes[3], Image = images[11] },
        new HomeImage { Home = homes[4], Image = images[12] },// New images for homeId 5
        new HomeImage { Home = homes[4], Image = images[13] },
        new HomeImage { Home = homes[4], Image = images[14] },
        new HomeImage { Home = homes[4], Image = images[15] },
        new HomeImage { Home = homes[5], Image = images[16] },// New images for homeId 6
        new HomeImage { Home = homes[5], Image = images[17] },
        new HomeImage { Home = homes[5], Image = images[18] },
        new HomeImage { Home = homes[5], Image = images[19] },
        new HomeImage { Home = homes[6], Image = images[20] },// New images for homeId 7
        new HomeImage { Home = homes[6], Image = images[21] },
        new HomeImage { Home = homes[6], Image = images[22] },
        new HomeImage { Home = homes[6], Image = images[23] },
        new HomeImage { Home = homes[7], Image = images[24] },// New images for homeId 8
        new HomeImage { Home = homes[7], Image = images[25] },
        new HomeImage { Home = homes[7], Image = images[26] },
        new HomeImage { Home = homes[7], Image = images[27] },
        new HomeImage { Home = homes[8], Image = images[28] },// New images for homeId 9
        new HomeImage { Home = homes[8], Image = images[29] },
        new HomeImage { Home = homes[8], Image = images[30] },
        new HomeImage { Home = homes[8], Image = images[31] },
        new HomeImage { Home = homes[9], Image = images[32] },// New images for homeId 10
        new HomeImage { Home = homes[9], Image = images[33] },
        new HomeImage { Home = homes[9], Image = images[34] },
        new HomeImage { Home = homes[9], Image = images[35] },

    };


            // Save all changes to the database

           // await _context.UserRoles.AddRangeAsync(userRoles);

            //await _context.Users.AddRangeAsync(users);

            await _context.Locations.AddRangeAsync(locations);

            await _context.Utilities.AddRangeAsync(utilities);

            await _context.Homes.AddRangeAsync(homes);

            await _context.Images.AddRangeAsync(images);

            await _context.HomeImages.AddRangeAsync(homeImages);

            await _context.SaveChangesAsync();
        }
    }
        public static class DatabaseInitialiserExtension
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            // Create IServiceScope to resolve service scope
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitialiser>();

            await initializer.InitialiseAsync();

            // Try to seeding data
            await initializer.SeedAsync();
        }
    }
}