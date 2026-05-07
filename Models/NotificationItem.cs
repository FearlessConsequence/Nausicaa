using System;
using Avalonia.Controls.Notifications;
using Avalonia.Media;

namespace CourseWork.Models;

public class NotificationItem
{
    public string Icon { get; set; } = "📄";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Date { get; set; } = DateTime.Now.ToString("HH:mm");
    public NotificationType Type { get; set; } = NotificationType.Info;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    public string BorderColor => Type switch
    {
        NotificationType.Success => "#28A745",
        NotificationType.Warning => "#FFC107",
        NotificationType.Error => "#DC3545",
        _ => "#17A2B8"
    };
    
    public string BackgroundColor => Type switch
    {
        NotificationType.Success => "#E8F5E9",
        NotificationType.Warning => "#FFF3CD",
        NotificationType.Error => "#F8D7DA",
        _ => "#E8F4F8"
    };
}