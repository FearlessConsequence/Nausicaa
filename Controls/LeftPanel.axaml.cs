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
        ConfigurePopupButtons();
        
        btn_statistics.IsVisible = App.IsChief;
        btn_statistics.Click += OnStatisticsClick;
        btn_createNewDocument.IsVisible = true;
        btn_yourDocuments.IsVisible = true;
        btn_otherDocuments.IsVisible = true;
        btn_drafts.IsVisible = true;
    }

    private void OnStatisticsClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var statisticsWindow = new StatisticsWindow();
        statisticsWindow.Show();
        currentWindow?.Close();
    }
    
    private void ConfigurePopupButtons()
    {
        // Скрываем все кнопки по умолчанию
        pop_documentType_btn_appel.IsVisible = false;
        pop_documentType_btn_statement.IsVisible = false;
        pop_documentType_btn_explanation_protocol.IsVisible = false;
        pop_documentType_btn_administrative_protocol.IsVisible = false;
        pop_documentType_btn_examination_report.IsVisible = false;
        pop_documentType_btn_medical_certificate.IsVisible = false;
        pop_documentType_btn_forensic_examination.IsVisible = false;
        pop_documentType_btn_resolution.IsVisible = false;
        pop_documentType_btn_deal.IsVisible = false;
        pop_documentType_btn_citizen.IsVisible = false;
        
        switch (_currentUserRole)
        {
            case UserRole.AdminInspector:
                pop_documentType_btn_deal.IsVisible = true;
                pop_documentType_btn_citizen.IsVisible = true;
                pop_documentType_btn_appel.IsVisible = true;
                pop_documentType_btn_statement.IsVisible = true;
                pop_documentType_btn_explanation_protocol.IsVisible = true;
                pop_documentType_btn_administrative_protocol.IsVisible = true;
                pop_documentType_btn_examination_report.IsVisible = true;
                break;
                
            case UserRole.MedicalExpert:
                pop_documentType_btn_medical_certificate.IsVisible = true;
                break;
                
            case UserRole.Judge:
                pop_documentType_btn_resolution.IsVisible = true;
                break;
                
            case UserRole.ForensicExpert:
                pop_documentType_btn_forensic_examination.IsVisible = true;
                break;
                
            case UserRole.PoliceOfficer:
            default:
                pop_documentType_btn_appel.IsVisible = true;
                pop_documentType_btn_statement.IsVisible = true;
                pop_documentType_btn_explanation_protocol.IsVisible = true;
                pop_documentType_btn_administrative_protocol.IsVisible = true;
                pop_documentType_btn_examination_report.IsVisible = true;
                break;
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
        pop_documentType_btn_medical_certificate.Click += OnMedicalCertificateClick;
        pop_documentType_btn_resolution.Click += OnResolutionClick;
        pop_documentType_btn_forensic_examination.Click += OnForensicExpertiseClick;
        pop_documentType_btn_deal.Click += OnDealClick;
        pop_documentType_btn_citizen.Click += OnCitizenCreateClick;
    }

    private void OnDealClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewDeal(App.CurrentUserId, App.CurrentUserRole);
        newWindow.Show();
        currentWindow?.Close();
    }

    private void OnCitizenCreateClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewCitizen(App.CurrentUserId, App.CurrentUserRole);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnMedicalCertificateClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewMedicalCertificate(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }

    private void OnResolutionClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewResolution(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }

    private void OnForensicExpertiseClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewForensicExpertise(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnMainClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new MainWindow(App.CurrentUserId, App.CurrentUserRole);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnFavoritesClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new FavouritesWindow(App.CurrentUserId);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnRecentsClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new RecentsWindow(App.CurrentUserId);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnReferenceBooksClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new ReferenceBooksWindow(App.CurrentUserId);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnCitizensClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new SearchCitizensWindow(App.CurrentUserId);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnYourDocumentsClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new YourDocumentsWindow(null, App.CurrentUserId);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnOtherDocumentsClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new OtherDocumentsWindow(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnDraftsClick(object? sender, RoutedEventArgs e)
    {
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new DraftsWindow(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnCreateNewDocumentClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = true;
    }
    
    private void OnAppelClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewAppel(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnStatementClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewStatement(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnAdministrativeProtocolClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewAdministrativeProtocol(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnExaminationReportClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewExaminationReport(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void OnExplanationProtocolClick(object? sender, RoutedEventArgs e)
    {
        pop_documentType.IsOpen = false;
        var currentWindow = this.VisualRoot as Window;
        var newWindow = new NewExplanationProtocol(App.CurrentUserId, currentWindow);
        newWindow.Show();
        currentWindow?.Close();
    }
    
    private void CloseParent()
    {
        var window = this.VisualRoot as Window;
        window?.Close();
    }
}