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

public partial class OtherDocumentsWindow : Window
{
    private readonly DatabaseHelper? _db;
    private readonly int _currentUserId;
    private List<ExternalDocument> _documents = new();
    public OtherDocumentsWindow()
    {
        InitializeComponent();
    }

    public OtherDocumentsWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        _db = new DatabaseHelper();
        
        btn_select_deal.Click += Btn_select_deal_Click;
        btn_search.Click += Btn_search_Click;
        
        emptyStateBorder.IsVisible = false;
        documentsContainer.IsVisible = false;
    }

    private void Btn_select_deal_Click(object? sender, RoutedEventArgs e)
    {
        var dealWindow = new SelectDealWindow(_currentUserId, this);
        dealWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selectedDeal = dealWindow.SelectedDeal;
                if (selectedDeal != null)
                {
                    txt_deal.Text = selectedDeal.Number;
                    txt_deal.Tag = selectedDeal.Id;
                }
                Activate();
            });
        };
        dealWindow.ShowDialog(this);
    }

    private async void Btn_search_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (txt_deal.Tag == null)
            {
                NotificationsControl.ShowWarning("Внимание", "Выберите дело");
                return;
            }
            
            int dealId = (int)txt_deal.Tag;
            if (_db == null) return; var drafts = await _db.GetDraftsAsync(_currentUserId);
            _documents = await _db.GetExternalDocumentsAsync(dealId, null);
            
            documentsContainer.ItemsSource = null;
            documentsContainer.ItemsSource = _documents;
            
            emptyStateBorder.IsVisible = _documents.Count == 0;
            documentsContainer.IsVisible = _documents.Count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Search: {ex.Message}");
            await ShowMessage("Ошибка", $"Не удалось найти документы:\n{ex.Message}");
        }
    }

    private async void OnRequestClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ExternalDocument doc)
        {
            var reasonWindow = new RequestReasonWindow();
            var result = await reasonWindow.ShowDialog<string?>(this);
            
            if (!string.IsNullOrWhiteSpace(result))
            {
                try
                {
                    if (_db == null) return; var drafts = await _db.GetDraftsAsync(_currentUserId);
                    await _db.SaveDocumentAccessRequestAsync(_currentUserId, doc.TableName, doc.Id, result);
                    
                    var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                    
                    // ✅ Передаём this как previousWindow
                    var viewerWindow = new DocumentViewerWindow(_currentUserId, fullDoc);
                    viewerWindow.Show();
                    this.Hide();  // ← Hide вместо Close
                }
                catch (Exception ex)
                {
                    await ShowMessage("Ошибка", $"Не удалось получить документ: {ex.Message}");
                }
            }
        }
    }

    private void ShowDocumentViewer(DocumentFull fullDoc)
    {
        var viewerWindow = new DocumentViewerWindow(_currentUserId, fullDoc);
        Hide();
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