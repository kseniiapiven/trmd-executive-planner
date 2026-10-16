using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;

namespace ExecutivePlanner.Services;

public interface IUserRepository
{
    Task<OperationResult<User?>> GetByLoginAsync(string login);
}
