using BE_EXE201.Dtos;

namespace BE_EXE201.Common.Payloads.Responses;

public class GetUsersResponse
{
    public IEnumerable<UserModel> Users { get; set; } = new List<UserModel>();
}
