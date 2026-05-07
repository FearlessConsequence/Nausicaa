using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using CourseWork.Models;

namespace CourseWork.Controls;

public partial class NotificationsControl : UserControl
{
    private List<NotificationItem> _notifications = new();
    private static NotificationsControl? _instance;
    
    private const int AUTO_HIDE_SECONDS = 10;
    private const int MAX_NOTIFICATIONS = 10;

    public NotificationsControl()
    {
        InitializeComponent();
        _instance = this;
        LoadSampleNotifications();
    }

    public void AddNotification(string icon, string title, string message, NotificationType type = NotificationType.Info)
    {
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                var item = new NotificationItem
                {
                    Icon = icon,
                    Title = title,
                    Message = message,
                    Date = DateTime.Now.ToString("HH:mm"),
                    Type = type,
                    Timestamp = DateTime.Now
                };

                _notifications.Insert(0, item);

                if (_notifications.Count > MAX_NOTIFICATIONS)
                    _notifications.RemoveAt(_notifications.Count - 1);

                UpdateList();
                _ = AutoHideAsync(item);
            }
            catch (Exception ex)
            {
                if (ex.Message != "Пользователь не авторизован") 
                {
                Console.WriteLine($"[ERROR] AddNotification: {ex.Message}");
                }
            }
        });
    }

    public static void ShowSuccess(string title, string message) => 
        _instance?.AddNotification("", title, message, NotificationType.Success);
    
    public static void ShowError(string title, string message) => 
        _instance?.AddNotification("", title, message, NotificationType.Error);
    
    public static void ShowWarning(string title, string message) => 
        _instance?.AddNotification("⚠️", title, message, NotificationType.Warning);
    
    public static void ShowInfo(string title, string message) => 
        _instance?.AddNotification("", title, message, NotificationType.Info);

    private async System.Threading.Tasks.Task AutoHideAsync(NotificationItem item)
    {
        await System.Threading.Tasks.Task.Delay(AUTO_HIDE_SECONDS * 1000);
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (_notifications.Remove(item))
                    UpdateList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AutoHideAsync: {ex.Message}");
            }
        });
    }

    private void UpdateList()
    {
        try
        {
            if (NotificationsList == null) return;
            var copy = _notifications.ToList();
            NotificationsList.ItemsSource = copy;
            txt_empty.IsVisible = _notifications.Count == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] UpdateList: {ex.Message}");
        }
    }

    private void LoadSampleNotifications()
    {
        _notifications = new List<NotificationItem>
        {
            new NotificationItem
            {
                Icon = "",
                Title = "Система готова",
                Date = DateTime.Now.ToString("HH:mm"),
                Message = "Приложение запущено",
                Type = NotificationType.Success
            }
        };
        UpdateList();
    }

    public void ClearAll()
    {
        _notifications.Clear();
        UpdateList();
    }
}