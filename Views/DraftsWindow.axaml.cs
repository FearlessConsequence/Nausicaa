using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Data;
using CourseWork.Models;
using CourseWork.Controls;

namespace CourseWork.Views;

public partial class DraftsWindow : Window
{
    private readonly DatabaseHelper _db;
    private readonly Window? _previousWindow;
    private readonly int _currentUserId;
    private List<Draft> _all = new();
    private List<Draft> _shown = new();

    public DraftsWindow(int userId)
    {
        InitializeComponent();
        _currentUserId = userId;
        _previousWindow = _previousWindow;
        cmb_filterType.SelectedIndex = 0;
        _db = new DatabaseHelper();
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        this.Opened += async (_, _) => await _load();
    }

    private async Task _load()
    {
        try
        {
            _all = await _db.GetDraftsAsync(_currentUserId);
            _applyFilter();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] {ex.Message}");
            await ShowMessage("Ошибка", $"Не удалось загрузить черновики: {ex.Message}");
        }
    }

    private void _applyFilter()
    {
        var sel = (cmb_filterType.SelectedItem as ComboBoxItem)?.Content?.ToString();
        _shown = string.IsNullOrEmpty(sel) || sel == "Все" 
            ? _all 
            : _all.Where(d => d.TypeDisplayName == sel).ToList();
        
        draftsList.ItemsSource = _shown;
        txt_empty.IsVisible = _shown.Count == 0;
        txt_draftsCount.Text = $"Всего: {_all.Count}";
    }
    
    private void CmbFilter_SelectionChanged(object? s, SelectionChangedEventArgs e) => _applyFilter();

    private async void BtnEdit_Click(object? s, RoutedEventArgs e)
    {
        if (s is Button b && b.Tag is int id)
        {
            var draft = _all.FirstOrDefault(x => x.Id == id);
            if (draft != null)
            {
                try
                {
                    Window? targetWindow = null;
                    
                    switch (draft.DocumentType)
                    {
                        case "appeals":
                            var appelWindow = new NewAppel(_currentUserId);
                            await appelWindow.LoadDraftAsync(draft);
                            targetWindow = appelWindow;
                            break;
                            
                        case "statement":
                            var statementWindow = new NewStatement(_currentUserId);
                            await statementWindow.LoadDraftAsync(draft);
                            targetWindow = statementWindow;
                            break;
                            
                        case "administrative_protocol":
                            var adminWindow = new NewAdministrativeProtocol(_currentUserId);
                            await adminWindow.LoadDraftAsync(draft);
                            targetWindow = adminWindow;
                            break;
                            
                        case "explanation_protocol":
                            var explanationWindow = new NewExplanationProtocol(_currentUserId);
                            await explanationWindow.LoadDraftAsync(draft);
                            targetWindow = explanationWindow;
                            break;
                            
                        case "examination_report":
                            var examWindow = new NewExaminationReport(_currentUserId);
                            await examWindow.LoadDraftAsync(draft);
                            targetWindow = examWindow;
                            break;
                    }
                    
                    if (targetWindow != null)
                    {
                        targetWindow.Show();
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] BtnEdit_Click: {ex.Message}");
                    NotificationsControl.ShowError("Ошибка", $"Не удалось загрузить черновик: {ex.Message}");
                }
            }
        }
    }

    private void Btn_back_Click(object? sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow(_currentUserId);
        mainWindow.Show();
        this.Close();
    }

    private async void BtnDelete_Click(object? s, RoutedEventArgs e)
    {
        if (s is Button b && b.Tag is int id)
        {
            try
            {
                await _db.DeleteDraftAsync(id);
                await _load();
                NotificationsControl.ShowSuccess("Черновик удален", $"Черновик ID {id} успешно удален");
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", $"Не удалось удалить черновик: {ex.Message}");
            }
        }
    }

    private async Task ShowMessage(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new Button { Content = "OK", Width = 80, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center }
                }
            }
        };
        
        var okButton = (dialog.Content as StackPanel)?.Children[1] as Button;
        if (okButton != null) okButton.Click += (s, args) => dialog.Close();
        
        await dialog.ShowDialog(this);
    }
}