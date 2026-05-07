using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;

namespace CourseWork.Views;

public partial class AdminInspectorWindow : Window
{
    private readonly int _currentUserId;
    
    public AdminInspectorWindow() : this(0) { }
    
    public AdminInspectorWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        WindowState = WindowState.Maximized;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);

        btn_newAppel.Click += Btn_newAppel_Click;
        btn_newStatement.Click += Btn_newStatement_Click;
        btn_newExplanationProtocol.Click += Btn_newExplanationProtocol_Click;
        btn_newAdministrativeProtocol.Click += Btn_newAdministrativeProtocol_Click;
        btn_newExaminationReport.Click += Btn_newExaminationReport_Click;
        btn_searchMain.Click += Btn_searchMain_Click;
    }
    
    private void Btn_newAppel_Click(object? sender, RoutedEventArgs e)
    {
        new NewAppel(_currentUserId).Show();
        Close();
    }
    
    private void Btn_newStatement_Click(object? sender, RoutedEventArgs e)
    {
        new NewStatement(_currentUserId).Show();
        Close();
    }
    
    private void Btn_newExplanationProtocol_Click(object? sender, RoutedEventArgs e)
    {
        new NewExplanationProtocol(_currentUserId).Show();
        Close();
    }
    
    private void Btn_newAdministrativeProtocol_Click(object? sender, RoutedEventArgs e)
    {
        new NewAdministrativeProtocol(_currentUserId).Show();
        Close();
    }
    
    private void Btn_newExaminationReport_Click(object? sender, RoutedEventArgs e)
    {
        new NewExaminationReport(_currentUserId).Show();
        Close();
    }
    
    private void Btn_searchMain_Click(object? sender, RoutedEventArgs e)
    {
        string dealNumber = txbx_deal.Text?.Trim() ?? "";
        
        if (string.IsNullOrWhiteSpace(dealNumber))
        {
            NotificationsControl.ShowWarning("Пустой поиск", "Введите номер дела");
            return;
        }
        
        new YourDocumentsWindow(_currentUserId).Show();
        Close();
    }
}