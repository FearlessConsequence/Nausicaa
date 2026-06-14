#pragma warning disable CS0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class MainWindow : Window
{
    private readonly int _currentUserId;
    private readonly DatabaseHelper? _db;
    private UserRole _currentUserRole;
    private List<RecentDocument> _recentDocuments = new();
    private List<Draft> _drafts = new();
    
    public MainWindow(int currentUserId, UserRole role = UserRole.PoliceOfficer)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        _currentUserRole = role;
        if (App.CurrentUserRole != role)
        {
            App.CurrentUserRole = role;
        }
        ConfigureForRole();
        WindowState = WindowState.Maximized;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, _currentUserRole);
        
        // Кнопки создания документов
        btn_newAppel.Click += btn_newAppel_click;
        btn_newStatement.Click += btn_newStatement_click;
        btn_newExplanationProtocol.Click += btn_newExplanationProtocol_click;
        btn_newAdministrativeProtocol.Click += btn_newAdministrativeProtocol_click;
        btn_newExaminationReport.Click += btn_newExaminationReport_click;
        btn_newMedicalCertificate.Click += btn_newMedicalCertificate_Click;
        btn_newResolution.Click += btn_newResolution_Click;
        btn_newForensicExpertise.Click += btn_newForensicExpertise_Click;
        
        // Поиск гражданина
        btn_citizen_search.Click += BtnCitizenSearch_Click;
        
        // Поиск документов для разных ролей
        btn_police_doc_search.Click += BtnPoliceDocSearch_Click;
        btn_doctor_doc_search.Click += BtnDoctorDocSearch_Click;
        btn_judge_doc_search.Click += BtnJudgeDocSearch_Click;
        btn_forensic_doc_search.Click += BtnForensicDocSearch_Click;
        btn_newDeal.Click += btn_newDeal_Click;
        btn_newCitizen.Click += btn_newCitizen_Click;
        
        this.Opened += async (s, e) => 
        {
            await LoadRecentDocumentsAsync();
            await LoadDraftsAsync();
        };
    }

    // ==========================================
    // ПОИСК ГРАЖДАНИНА
    // ==========================================
    private void BtnCitizenSearch_Click(object? sender, RoutedEventArgs e)
    {
        // Проверяем, не включён ли чекбокс поиска по гражданину в блоке документа
        if (chk_police_search_doc.IsChecked == true ||
            chk_doctor_search_doc.IsChecked == true ||
            chk_judge_search_doc.IsChecked == true ||
            chk_forensic_search_doc.IsChecked == true)
        {
            NotificationsControl.ShowWarning("Внимание", 
                "Снимите галочку 'Искать по гражданину' в блоке поиска документа");
            return;
        }
        
        string passport = txbx_citizen_passport.Text?.Trim() ?? "";
        string fio = txbx_citizen_fio.Text?.Trim() ?? "";
        
        if (string.IsNullOrWhiteSpace(fio) && string.IsNullOrWhiteSpace(passport))
        {
            NotificationsControl.ShowWarning("Введите данные", 
                "Для поиска гражданина укажите фамилию или паспортные данные");
            return;
        }
        
        var searchParams = new CitizenSearchParams
        {
            Passport = passport,
            FullName = fio,
            Birthday = dp_citizen_birthday.SelectedDate?.DateTime,
            Phone = txbx_citizen_phone.Text?.Trim(),
            Address = txbx_citizen_address.Text?.Trim()
        };
        
        // ✅ Разбираем ФИО на части
        if (!string.IsNullOrWhiteSpace(fio))
        {
            var parts = fio.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1) searchParams.LastName = parts[0];
            if (parts.Length >= 2) searchParams.FirstName = parts[1];
            if (parts.Length >= 3) searchParams.Patronymic = parts[2];
        }
        
        var searchWindow = new SearchCitizensWindow(App.CurrentUserId, searchParams);
        searchWindow.Show();
        Close();
    }

    // ==========================================
    // ПОЛИЦЕЙСКИЙ - поиск документов
    // ==========================================
    private void BtnPoliceDocSearch_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txbx_police_doc_number.Text))
        {
            NotificationsControl.ShowWarning("Введите номер документа", 
                "Для поиска документов укажите номер документа");
            return;
        }
        bool searchByCitizen = chk_police_search_doc.IsChecked == true;
    
        string docNumber = txbx_police_doc_number.Text?.Trim() ?? "";
        string docType = (cmb_police_doc_type.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Любой";
        DateTime? date = dp_police_doc_date.SelectedDate?.DateTime;
        
        CitizenSearchParams? citizenParams = null;
        if (searchByCitizen)
        {
            string passport = txbx_citizen_passport.Text?.Trim() ?? "";
            string fio = txbx_citizen_fio.Text?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(passport) && string.IsNullOrWhiteSpace(fio))
            {
                NotificationsControl.ShowWarning("Введите данные гражданина", 
                    "Для поиска по гражданину укажите ФИО или паспорт");
                return;
            }
            
            citizenParams = new CitizenSearchParams
            {
                Passport = passport,
                FullName = fio,
                Birthday = dp_citizen_birthday.SelectedDate?.DateTime,
                Phone = txbx_citizen_phone.Text?.Trim(),
                Address = txbx_citizen_address.Text?.Trim()
            };
            
            if (!string.IsNullOrWhiteSpace(fio))
            {
                var parts = fio.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1) citizenParams.LastName = parts[0];
                if (parts.Length >= 2) citizenParams.FirstName = parts[1];
                if (parts.Length >= 3) citizenParams.Patronymic = parts[2];
            }
        }
        
        // ✅ searchValue = "" (пустая строка - номер документа), filterValue = dealNumber (номер дела)
        new YourDocumentsWindow(this, App.CurrentUserId, docNumber, date, citizenParams, docType).Show();
        this.Close();
    }

    // ==========================================
    // ВРАЧ - поиск документов
    // ==========================================
    private void BtnDoctorDocSearch_Click(object? sender, RoutedEventArgs e)
    {
        bool searchByCitizen = chk_doctor_search_doc.IsChecked == true;
        
        string reportNumber = txbx_doctor_report_number.Text?.Trim() ?? "";
        string certNumber = txbx_doctor_cert_number.Text?.Trim() ?? "";
        DateTime? date = dp_doctor_doc_date.SelectedDate?.DateTime;
        
        if (!string.IsNullOrWhiteSpace(reportNumber) && !string.IsNullOrWhiteSpace(certNumber))
        {
            NotificationsControl.ShowWarning("Внимание", 
                "Заполните только одно поле: номер направления или номер акта");
            return;
        }
        
        if (!searchByCitizen && string.IsNullOrWhiteSpace(reportNumber) && string.IsNullOrWhiteSpace(certNumber))
        {
            NotificationsControl.ShowWarning("Введите номер", 
                "Для поиска укажите номер направления или номер акта");
            return;
        }
        
        CitizenSearchParams? citizenParams = null;
        if (searchByCitizen)
        {
            string passport = txbx_citizen_passport.Text?.Trim() ?? "";
            string fio = txbx_citizen_fio.Text?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(passport) && string.IsNullOrWhiteSpace(fio))
            {
                NotificationsControl.ShowWarning("Введите данные гражданина", 
                    "Для поиска по гражданину укажите ФИО или паспорт");
                return;
            }
            
            citizenParams = new CitizenSearchParams
            {
                Passport = passport,
                FullName = fio,
                Birthday = dp_citizen_birthday.SelectedDate?.DateTime,
                Phone = txbx_citizen_phone.Text?.Trim(),
                Address = txbx_citizen_address.Text?.Trim()
            };
            
            if (!string.IsNullOrWhiteSpace(fio))
            {
                var parts = fio.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1) citizenParams.LastName = parts[0];
                if (parts.Length >= 2) citizenParams.FirstName = parts[1];
                if (parts.Length >= 3) citizenParams.Patronymic = parts[2];
            }
        }
        
        string searchNumber = "";
        string docType = "Все";
        
        if (!string.IsNullOrWhiteSpace(reportNumber))
        {
            searchNumber = reportNumber;
            docType = "Направление на мед. освид.";
        }
        else if (!string.IsNullOrWhiteSpace(certNumber))
        {
            searchNumber = certNumber;
            docType = "Акт медицинского освидетельствования";
        }
        
        // ✅ Врач: searchValue - номер документа, filterValue - пустая строка
        var documentsWindow = new YourDocumentsWindow(this, App.CurrentUserId, searchNumber, date, citizenParams, docType);
        documentsWindow.Show();
        this.Close();
    }

    // ==========================================
    // СУДЬЯ - поиск документов
    // ==========================================
    private void BtnJudgeDocSearch_Click(object? sender, RoutedEventArgs e)
    {
        bool searchByCitizen = chk_judge_search_doc.IsChecked == true;
        
        string number = txbx_judge_number.Text?.Trim() ?? "";
        string docType = (cmb_judge_doc_type.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";
        DateTime? date = dp_judge_doc_date.SelectedDate?.DateTime;
        
        // Проверка: если не ищем по гражданину, то номер обязателен
        if (!searchByCitizen && string.IsNullOrWhiteSpace(number))
        {
            NotificationsControl.ShowWarning("Введите номер", "Укажите номер документа");
            return;
        }
        
        CitizenSearchParams? citizenParams = null;
        if (searchByCitizen)
        {
            string passport = txbx_citizen_passport.Text?.Trim() ?? "";
            string fio = txbx_citizen_fio.Text?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(passport) && string.IsNullOrWhiteSpace(fio))
            {
                NotificationsControl.ShowWarning("Введите данные гражданина", 
                    "Для поиска по гражданину укажите ФИО или паспорт");
                return;
            }
            
            citizenParams = new CitizenSearchParams
            {
                Passport = passport,
                FullName = fio,
                Birthday = dp_citizen_birthday.SelectedDate?.DateTime,
                Phone = txbx_citizen_phone.Text?.Trim(),
                Address = txbx_citizen_address.Text?.Trim()
            };
            
            if (!string.IsNullOrWhiteSpace(fio))
            {
                var parts = fio.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1) citizenParams.LastName = parts[0];
                if (parts.Length >= 2) citizenParams.FirstName = parts[1];
                if (parts.Length >= 3) citizenParams.Patronymic = parts[2];
            }
        }
        
        // ✅ Судья: searchValue = number, filterValue = "" (пустая строка)
        new YourDocumentsWindow(this, App.CurrentUserId, number, date, citizenParams, docType).Show();
        this.Close();
    }

    // ==========================================
    // ЭКСПЕРТ - поиск документов
    // ==========================================
    private void BtnForensicDocSearch_Click(object? sender, RoutedEventArgs e)
    {
        bool searchByCitizen = chk_forensic_search_doc.IsChecked == true;
        
        string number = txbx_forensic_number.Text?.Trim() ?? "";
        DateTime? date = dp_forensic_doc_date.SelectedDate?.DateTime;
        
        // Проверка: если не ищем по гражданину, то номер обязателен
        if (!searchByCitizen && string.IsNullOrWhiteSpace(number))
        {
            NotificationsControl.ShowWarning("Введите номер", "Укажите номер экспертизы");
            return;
        }
        
        CitizenSearchParams? citizenParams = null;
        if (searchByCitizen)
        {
            string passport = txbx_citizen_passport.Text?.Trim() ?? "";
            string fio = txbx_citizen_fio.Text?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(passport) && string.IsNullOrWhiteSpace(fio))
            {
                NotificationsControl.ShowWarning("Введите данные гражданина", 
                    "Для поиска по гражданину укажите ФИО или паспорт");
                return;
            }
            
            citizenParams = new CitizenSearchParams
            {
                Passport = passport,
                FullName = fio,
                Birthday = dp_citizen_birthday.SelectedDate?.DateTime,
                Phone = txbx_citizen_phone.Text?.Trim(),
                Address = txbx_citizen_address.Text?.Trim()
            };
            
            if (!string.IsNullOrWhiteSpace(fio))
            {
                var parts = fio.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1) citizenParams.LastName = parts[0];
                if (parts.Length >= 2) citizenParams.FirstName = parts[1];
                if (parts.Length >= 3) citizenParams.Patronymic = parts[2];
            }
        }
        
        // ✅ Эксперт: searchValue = number, filterValue = "" (пустая строка)
        new YourDocumentsWindow(this, App.CurrentUserId, number, date, citizenParams).Show();
        this.Close();
    }
        
    // ==========================================
    // ЗАГРУЗКА ДАННЫХ
    // ==========================================
    private async Task LoadRecentDocumentsAsync()
    {
        try
        {
            if (_db == null) return;
            
            
            var recentDocs = await _db.GetAllDocumentsAsync(_currentUserRole, App.CurrentUserId);
            
            
            recentDocumentsList.ItemsSource = recentDocs.Take(10).ToList();
            txtNoRecent.IsVisible = recentDocs.Count == 0;
            
            await Task.Delay(100);
            SubscribeToRecentButtons();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"LoadRecentDocuments: {ex.Message}");
            Console.WriteLine($"[ERROR] LoadRecentDocuments: {ex.Message}");
            Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
        }
    }
        
    private async Task LoadDraftsAsync()
    {
        try
        {
            if (_db == null) return;
            _drafts = await _db.GetDraftsAsync(App.CurrentUserId);
            draftsList.ItemsSource = _drafts.Take(10).ToList();
            await Task.Delay(100);
            SubscribeToDraftButtons();
            txtNoDrafts.IsVisible = _drafts.Count == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDrafts: {ex.Message}");
        }
    }
    
    // ==========================================
    // ПОДПИСКИ НА КНОПКИ В СПИСКАХ
    // ==========================================
    private void SubscribeToRecentButtons()
    {
        var buttons = recentDocumentsList.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Name == "btnOpenRecent")
            .ToList();
        
        foreach (var btn in buttons)
        {
            btn.Click -= OnRecentOpenClick;
            btn.Click += OnRecentOpenClick;
            
            // ✅ Добавить установку Tag
            var doc = btn.DataContext as RecentDocument;
            if (doc != null)
            {
                btn.Tag = doc;
            }
        }
    }

    private void SubscribeToDraftButtons()
    {
        var buttons = draftsList.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Name == "btnOpenDraft")
            .ToList();

        
        foreach (var btn in buttons)
        {
            btn.Click -= OnDraftOpenClick;
            btn.Click += OnDraftOpenClick;
        }
    }
    // ==========================================
    // ОТКРЫТИЕ ДОКУМЕНТОВ ИЗ СПИСКОВ
    // ==========================================
    private async void OnRecentOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is RecentDocument doc)
        {
            // Определяем тип документа по DocumentTypeId
            string tableName = doc.DocumentTypeId switch
            {
                1 => "statement",                      // Заявление
                2 => "appeals",                        // Обращение
                3 => "explanation_protocol",           // Протокол объяснения
                4 => "medical_examination_report",     // Направление на мед. освид.
                5 => "administrative_protocol",        // Административный протокол
                6 => "medical_examination_certificate", // Акт медицинского освидетельствования
                7 => "forensic_medical_examination",   // Судебно-медицинская экспертиза
                8 => "resolution",                     // Постановление
                13 => "deal",                          // ДЕЛО (добавлено!)
                _ => "unknown"
            };
            
            if (tableName != "unknown")
            {
                if (_db == null) return;
                try
                {
                    var fullDoc = await _db.GetFullDocumentAsync(tableName, doc.Id);
                    var viewer = new DocumentViewerWindow(App.CurrentUserId, fullDoc, this);
                    viewer.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    NotificationsControl.ShowError("Ошибка", $"Не удалось открыть документ: {ex.Message}");
                }
            }
            else
            {
                NotificationsControl.ShowWarning("Внимание", $"Неизвестный тип документа: {doc.DocumentType} (ID={doc.DocumentTypeId})");
            }
        }
    }

    private async void OnDraftOpenClick(object? sender, RoutedEventArgs e)
    {   
        // Проверяем тип sender
        if (sender is not Button btn)
        {
            return;
        }
        
        if (btn.Tag is not Draft draft)
        {
            return;
        }
        
        

        Window? targetWindow = draft.DocumentType switch
        {
            "appeals" => new NewAppel(App.CurrentUserId, this, draft.Id),
            "statement" => new NewStatement(App.CurrentUserId, this, draft.Id),
            "explanation_protocol" => new NewExplanationProtocol(App.CurrentUserId, this, draft.Id),
            "medical_examination_report" => new NewExaminationReport(App.CurrentUserId, this, draft.Id),
            "administrative_protocol" => new NewAdministrativeProtocol(App.CurrentUserId, this, draft.Id),
            "medical_certificate" => new NewMedicalCertificate(App.CurrentUserId, this, draft.Id),
            "forensic_expertise" => new NewForensicExpertise(App.CurrentUserId, this, draft.Id),
            "resolution" => new NewResolution(App.CurrentUserId, this, draft.Id),
            _ => null
        };
        
        
        if (targetWindow != null)
        {
            if (targetWindow is NewAppel appel) await appel.LoadDraftAsync(draft);
            else if (targetWindow is NewStatement statement) await statement.LoadDraftAsync(draft);
            else if (targetWindow is NewExplanationProtocol exp) await exp.LoadDraftAsync(draft);
            else if (targetWindow is NewExaminationReport exam) await exam.LoadDraftAsync(draft);
            else if (targetWindow is NewAdministrativeProtocol admin) await admin.LoadDraftAsync(draft);
            else if (targetWindow is NewMedicalCertificate cert) await cert.LoadDraftAsync(draft);
            else if (targetWindow is NewForensicExpertise forensic) await forensic.LoadDraftAsync(draft);
            else if (targetWindow is NewResolution resolution) await resolution.LoadDraftAsync(draft);
            targetWindow.Show();
            this.Hide();
        }
    }

    // ==========================================
    // КНОПКИ БЫСТРОГО СОЗДАНИЯ ДОКУМЕНТОВ
    // ==========================================

    private void btn_newDeal_Click(object? sender, RoutedEventArgs e)
    {
        new NewDeal(_currentUserId, _currentUserRole).Show();
        this.Close();
    }

    private void btn_newCitizen_Click(object? sender, RoutedEventArgs e)
    {
        new NewCitizen(App.CurrentUserId, App.CurrentUserRole).Show();
        this.Close();
    }
    private void btn_newAppel_click(object? sender, RoutedEventArgs e)
    {
        new NewAppel(App.CurrentUserId, this).Show();
        this.Hide();
    }
    
    private void btn_newStatement_click(object? sender, RoutedEventArgs e)
    {
        new NewStatement(App.CurrentUserId, this).Show();
        this.Close();
    }
    
    private void btn_newExplanationProtocol_click(object? sender, RoutedEventArgs e)
    {
        new NewExplanationProtocol(App.CurrentUserId, this).Show();
        this.Close();
    }
    
    private void btn_newAdministrativeProtocol_click(object? sender, RoutedEventArgs e)
    {
        new NewAdministrativeProtocol(App.CurrentUserId, this).Show();
        this.Close();
    }
    
    private void btn_newExaminationReport_click(object? sender, RoutedEventArgs e)
    {
        new NewExaminationReport(App.CurrentUserId, this).Show();
        this.Close();
    }

    private void btn_newMedicalCertificate_Click(object? sender, RoutedEventArgs e)
    {
        var newCertWindow = new NewMedicalCertificate(App.CurrentUserId, this);
        newCertWindow.Show();
        this.Hide();
    }

    private void btn_newResolution_Click(object? sender, RoutedEventArgs e)
    {
        var newResolutionWindow = new NewResolution(App.CurrentUserId, this);
        newResolutionWindow.Show();
        this.Close();
    }

    private void btn_newForensicExpertise_Click(object? sender, RoutedEventArgs e)
    {
        var newExpertiseWindow = new NewForensicExpertise(App.CurrentUserId, this);
        newExpertiseWindow.Show();
        this.Close();
    }

    // ==========================================
    // НАСТРОЙКА РОЛЕЙ
    // ==========================================
    private async Task ConfigureForRole()
    {
        DocSearchPolice.IsVisible = false;
        DocSearchDoctor.IsVisible = false;
        DocSearchJudge.IsVisible = false;
        DocSearchForensic.IsVisible = false;
        
        btn_newAppel.IsVisible = false;
        btn_newStatement.IsVisible = false;
        btn_newExplanationProtocol.IsVisible = false;
        btn_newExaminationReport.IsVisible = false;
        btn_newAdministrativeProtocol.IsVisible = false;
        btn_newMedicalCertificate.IsVisible = false;
        btn_newResolution.IsVisible = false;
        btn_newForensicExpertise.IsVisible = false;
        
        switch (App.CurrentUserRole)
        {
            case UserRole.AdminInspector:
                DocSearchPolice.IsVisible = true;
                btn_newAppel.IsVisible = true;
                btn_newStatement.IsVisible = true;
                btn_newExplanationProtocol.IsVisible = true;
                btn_newExaminationReport.IsVisible = true;
                btn_newAdministrativeProtocol.IsVisible = true;
                btn_newDeal.IsVisible = true;        // ← показать кнопку Дело
                btn_newCitizen.IsVisible = true;
                break;
            case UserRole.ChiefOfPolice:
                DocSearchPolice.IsVisible = true;
                btn_newAppel.IsVisible = true;
                btn_newStatement.IsVisible = true;
                btn_newExplanationProtocol.IsVisible = true;
                btn_newExaminationReport.IsVisible = true;
                btn_newAdministrativeProtocol.IsVisible = true;
                btn_newMedicalCertificate.IsVisible = false;
                btn_newResolution.IsVisible = false;
                btn_newForensicExpertise.IsVisible = false;
                break;

            case UserRole.MedicalExpert:
                DocSearchDoctor.IsVisible = true;
                btn_newMedicalCertificate.IsVisible = true;
                break;
                
            case UserRole.Judge:
                DocSearchJudge.IsVisible = true;
                btn_newResolution.IsVisible = true;
                break;
                
            case UserRole.ForensicExpert:
                DocSearchForensic.IsVisible = true;
                btn_newForensicExpertise.IsVisible = true;
                var expertPostId = await _db.GetCitizensAndPostsIdByUserIdAsync(App.CurrentUserId);
                Console.WriteLine($"[DEBUG] citizen_post_id для эксперта: {expertPostId}");
                break;
                
            case UserRole.PoliceOfficer:
            default:
                DocSearchPolice.IsVisible = true;
                btn_newAppel.IsVisible = true;
                btn_newStatement.IsVisible = true;
                btn_newExplanationProtocol.IsVisible = true;
                btn_newExaminationReport.IsVisible = true;
                btn_newAdministrativeProtocol.IsVisible = true;
                break;
        }
    }
}