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

namespace BE_EXE201.Services
{
    public class UserService
    {
        private readonly IRepository<User, int> _userRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<UserRole, int> _userRoleRepository;

        public UserService(IRepository<User, int> userRepository, IMapper mapper,IRepository<UserRole,int>userRoleRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            var result = _userRepository.GetAll().ToList();
            return  _mapper.Map<List<UserModel>>(result);
        }
        public async Task<UserModel> CreateNewUser(UserModel newUser) 
        {
            var userEntity =  _mapper.Map<User>(newUser);
            var existedUser = _userRepository.FindByCondition(x=> x.Email == newUser.Email).FirstOrDefault();
            if(existedUser is not null) 
            {
                throw new BadRequestException("email already exist");
            }
            var userRoleEntity = _userRoleRepository.FindByCondition(ur => ur.RoleId == newUser.RoleID).FirstOrDefault();

            userEntity.UserRole = userRoleEntity!;

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

        public async Task<UserModel> UpdateUser(UserModel userUpdate, UpdateUserRequest req)
        {
            try
            {
                var userEntity = _mapper.Map<User>(userUpdate);

                var existedUser = await _userRepository.FindByCondition(x => x.Email == userEntity.Email).FirstOrDefaultAsync();

                if (existedUser != null)
                {
                    // Cập nhật thông tin người dùng từ req
                    if (!string.IsNullOrEmpty(req.UserName))
                    {
                        existedUser.UserName = req.UserName;
                    }
                    if (!string.IsNullOrEmpty(req.Password))
                    {
                        existedUser.Password = SecurityUtil.Hash(req.Password);
                    }
                    if (!string.IsNullOrEmpty(req.FullName))
                    {
                        existedUser.FullName = req.FullName;
                    }
                    if (!string.IsNullOrEmpty(req.Email))
                    {
                        existedUser.Email = req.Email;
                    }
                    if (!string.IsNullOrEmpty(req.Gender))
                    {
                        existedUser.Gender = req.Gender;
                    }
                    
                    if (!string.IsNullOrEmpty(req.Address))
                    {
                        existedUser.Address = req.Address;
                    }
                    if (req.BirthDate.HasValue)
                    {
                        existedUser.BirthDate = (DateTime)req.BirthDate;
                    }
                    if (!string.IsNullOrEmpty(req.PhoneNumber))
                    {
                        existedUser.PhoneNumber = req.PhoneNumber;
                    }

                    _mapper.Map(userEntity, existedUser);

                    var user = _userRepository.Update(existedUser);

                    var result = await _userRepository.Commit();

                    if (result > 0)
                    {
                        return _mapper.Map<UserModel>(user);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return null;
            }
        }


    }
}
