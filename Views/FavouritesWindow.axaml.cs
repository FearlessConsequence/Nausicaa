using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using CourseWork.Data;
using CourseWork.Controls;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class FavouritesWindow : Window
{
    private readonly DatabaseHelper _db;  // ✅ добавили readonly и инициализацию
    private readonly int _currentUserId;
    private List<MyDocument> _favorites = new();

    // ✅ Конструктор по умолчанию
    public FavouritesWindow() : this(0) { }

    // ✅ Основной конструктор
    public FavouritesWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();  // ✅ инициализация _db
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        this.Opened += async (s, e) => await LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            var rawFavorites = await _db.GetFavoriteDocumentsAsync(_currentUserId);
            
            if (rawFavorites.Count == 0)
            {
                documentsContainer.ItemsSource = null;
                emptyStateBorder.IsVisible = true;
                return;
            }

            _favorites = rawFavorites.Select(f => new MyDocument
            {
                Id = f.Id,
                DocumentType = f.DocumentType,
                TableName = f.DocumentType switch
                {
                    "Заявление" => "statement",
                    "Обращение" => "appeals",
                    "Протокол объяснения" => "explanation_protocol",
                    "Направление на мед. освид." => "medical_examination_report",
                    "Административный протокол" => "administrative_protocol",
                    _ => "unknown"
                },
                Number = f.Number,
                CreatedAt = f.MakingDateAndTime,
                CitizenFullName = f.CitizenName ?? "Неизвестно",
                Content = "",
                IsFavorite = true 
            }).ToList();

            documentsContainer.ItemsSource = _favorites;
            emptyStateBorder.IsVisible = false;
            
            await Task.Delay(100);
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadFavoritesAsync: {ex.Message}");
            emptyStateBorder.IsVisible = true;
        }
    }

    private void SubscribeToButtons()
    {
        var buttons = documentsContainer.GetVisualDescendants()
            .OfType<Button>()
            .ToList();
            
        Console.WriteLine($"[DEBUG] SubscribeToButtons: найдено кнопок: {buttons.Count}");
        
        foreach (var button in buttons)
        {
            if (button.Name == "FavoriteButton")
            {
                // ✅ Отписываемся старые обработчики
                button.Click -= OnRemoveFromFavorites;
                // ✅ Подписываемся новые
                button.Click += OnRemoveFromFavorites;
                
                // ✅ Обновляем иконку (должна быть звезда)
                button.Content = "★";
                button.Foreground = new SolidColorBrush(Color.Parse("#FFB800"));
            }
            else if (button.Name == "OpenButton")
            {
                button.Click -= OnOpenClick;
                button.Click += OnOpenClick;
            }
        }
    }

    private async void OnRemoveFromFavorites(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MyDocument doc)
        {
            try
            {
                // ✅ Удаляем из БД
                await _db.RemoveFromFavoritesAsync(_currentUserId, doc.TableName, doc.Id);
                
                // ✅ Полностью перезагружаем список из БД
                await LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RemoveFromFavorites: {ex.Message}");
                NotificationsControl.ShowError("Ошибка", $"Не удалось удалить из избранного: {ex.Message}");
            }
        }
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MyDocument doc)
        {
            try
            {
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                new DocumentViewerWindow(_currentUserId, fullDoc, "Favorites").Show();
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Open document: {ex.Message}");
                await ShowMessage("Ошибка", $"Не удалось открыть документ: {ex.Message}");
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