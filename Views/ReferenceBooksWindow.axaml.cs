using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class ReferenceBooksWindow : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId = 0;

    public ReferenceBooksWindow()
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        btn_search.Click += OnSearchClick;
        
        txt_empty.IsVisible = true;
        itemsContainer.IsVisible = false;
        txt_current_title.Text = string.Empty;
        
        cmb_search_type.SelectedIndex = 0;
    }
    private async void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        string searchText = txt_search.Text?.Trim() ?? "";
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            await ShowMessage("Внимание", "Введите поисковый запрос");
            return;
        }
        
        var selectedType = (cmb_search_type.SelectedItem as ComboBoxItem)?.Content?.ToString();
        
        if (string.IsNullOrEmpty(selectedType))
        {
            return;
        }
        
        txt_current_title.Text = $"{selectedType}: \"{searchText}\"";
        
        object? results = null;
        
        switch (selectedType)
        {
            case "Статьи КоАП":
                results = await _db.SearchArticlesAsync(searchText);
                txt_current_title.Text = $"Статьи КоАП - результаты поиска: \"{searchText}\"";
                break;
                
            case "Должности":
                results = await _db.SearchPostsAsync(searchText);
                txt_current_title.Text = $"Должности - результаты поиска: \"{searchText}\"";
                break;
                
            case "Организации":
                results = await _db.SearchStructuresAsync(searchText);
                txt_current_title.Text = $"Организации - результаты поиска: \"{searchText}\"";
                break;
        }
        
        if (results != null)
        {
            itemsContainer.ItemsSource = (System.Collections.IEnumerable?)results;
            var count = ((System.Collections.IList)results).Count;
            txt_empty.IsVisible = count == 0;
            itemsContainer.IsVisible = count > 0;
            
            if (count == 0)
            {
                txt_empty.Text = "Ничего не найдено";
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