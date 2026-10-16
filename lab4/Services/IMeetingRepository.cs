using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;

namespace ExecutivePlanner.Services;

// Контракт доступу до даних зустрічей: ViewModel залежить лише від нього.
// Методи не кидають винятків: збій повертається як контрольований результат.
public interface IMeetingRepository
{
    Task<OperationResult<List<Meeting>>> GetAllAsync();
    Task<OperationResult<Meeting?>> GetByIdAsync(int id);
    Task<bool> InsertAsync(Meeting meeting);
    Task<bool> UpdateAsync(Meeting meeting);
    Task<bool> DeleteAsync(Meeting meeting);
}
