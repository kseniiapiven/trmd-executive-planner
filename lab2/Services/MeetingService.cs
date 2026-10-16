using System.Collections.ObjectModel;
using ExecutivePlanner.Models;

namespace ExecutivePlanner.Services;

// Сховище в пам'яті; у лабораторній роботі №3 буде замінене на SQLite.
public class MeetingService : IMeetingService
{
    public ObservableCollection<Meeting> Meetings { get; } = new();

    public Meeting? GetById(Guid id) => Meetings.FirstOrDefault(m => m.Id == id);

    public void Update(Meeting meeting)
    {
        var index = Meetings.IndexOf(Meetings.First(m => m.Id == meeting.Id));
        Meetings[index] = meeting; // заміна елемента оновлює CollectionView
    }
}
