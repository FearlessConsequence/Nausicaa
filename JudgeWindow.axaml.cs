using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;

namespace CourseWork.Views;

public partial class JudgeWindow : Window
{
    private readonly int _currentUserId;
    
    public JudgeWindow() : this(0) { }
    
    public JudgeWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        WindowState = WindowState.Maximized;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        btn_create_resolution.Click += Btn_create_resolution_Click;
    }
    
    private void Btn_create_resolution_Click(object? sender, RoutedEventArgs e)
    {
        NotificationsControl.ShowInfo("Постановление", "Форма создания постановления");
    }
}