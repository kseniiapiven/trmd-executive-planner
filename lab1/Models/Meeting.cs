namespace ExecutivePlanner.Models;

public class Meeting
{
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }

    public string StartDisplay => StartTime.ToString("dd.MM.yyyy HH:mm");
}
