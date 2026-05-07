using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Models;
using CourseWork.Views;

namespace CourseWork.Controls;

public partial class LeftPanel : UserControl
{
    private int _currentUserId;
    private UserRole _currentUserRole;
    
    public LeftPanel()
    {
        InitializeComponent();
    }
    
    public void SetUserId(int userId, UserRole role = UserRole.PoliceOfficer)
    {
        _currentUserId = userId;
        _currentUserRole = role;
        SetupButtons();
        
        // Скрываем недоступные кнопки в зависимости от роли
        if (role == UserRole.Judge || role == UserRole.MedicalExpert)
        {
            btn_createNewDocument.IsVisible = false;
            btn_yourDocuments.IsVisible = false;
            btn_otherDocuments.IsVisible = false;
            btn_drafts.IsVisible = false;
        }
        else
        {
            btn_createNewDocument.IsVisible = true;
            btn_yourDocuments.IsVisible = true;
            btn_otherDocuments.IsVisible = true;
            btn_drafts.IsVisible = true;
        }
    }
    
    private void SetupButtons()
    {
        btn_main.Click += OnMainClick;
        btn_favorites.Click += OnFavoritesClick;
        btn_recents.Click += OnRecentsClick;
        btn_referenceBooks.Click += OnReferenceBooksClick;
        btn_citizens.Click += OnCitizensClick;
        btn_yourDocuments.Click += OnYourDocumentsClick;
        btn_otherDocuments.Click += OnOtherDocumentsClick;
        btn_drafts.Click += OnDraftsClick;
        btn_createNewDocument.Click += OnCreateNewDocumentClick;
        
        pop_documentType_btn_appel.Click += OnAppelClick;
        pop_documentType_btn_statement.Click += OnStatementClick;
        pop_documentType_btn_administrative_protocol.Click += OnAdministrativeProtocolClick;
        pop_documentType_btn_examination_report.Click += OnExaminationReportClick;
        pop_documentType_btn_explanation_protocol.Click += OnExplanationProtocolClick;
    }
    
    private void OnMainClick(object? sender, RoutedEventArgs e)
    {
        new MainWindow(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnFavoritesClick(object? sender, RoutedEventArgs e)
    {
        new FavouritesWindow(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnRecentsClick(object? sender, RoutedEventArgs e)
    {
        new RecentsWindow(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnReferenceBooksClick(object? sender, RoutedEventArgs e)
    {
        new ReferenceBooksWindow().Show();
        CloseParent();
    }
    
    private void OnCitizensClick(object? sender, RoutedEventArgs e)
    {
        new SearchCitizensWindow(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnYourDocumentsClick(object? sender, RoutedEventArgs e)
    {
        new YourDocumentsWindow(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnOtherDocumentsClick(object? sender, RoutedEventArgs e)
    {
        new OtherDocumentsWindow(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnDraftsClick(object? sender, RoutedEventArgs e)
    {
        new DraftsWindow(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnCreateNewDocumentClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = true;
    }
    
    private void OnAppelClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        new NewAppel(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnStatementClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        new NewStatement(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnAdministrativeProtocolClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        new NewAdministrativeProtocol(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnExaminationReportClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        new NewExaminationReport(_currentUserId).Show();
        CloseParent();
    }
    
    private void OnExplanationProtocolClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        new NewExplanationProtocol(_currentUserId).Show();
        CloseParent();
    }
    
    private void CloseParent()
    {
        var window = this.VisualRoot as Window;
        window?.Close();
    }
}