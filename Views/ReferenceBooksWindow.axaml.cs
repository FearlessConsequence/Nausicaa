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
            NotificationsControl.ShowWarning("Внимание", "Введите текст для поиска");
            return;
        }
        
        var selectedType = (cmb_search_type.SelectedItem as ComboBoxItem)?.Content?.ToString();
        
        if (string.IsNullOrEmpty(selectedType))
        {
            NotificationsControl.ShowWarning("Внимание", "Выберите тип справочника");
            return;
        }
        
        txt_current_title.Text = $"{selectedType}: \"{searchText}\"";
        
        object? results = null;
        
        try
        {
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
                    
                default:
                    NotificationsControl.ShowError("Ошибка", $"Неизвестный тип: {selectedType}");
                    return;
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
            else
            {
                txt_empty.IsVisible = true;
                txt_empty.Text = "Ошибка при выполнении поиска";
                itemsContainer.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Поиск не выполнен: {ex.Message}");
            txt_empty.IsVisible = true;
            txt_empty.Text = "Произошла ошибка при поиске";
            itemsContainer.IsVisible = false;
        }
    }
}