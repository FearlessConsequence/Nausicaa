using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseWork.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class RecentsWindow : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private List<MyDocument> _allDocuments = new();

    public RecentsWindow() : this(0) { }

    public RecentsWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        txt_search.TextChanged += OnSearchTextChanged;
        this.Opened += async (s, e) => await LoadDocumentsAsync();
        
        if (_currentUserId == 0)
        {
            NotificationsControl.ShowError("Ошибка", "Пользователь не авторизован");
            return;
        }
    }

    private async Task LoadDocumentsAsync()
    {
        try
        {
            _allDocuments = await _db.GetUserDocumentsAsync(_currentUserId);
            var recentDocs = _allDocuments.OrderByDescending(d => d.CreatedAt).Take(50).ToList();
            
            documentsContainer.ItemsSource = recentDocs;
            emptyStateBorder.IsVisible = recentDocs.Count == 0;
            
            await Task.Delay(100);
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDocumentsAsync: {ex.Message}");
            emptyStateBorder.IsVisible = true;
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        string searchText = txt_search.Text?.Trim() ?? "";
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            var recentDocs = _allDocuments.OrderByDescending(d => d.CreatedAt).Take(50).ToList();
            documentsContainer.ItemsSource = recentDocs;
        }
        else
        {
            var filtered = _allDocuments.Where(d => 
                (d.Number?.ToString().Contains(searchText) ?? false) ||
                d.CitizenFullName?.ToLower().Contains(searchText.ToLower()) == true ||
                d.Content?.ToLower().Contains(searchText.ToLower()) == true ||
                d.DocumentType?.ToLower().Contains(searchText.ToLower()) == true
            ).OrderByDescending(d => d.CreatedAt).Take(50).ToList();
            
            documentsContainer.ItemsSource = filtered;
        }
        
        emptyStateBorder.IsVisible = documentsContainer.ItemsSource is ICollection<MyDocument> list && list.Count == 0;
        
        Task.Delay(100).ContinueWith(_ => 
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => SubscribeToButtons());
        });
    }

    private void SubscribeToButtons()
    {
        var buttons = documentsContainer.GetVisualDescendants()
            .OfType<Button>()
            .ToList();
            
        foreach (var button in buttons)
        {
            if (button.Name == "FavoriteButton" && button.Tag is MyDocument doc)
            {
                button.Content = doc.IsFavorite ? "★" : "☆";
                button.Foreground = doc.IsFavorite 
                    ? new SolidColorBrush(Color.Parse("#FFB800")) 
                    : new SolidColorBrush(Color.Parse("#6C757D"));
                
                button.Click -= OnFavoriteClick;
                button.Click += OnFavoriteClick;
            }
            else if (button.Name == "OpenButton" && button.Tag is MyDocument doc2)
            {
                button.Click -= OnOpenClick;
                button.Click += OnOpenClick;
            }
        }
    }

    private async void OnFavoriteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Toggle: userId={_currentUserId}, table={doc.TableName}, docId={doc.Id}");
                
                await _db.ToggleFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
                bool isFavorite = await _db.IsFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
                
                Console.WriteLine($"[DEBUG] Новый статус: {isFavorite}");
                
                button.Content = isFavorite ? "★" : "☆";
                button.Foreground = isFavorite 
                    ? new SolidColorBrush(Color.Parse("#FFB800")) 
                    : new SolidColorBrush(Color.Parse("#6C757D"));
                
                doc.IsFavorite = isFavorite;
                
                NotificationsControl.ShowSuccess("Избранное", isFavorite ? "Добавлено" : "Удалено");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.WriteLine($"[ERROR] {ex.StackTrace}");
                NotificationsControl.ShowError("Ошибка", ex.Message);
            }
        }
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
            // ✅ Передаём "Recents" как источник
            var viewerWindow = new DocumentViewerWindow(_currentUserId, fullDoc, this);
            Close();
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