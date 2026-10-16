namespace ExecutivePlanner.Models;

public class Meeting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }

    public string StartDisplay => StartTime.ToString("dd.MM.yyyy HH:mm");
}
