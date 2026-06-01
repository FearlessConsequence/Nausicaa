using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using CourseWork.Models;
using CourseWork.Views;

namespace CourseWork.Controls;

public partial class LeftPanel : UserControl
{
    private int _currentUserId;
    private UserRole _currentUserRole;
    private int _currentDraftId;
    
    public LeftPanel()
    {
        InitializeComponent();
    }
    
    public void SetUserId(int userId, UserRole role = UserRole.PoliceOfficer, int currentDraftId = 0)
    {
        _currentUserId = userId;
        _currentUserRole = role;
        _currentDraftId = currentDraftId;
        SetupButtons();
        ConfigurePopupByRole();
        
        btn_createNewDocument.IsVisible = true;
        btn_yourDocuments.IsVisible = true;
        btn_otherDocuments.IsVisible = true;
        btn_drafts.IsVisible = true;
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
        pop_documentType_btn_medical_certificate.Click += OnMedicalCertificateClick;
        pop_documentType_btn_resolution.Click += OnResolutionClick;
        pop_documentType_btn_forensic_expertise.Click += OnForensicExpertiseClick;
    }
    
    private void OnMedicalCertificateClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var window = this.VisualRoot as Window;
        var newCertWindow = new NewMedicalCertificate(_currentUserId, window);
        newCertWindow.Show();
        window?.Close();
    }

    private void OnResolutionClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var window = this.VisualRoot as Window;
        var newResolutionWindow = new NewResolution(_currentUserId, window);
        newResolutionWindow.Show();
        window?.Close();
    }

    private void OnForensicExpertiseClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var window = this.VisualRoot as Window;
        var newExpertiseWindow = new NewForensicExpertise(_currentUserId, window);
        newExpertiseWindow.Show();
        window?.Close();
    }
    private void OnMainClick(object? sender, RoutedEventArgs e)
    {
        new MainWindow(App.CurrentUserId, App.CurrentUserRole).Show();
        CloseParent();
    }
    
    private void OnFavoritesClick(object? sender, RoutedEventArgs e)
    {
        new FavouritesWindow(App.CurrentUserId).Show();
        CloseParent();
    }
    
    private void OnRecentsClick(object? sender, RoutedEventArgs e)
    {
        new RecentsWindow(App.CurrentUserId).Show();
        CloseParent();
    }
    
    private void OnReferenceBooksClick(object? sender, RoutedEventArgs e)
    {
        new ReferenceBooksWindow(App.CurrentUserId).Show();
        CloseParent();
    }
    
    private void OnCitizensClick(object? sender, RoutedEventArgs e)
    {
        new SearchCitizensWindow(App.CurrentUserId).Show();
        CloseParent();
    }
    
    private void OnYourDocumentsClick(object? sender, RoutedEventArgs e)
    {
        var window = this.VisualRoot as Window;
        new YourDocumentsWindow(null, App.CurrentUserId).Show();
        window?.Close();
    }
    
    private void OnOtherDocumentsClick(object? sender, RoutedEventArgs e)
    {
        var window = this.VisualRoot as Window;
        new OtherDocumentsWindow(App.CurrentUserId, window).Show();
        window?.Close();
    }
    
    private void OnDraftsClick(object? sender, RoutedEventArgs e)
    {
        var window = this.VisualRoot as Window;
        new DraftsWindow(App.CurrentUserId, window).Show();
        window?.Close();
    }
    
    private void OnCreateNewDocumentClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = true;
    }
    
    private void OnAppelClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var window = this.VisualRoot as Window;
        new NewAppel(App.CurrentUserId, window).Show();
        window?.Close();
    }
    
    private void OnStatementClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var window = this.VisualRoot as Window;
        new NewStatement(App.CurrentUserId, window).Show();
        window?.Close();
    }
    
    private void OnAdministrativeProtocolClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var window = this.VisualRoot as Window;
        new NewAdministrativeProtocol(App.CurrentUserId, window).Show();
        window?.Close();
    }
    
    private void OnExaminationReportClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var window = this.VisualRoot as Window;
        new NewExaminationReport(App.CurrentUserId, window).Show();
        window?.Close();
    }
    
    private void OnExplanationProtocolClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var window = this.VisualRoot as Window;
        new NewExplanationProtocol(App.CurrentUserId, window).Show();
        window?.Close();
    }
    
    private void CloseParent()
    {
        var window = this.VisualRoot as Window;
        window?.Close();
    }

    private void ConfigurePopupByRole()
    {
        // Скрываем все кнопки по умолчанию
        pop_documentType_btn_appel.IsVisible = false;
        pop_documentType_btn_statement.IsVisible = false;
        pop_documentType_btn_explanation_protocol.IsVisible = false;
        pop_documentType_btn_examination_report.IsVisible = false;
        pop_documentType_btn_administrative_protocol.IsVisible = false;
        pop_documentType_btn_medical_certificate.IsVisible = false;
        pop_documentType_btn_resolution.IsVisible = false;
        pop_documentType_btn_forensic_expertise.IsVisible = false;
        
        switch (_currentUserRole)
        {
            case UserRole.PoliceOfficer:
            case UserRole.AdminInspector:
                // Полицейский и инспектор - все основные типы
                pop_documentType_btn_appel.IsVisible = true;
                pop_documentType_btn_statement.IsVisible = true;
                pop_documentType_btn_explanation_protocol.IsVisible = true;
                pop_documentType_btn_examination_report.IsVisible = true;
                pop_documentType_btn_administrative_protocol.IsVisible = true;
                break;
                
            case UserRole.MedicalExpert:
                // Врач - только акт мед. освидетельствования
                pop_documentType_btn_medical_certificate.IsVisible = true;
                break;
                
            case UserRole.Judge:
                // Судья - только постановление
                pop_documentType_btn_resolution.IsVisible = true;
                break;
                
            case UserRole.ForensicExpert:
                // Эксперт - только экспертиза
                pop_documentType_btn_forensic_expertise.IsVisible = true;
                break;
        }
    }
}