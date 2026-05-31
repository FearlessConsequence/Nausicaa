using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CourseWork.Data;
using CourseWork.Models;
using CourseWork.Controls;

namespace CourseWork.Views;

public partial class DraftsWindow : Window
{
    private readonly DatabaseHelper? _db;
    private readonly int _currentUserId;
    private List<Draft> _all = new();
    private List<Draft> _shown = new();
    private readonly Window? _previousWindow;

    public DraftsWindow(int userId, Window? previousWindow = null)
    {
        InitializeComponent();
        _currentUserId = userId;
        _db = new DatabaseHelper();
        _previousWindow = previousWindow;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        // ✅ Настраиваем ComboBox в зависимости от роли
        ConfigureFilterByRole();
        
        cmb_filterType.SelectedIndex = 0;
        
        this.Opened += async (_, _) => await _load();
    }

    // ✅ Настройка ComboBox по роли
    private void ConfigureFilterByRole()
    {
        var role = App.CurrentUserRole;
        
        switch (role)
        {
            case UserRole.PoliceOfficer:
            case UserRole.AdminInspector:
                // Полицейский/Инспектор: показываем бордер с ComboBox
                FilterBorder.IsVisible = true;
                cmb_filterType.Items.Clear();
                cmb_filterType.Items.Add(new ComboBoxItem { Content = "Все" });
                cmb_filterType.Items.Add(new ComboBoxItem { Content = "Обращение" });
                cmb_filterType.Items.Add(new ComboBoxItem { Content = "Заявление" });
                cmb_filterType.Items.Add(new ComboBoxItem { Content = "Административный протокол" });
                cmb_filterType.Items.Add(new ComboBoxItem { Content = "Протокол объяснения" });
                cmb_filterType.Items.Add(new ComboBoxItem { Content = "Направление на мед. освид." });
                cmb_filterType.SelectedIndex = 0;
                break;
                
            case UserRole.MedicalExpert:
            case UserRole.Judge:
            case UserRole.ForensicExpert:
                // Врач, судья, эксперт - скрываем весь бордер
                FilterBorder.IsVisible = false;
                break;
        }
    }

    private async Task _load()
    {
        try
        {
            if (_db == null) return;
            _all = await _db.GetDraftsAsync(App.CurrentUserId);
            _applyFilter();
            
            await Task.Delay(100);
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] {ex.Message}");
            NotificationsControl.ShowError("Ошибка", $"Не удалось загрузить черновики: {ex.Message}");
        }
    }

    private void SubscribeToButtons()
    {
        var buttons = draftsList.GetVisualDescendants()
            .OfType<Button>()
            .ToList();
            
        foreach (var button in buttons)
        {
            if (button.Name == "BtnEdit")
            {
                button.Click -= BtnEdit_Click;
                button.Click += BtnEdit_Click;
            }
            else if (button.Name == "BtnDelete")
            {
                button.Click -= BtnDelete_Click;
                button.Click += BtnDelete_Click;
            }
        }
    }

    private void _applyFilter()
    {
        var role = App.CurrentUserRole;
        var sel = cmb_filterType.IsVisible ? (cmb_filterType.SelectedItem as ComboBoxItem)?.Content?.ToString() : "Все";
        
        _shown = string.IsNullOrEmpty(sel) || sel == "Все" 
            ? _all 
            : _all.Where(d => d.TypeDisplayName == sel).ToList();
        
        // Если роль не полицейский - показываем только их тип документов
        if (role == UserRole.MedicalExpert)
        {
            _shown = _shown.Where(d => d.TypeDisplayName == "Акт медицинского освидетельствования").ToList();
        }
        else if (role == UserRole.Judge)
        {
            _shown = _shown.Where(d => d.TypeDisplayName == "Постановление").ToList();
        }
        else if (role == UserRole.ForensicExpert)
        {
            _shown = _shown.Where(d => d.TypeDisplayName == "Судебно-медицинская экспертиза").ToList();
        }
        
        for (int i = 0; i < _shown.Count; i++)
        {
            _shown[i].DraftNumber = i + 1;
        }
        
        draftsList.ItemsSource = _shown;
        txt_empty.IsVisible = _shown.Count == 0;
        txt_draftsCount.Text = $"Всего: {_all.Count}";
    }
    
    private void CmbFilter_SelectionChanged(object? s, SelectionChangedEventArgs e)
    {
        _applyFilter();
        SubscribeToButtons();
    }

    private async void BtnEdit_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int id)
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
                            var appelWindow = new NewAppel(App.CurrentUserId, this, draft.Id);
                            await appelWindow.LoadDraftAsync(draft);
                            targetWindow = appelWindow;
                            break;
                            
                        case "statement":
                            var statementWindow = new NewStatement(App.CurrentUserId, this, draft.Id);
                            await statementWindow.LoadDraftAsync(draft);
                            targetWindow = statementWindow;
                            break;
                            
                        case "administrative_protocol":
                            var adminWindow = new NewAdministrativeProtocol(App.CurrentUserId, this, draft.Id);
                            await adminWindow.LoadDraftAsync(draft);
                            targetWindow = adminWindow;
                            break;
                            
                        case "explanation_protocol":
                            var explanationWindow = new NewExplanationProtocol(App.CurrentUserId, this, draft.Id);
                            await explanationWindow.LoadDraftAsync(draft);
                            targetWindow = explanationWindow;
                            break;
                            
                        case "medical_examination_report":
                            var examWindow = new NewExaminationReport(App.CurrentUserId, this, draft.Id);
                            await examWindow.LoadDraftAsync(draft);
                            targetWindow = examWindow;
                            break;
                            
                        case "medical_certificate":
                            var certWindow = new NewMedicalCertificate(App.CurrentUserId, this, draft.Id);
                            await certWindow.LoadDraftAsync(draft);
                            targetWindow = certWindow;
                            break;
                            
                        case "resolution":
                            var resolutionWindow = new NewResolution(App.CurrentUserId, this, draft.Id);
                            await resolutionWindow.LoadDraftAsync(draft);
                            targetWindow = resolutionWindow;
                            break;
                            
                        case "forensic_expertise":
                            var forensicWindow = new NewForensicExpertise(App.CurrentUserId, this, draft.Id);
                            await forensicWindow.LoadDraftAsync(draft);
                            targetWindow = forensicWindow;
                            break;
                    }
                    
                    if (targetWindow != null)
                    {
                        targetWindow.Show();
                        this.Hide();
                    }
                    else
                    {
                        NotificationsControl.ShowError("Ошибка", $"Неизвестный тип документа: {draft.DocumentType}");
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

    private async void BtnDelete_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int id)
        {
            try
            {
                if (_db == null) return;
                await _db.DeleteDraftAsync(id);
                await _load();
                NotificationsControl.ShowSuccess("Черновик удален", $"Черновик успешно удален");
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", $"Не удалось удалить черновик: {ex.Message}");
            }
        }
    }
}