using ExecutivePlanner.Models;

namespace ExecutivePlanner.Services;

// Контракт доступу до даних зустрічей: ViewModel залежить лише від нього.
public interface IMeetingRepository
{
    Task<List<Meeting>> GetAllAsync();
    Task<Meeting?> GetByIdAsync(int id);
    Task<int> InsertAsync(Meeting meeting);
    Task<int> UpdateAsync(Meeting meeting);
    Task<int> DeleteAsync(Meeting meeting);
}
