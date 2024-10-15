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

namespace BE_EXE201.Services
{
    public class UserService
    {
        private readonly IRepository<User, int> _userRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<UserRole, int> _userRoleRepository;
        private readonly EmailService _emailService;
        private readonly IRepository<PaymentTransaction, int> _paymentTransactionRepository;

        public UserService(
            IRepository<User, int> userRepository,
            IMapper mapper,
            IRepository<UserRole, int> userRoleRepository,
            EmailService emailService,
            IRepository<PaymentTransaction, int> paymentTransactionRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userRoleRepository = userRoleRepository;
            _emailService = emailService;
            _paymentTransactionRepository = paymentTransactionRepository;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            var result = _userRepository.GetAll().ToList();
            return _mapper.Map<List<UserModel>>(result);
        }
        public async Task<UserModel> CreateNewUser(UserModel newUser)
        {
            var userEntity = _mapper.Map<User>(newUser);
            var existedUser = _userRepository.FindByCondition(x => x.Email == newUser.Email).FirstOrDefault();
            if (existedUser is not null)
            {
                throw new BadRequestException("email already exist");
            }
            var userRoleEntity = _userRoleRepository.FindByCondition(ur => ur.RoleId == newUser.RoleID).FirstOrDefault();

            userEntity.UserRole = userRoleEntity!;

            userEntity.AvatarUrl = GravatarHelper.GetGravatarUrl(newUser.Email);

            await _userRepository.AddAsync(userEntity);
            var result = await _userRepository.Commit();
            if (result > 0)
            {
                // get latest userID
                newUser.UserId = _userRepository.GetAll().ToList().Max(x => x.UserId);
                return newUser;
            }
            else
            {
                return null;
            }
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
        public async Task<bool> UpdateUserPassword(UserModel user, string newPassword)
        {
            try
            {
                var existedUser = await _userRepository.FindByCondition(x => x.UserId == user.UserId || x.Email == user.Email).FirstOrDefaultAsync();

                if (existedUser != null)
                {
                    // Mã hóa mật khẩu mới
                    existedUser.Password = SecurityUtil.Hash(newPassword);

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

        /*  public async Task<CreateUserModel> FirstStep(CreateUserModel req)
          {
              var userEntity = _mapper.Map<User>(req);
              var user = _userRepository.FindByCondition(x => x.Email == req.Email).FirstOrDefault();

              if (user != null && user.OTPCode != null)
              {
                  user.RoleID = 4;
                  user.Status = "InActive";
                  user.CreateDate = DateTime.Now.AddMinutes(2);
                  user.OTPCode = new Random().Next(100000, 999999).ToString();

                  user = _userRepository.Update(user);
                  int rs = await _userRepository.Commit();
                  if (rs > 0)
                  {
                      return _mapper.Map<CreateUserModel>(user);
                  }
                  else
                  {
                      return null;
                  }
              }
              userEntity.RoleID = 2;
              userEntity.Status = "InActive";
              userEntity.CreateDate = DateTime.Now.AddMinutes(2);
              userEntity.Password = SecurityUtil.Hash(req.Password);
              var existedUser = _userRepository.FindByCondition(x => x.Email == req.Email).FirstOrDefault();
              if (existedUser != null)
              {
                  throw new BadRequestException("email already exist");
              }
              userEntity = await _userRepository.AddAsync(userEntity);
              int result = await _userRepository.Commit();
              if (result > 0)
              {
                  // get latest userID
                  //newUser.UserId = _userRepository.GetAll().OrderByDescending(x => x.);
                  req.UserId = userEntity.UserId;
                  return _mapper.Map<CreateUserModel>(userEntity);
              }
              else
              {
                  return null;
              }
          }*/



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
                .FindByCondition(pt => pt.OrderId == orderId)
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
                .FindByCondition(pt => pt.OrderId == paymentResponse.OrderId)
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

    }

}
