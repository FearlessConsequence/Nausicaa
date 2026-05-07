using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;

namespace CourseWork.Views;

public partial class MedicalExpertWindow : Window
{
    private readonly int _currentUserId;
    
    public MedicalExpertWindow() : this(0) { }
    
    public MedicalExpertWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        WindowState = WindowState.Maximized;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        btn_create_examination.Click += Btn_create_examination_Click;
    }
    
    private void Btn_create_examination_Click(object? sender, RoutedEventArgs e)
    {
        NotificationsControl.ShowInfo("Экспертиза", "Форма создания судебно-медицинской экспертизы");
    }
}