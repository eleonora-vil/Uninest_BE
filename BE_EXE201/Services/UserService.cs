using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Dtos;
using BE_EXE201.Entities;
using BE_EXE201.Exceptions;
using BE_EXE201.Helpers;
using BE_EXE201.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BE_EXE201.Dtos.Payment;
using BE_EXE201.Dtos.User;

namespace BE_EXE201.Services
{
    public class UserService
    {
        private readonly IRepository<User, int> _userRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<UserRole, int> _userRoleRepository;
        private readonly EmailService _emailService;
        private readonly IRepository<PaymentTransaction, int> _paymentTransactionRepository;
        private readonly CloudService _cloudService;
        private readonly AppDbContext _dbContext;

        private const decimal MembershipFee = 200000;


        public UserService(
            IRepository<User, int> userRepository,
            IMapper mapper,
            IRepository<UserRole, int> userRoleRepository,
            EmailService emailService,
            IRepository<PaymentTransaction, int> paymentTransactionRepository,
            CloudService cloudService,
            AppDbContext dbContext)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userRoleRepository = userRoleRepository;
            _emailService = emailService;
            _paymentTransactionRepository = paymentTransactionRepository;
            _cloudService = cloudService;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            var result = _userRepository.GetAll().ToList();
            return _mapper.Map<List<UserModel>>(result);
        }
        public async Task<UserModel> CreateNewUser(UserModel newUser)
        {
            var userEntity = _mapper.Map<User>(newUser);

            if (await _userRepository.FindByCondition(x => x.Email == newUser.Email).AnyAsync())
            {
                throw new BadRequestException("Email already exists");
            }

            var userRoleEntity = await _userRoleRepository.FindByCondition(ur => ur.RoleId == newUser.RoleID).FirstOrDefaultAsync();
            if (userRoleEntity == null)
            {
                throw new BadRequestException("Invalid role ID");
            }
            userEntity.UserRole = userRoleEntity;

            userEntity.AvatarUrl = GravatarHelper.GetGravatarUrl(newUser.Email);

            await _userRepository.AddAsync(userEntity);
            var result = await _userRepository.Commit();

            if (result <= 0)
            {
                throw new InvalidOperationException("Failed to create new user.");
            }

            var latestUserId = await _userRepository.FindByCondition(x => true)
                                                    .OrderByDescending(x => x.UserId)
                                                    .Select(x => x.UserId)
                                                    .FirstOrDefaultAsync();
            newUser.UserId = latestUserId;

            return newUser;
        }




        public async Task<UserModel> GetUserById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            var userEntity = _mapper.Map<UserModel>(user);
            if (user is not null)
            {
                return userEntity;
            }
            return null;
        }



        public async Task<string> UpdateUserImageAsync(int userId, string requesterEmail, IFormFile image)
        {
            // Check if the requester has permission to update this user's image
            var requester = await _userRepository.FindByCondition(u => u.Email == requesterEmail).FirstOrDefaultAsync();
            if (requester == null)
            {
                return "Requester not found";
            }

            // Find the user by userId
            var user = await _userRepository.FindByCondition(u => u.UserId == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return "User not found";
            }

            // Upload the image to the cloud service
            var uploadResult = await _cloudService.UploadImageAsync(image);
            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.Url.ToString()))
            {
                return "Failed to upload image.";
            }

            // Update the user's avatar URL and save to the database
            user.AvatarUrl = uploadResult.Url.ToString();
            _userRepository.Update(user);
            var updateResult = await _userRepository.Commit();

            if (updateResult <= 0)
            {
                return "Failed to update user image.";
            }

            // Return null to indicate success
            return null;
        }



        public async Task<UserModel> UpdateUser(UserModel existingUser, UpdateUserRequest req)
        {
            try
            {
                var userEntity = await _userRepository.FindByCondition(x => x.Email == existingUser.Email).FirstOrDefaultAsync();

                if (userEntity != null)
                {
                    // Update user properties
                    if (!string.IsNullOrEmpty(req.UserName))
                    {
                        userEntity.UserName = req.UserName;
                    }
                    if (!string.IsNullOrEmpty(req.FullName))
                    {
                        userEntity.FullName = req.FullName;
                    }
                    if (!string.IsNullOrEmpty(req.Gender))
                    {
                        userEntity.Gender = req.Gender;
                    }
                    if (!string.IsNullOrEmpty(req.Address))
                    {
                        userEntity.Address = req.Address;
                    }
                    if (req.BirthDate.HasValue)
                    {
                        userEntity.BirthDate = req.BirthDate.Value;
                    }
                    if (!string.IsNullOrEmpty(req.PhoneNumber))
                    {
                        userEntity.PhoneNumber = req.PhoneNumber;
                    }
                    /*if (req.IsActiveMember.HasValue)
                    {
                        userEntity.IsActiveMember = req.IsActiveMember.Value;
                    }*/
                    if (req.IsMember.HasValue)
                    {
                        userEntity.IsMember = req.IsMember.Value;
                    }
                    if (req.Wallet is not null)
                    {
                        userEntity.Wallet = req.Wallet.Value;
                    }



                    _userRepository.Update(userEntity);
                    var result = await _userRepository.Commit();

                    if (result > 0)
                    {
                        return _mapper.Map<UserModel>(userEntity);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserWalletAfterPosted(UserModel user, decimal amount)
        {
            try
            {
                var existedUser = await _userRepository.FindByCondition(x => x.UserId == user.UserId || x.Email == user.Email).FirstOrDefaultAsync();

                if (existedUser != null)
                {
                    // Cập nhật wallet
                    existedUser.Wallet -= amount;

                    // Cập nhật thông tin người dùng
                    _userRepository.Update(existedUser);

                    var result = await _userRepository.Commit();

                    return result > 0;
                }
                else
                {
                    return false; // Người dùng không tồn tại
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user password: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserPassword(UserModel user, string newPassword)
        {
            try
            {
                var existedUser = await _userRepository.FindByCondition(x => x.UserId == user.UserId || x.Email == user.Email).FirstOrDefaultAsync();

                if (existedUser != null)
                {
                    existedUser.Password = SecurityUtil.Hash(newPassword);
                    _userRepository.Update(existedUser);
                    var result = await _userRepository.Commit();
                    return result > 0;
                }
                else
                {
                    return false; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user password: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> SendPasswordResetEmail(string email, string fullName, string newPassword)
        {
            var mailData = new MailData()
            {
                EmailToId = email,
                EmailToName = fullName,
                EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Password Reset</h2>
    <p>Your new password is: <span style=""font-weight: bold; color: #e74c3c;"">{newPassword}</span></p>
    <p>Please log in and change your password.</p>
    <p style=""color: #888; font-size: 14px;"">Powered by UniNest</p>
</div>",
                EmailSubject = "Password Reset"
            };

            return await _emailService.SendEmailAsync(mailData);
        }


        public async Task<UserModel> GetUserByEmail(string email)
        {
            return _mapper.Map<UserModel>(_userRepository.FindByCondition(x => x.Email == email).FirstOrDefault());
        }

        public async Task<UserModel> GetUserInToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new BadRequestException("Authorization header is missing or invalid.");
            }
            // Decode the JWT token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check if the token is expired
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                throw new BadRequestException("Token has expired.");
            }

            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            var user = await _userRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();
            if (user is null)
            {
                throw new BadRequestException("Can not found User");
            }
            return _mapper.Map<UserModel>(user);
        }




        public async Task<bool> SubmitOTP(SubmitOTPResquest req)
        {
            var existedUser = await _userRepository.FindByCondition(x => x.Email == req.Email).FirstOrDefaultAsync();

            if (existedUser == null)
            {
                throw new BadRequestException("Account does not exist");
            }

            if (req.OTP != existedUser.OTPCode)
            {
                throw new BadRequestException("OTP Code is not correct");
            }

            existedUser.Status = "Active";

            existedUser.OTPCode = "0"; // Đặt lại mã OTP thành "0" sau khi xác thực|
            _userRepository.Update(existedUser);
            var result = await _userRepository.Commit();

            return result > 0;
            /*   if (existedUser.CreateDate > DateTime.Now.AddHours(-1))
               {
                   existedUser.Status = "Active";

                   existedUser.OTPCode = "0"; // Đặt lại mã OTP thành "0" sau khi xác thực|
                   _userRepository.Update(existedUser);
                   var result = await _userRepository.Commit();

                   return result > 0; 
               }
               else
               {
                   throw new BadRequestException("OTP code is expired");
               }*/
        }

        public async Task<User?> GetUserByOrderId(string orderId)
        {
            var transaction = _paymentTransactionRepository
                .FindByCondition(pt => pt.TransactionId == orderId)
                .FirstOrDefault();

            if (transaction is not null)
            {
                return await _userRepository.GetByIdAsync(transaction.UserId);
            }
            return null;
        }


        public async Task UpdateUserWallet(User user, VnPaymentResponseModel paymentResponse)
        {
            // Increase user's wallet balance by payment amount
            user.Wallet += paymentResponse.Amount; // Ensure Amount is decimal
            _userRepository.Update(user);

            // Retrieve the existing payment transaction based on OrderId
            var existingTransaction = await _paymentTransactionRepository
                .FindByCondition(pt => pt.TransactionId == paymentResponse.OrderId)
                .FirstOrDefaultAsync();

            if (existingTransaction != null)
            {
                // Update the existing transaction details
                existingTransaction.TransactionId = paymentResponse.TransactionId;
                existingTransaction.Amount = (decimal)paymentResponse.Amount;
                existingTransaction.Status = "Success";
                existingTransaction.UpdatedDate = DateTime.Now; // If you track updated dates
            }
            else
            {
                throw new TransactionNotFoundException($"Transaction with OrderId {paymentResponse.OrderId} not found.");
            }

            // Commit both changes
            await _userRepository.Commit(); // Commit user wallet update
            await _paymentTransactionRepository.Commit(); // Commit transaction update
        }


        public async Task<bool> ChangePassword(UserModel user, string currentPassword, string newPassword)
        {
            try
            {
                var existedUser = await _userRepository.FindByCondition(x => x.UserId == user.UserId || x.Email == user.Email).FirstOrDefaultAsync();

                if (existedUser != null)
                {
                    // Verify current password
                    if (!SecurityUtil.VerifyHash(currentPassword, existedUser.Password))
                    {
                        throw new BadRequestException("Current password is incorrect");
                    }

                    // Hash and set new password
                    existedUser.Password = SecurityUtil.Hash(newPassword);

                    // Update user information
                    _userRepository.Update(existedUser);

                    var result = await _userRepository.Commit();

                    return result > 0;
                }
                else
                {
                    return false; // User doesn't exist
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing password: {ex.Message}");
                throw;
            }
        }
        public async Task<UserModel> GetUserProfile(string email)
        {
            var user = await _userRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();
            return _mapper.Map<UserModel>(user);
        }

        public async Task<User> RegisterMembershipAsync(int userId, bool autoRenew)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found");
                }

                if (user.IsActiveMember)
                {
                    throw new InvalidOperationException("User is already an active member");
                }

                // Check if the user has sufficient funds in their wallet
                if (user.Wallet < MembershipFee)
                {
                    throw new InvalidOperationException($"Insufficient funds in wallet. Required: {MembershipFee}, Available: {user.Wallet}");
                }

                user.IsMember = true;
                user.MembershipStartDate = DateTime.UtcNow;
                user.MembershipEndDate = DateTime.UtcNow.AddMonths(3);
                user.AutoRenewMembership = autoRenew;
                user.Wallet -= MembershipFee;

                _userRepository.Update(user);
                await _userRepository.Commit();

                await transaction.CommitAsync();

                return user;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

}


