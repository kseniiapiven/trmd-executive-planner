using System.Collections.ObjectModel;
using ExecutivePlanner.Models;

namespace ExecutivePlanner.Services;

public interface IMeetingService
{
    ObservableCollection<Meeting> Meetings { get; }
    Meeting? GetById(Guid id);
    void Update(Meeting meeting);
}
