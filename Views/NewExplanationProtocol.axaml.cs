#pragma warning disable CS0649
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class NewExplanationProtocol : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private readonly Window? _previousWindow;
    private int? _currentDraftId;
    
    public NewExplanationProtocol(int currentUserId, Window? previousWindow = null, int? currentDraftId = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        _previousWindow = previousWindow;
        _currentDraftId = currentDraftId;
        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        SetupFormButtons();
    }

    private void SetupFormButtons()
    {
        btn_select_citizen.Click += Btn_select_citizen_Click;
        btn_select_deal.Click += Btn_select_deal_Click;
        btn_create.Click += Btn_create_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
        btn_cancel.Click += Btn_cancel_Click;
    }

    private async void Btn_select_citizen_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var citizensWindow = new SelectCitizenWindow(App.CurrentUserId, this);
            
            citizensWindow.Closed += (s, args) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var selectedCitizen = citizensWindow.SelectedCitizen;
                    if (selectedCitizen != null)
                    {
                        txt_citizen.Text = selectedCitizen.FullName;
                        txt_citizen.Tag = selectedCitizen.Id;
                        txt_citizen_error.IsVisible = false;
                    }
                    Activate();
                });
            };
            
            citizensWindow.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Btn_select_citizen_Click: {ex.Message}");
        }
    }

    private async void Btn_select_deal_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dealWindow = new SelectDealWindow(App.CurrentUserId, this);
            
            dealWindow.Closed += (s, args) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var selectedDeal = dealWindow.SelectedDeal;
                    if (selectedDeal != null)
                    {
                        txt_deal.Text = $"{selectedDeal.Number} - {selectedDeal.CitizenFullName}";
                        txt_deal.Tag = selectedDeal.Id;
                        txt_deal_error.IsVisible = false;
                    }
                    Activate();
                });
            };
            
            await dealWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Btn_select_deal_Click: {ex.Message}");
            NotificationsControl.ShowError("Ошибка", $"Не удалось открыть окно выбора дела: {ex.Message}");
        }
    }

    private async void Btn_create_Click(object? sender, RoutedEventArgs e)
    {
        if (txt_citizen.Tag == null)
        {
            txt_citizen_error.IsVisible = true;
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите гражданина");
            return;
        }
        txt_citizen_error.IsVisible = false;

        if (txt_deal.Tag == null)
        {
            txt_deal_error.IsVisible = true;
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите дело");
            return;
        }
        txt_deal_error.IsVisible = false;

        if (string.IsNullOrWhiteSpace(txt_content.Text))
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, заполните содержание объяснения");
            return;
        }

        try
        {
            int citizenId = (int)txt_citizen.Tag;
            int dealId = (int)txt_deal.Tag;
            string content = txt_content.Text ?? "";
            
            int? number = null;
            if (!string.IsNullOrWhiteSpace(txt_number.Text) && int.TryParse(txt_number.Text, out int num))
                number = num;

            int newId = await _db.CreateExplanationProtocolAsync(citizenId, dealId, content, number);

            if (_currentDraftId.HasValue)
                await _db.DeleteDraftAsync(_currentDraftId.Value);

            NotificationsControl.ShowSuccess("Успех", "Протокол объяснения успешно создан!");
            
            // ✅ Возврат в предыдущее окно
            if (_previousWindow != null)
            {
                _previousWindow.Show();
            }
            this.Close();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Не удалось создать протокол: {ex.Message}");
        }
    }

    private async void Btn_save_draft_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var formData = new
            {
                citizen = txt_citizen.Tag as int?,
                citizen_name = txt_citizen.Text,
                deal = txt_deal.Tag as int?,
                deal_name = txt_deal.Text,
                number = txt_number.Text,
                making_date = dp_date.SelectedDate?.ToString("yyyy-MM-dd"),
                making_time = tp_time.SelectedTime?.ToString(),
                content = txt_content.Text,
                need_forensic_examination = chk_forensic_examination.IsChecked ?? false,
                need_medical_certificate = chk_medical_certificate.IsChecked ?? false,
                citizen_signature = chk_citizen_signature.IsChecked ?? false,
                officer_signature = chk_officer_signature.IsChecked ?? false
            };
            
            string formDataJson = JsonSerializer.Serialize(formData);
            
            if (_currentDraftId.HasValue)
            {
                await _db.UpdateDraftAsync(_currentDraftId.Value, formDataJson);
                NotificationsControl.ShowSuccess("Успех", "Черновик обновлён!");
            }
            else
            {
                int newId = await _db.SaveDraftAsync(_currentUserId, "explanation_protocol", formDataJson);
                _currentDraftId = newId;
                NotificationsControl.ShowSuccess("Успех", "Черновик сохранён!");
            }
            
            new MainWindow(App.CurrentUserId, App.CurrentUserRole).Show();
            this.Close();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Не удалось сохранить черновик: {ex.Message}");
        }
    }

    private void Btn_cancel_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // ✅ Возврат в предыдущее окно
            if (_previousWindow != null)
            {
                _previousWindow.Show();
            }
        }
        catch
        {
            new MainWindow(App.CurrentUserId).Show();
        }
        
        this.Close();
    }
    
    public async Task LoadDraftAsync(Draft draft)
    {
        try
        {
            _currentDraftId = draft.Id;
            
            if (draft.CitizenId.HasValue)
            {
                var citizen = await _db.GetCitizenByIdAsync(draft.CitizenId.Value);
                if (citizen != null)
                {
                    txt_citizen.Text = citizen.FullName;
                    txt_citizen.Tag = citizen.Id;
                }
            }
            
            if (draft.DealId.HasValue)
            {
                txt_deal.Tag = draft.DealId;
                txt_deal.Text = draft.DealId.ToString();
            }
            
            if (!string.IsNullOrWhiteSpace(draft.Number))
                txt_number.Text = draft.Number;
            
            if (draft.DocumentDate.HasValue)
            {
                dp_date.SelectedDate = draft.DocumentDate.Value.Date;
                tp_time.SelectedTime = new TimeSpan(draft.DocumentDate.Value.Hour, draft.DocumentDate.Value.Minute, 0);
            }
            
            if (!string.IsNullOrWhiteSpace(draft.Content))
                txt_content.Text = draft.Content;
            
            if (draft.NeedMedicalExamination.HasValue)
                chk_forensic_examination.IsChecked = draft.NeedMedicalExamination.Value;
            
            if (draft.NeedCertificate.HasValue)
                chk_medical_certificate.IsChecked = draft.NeedCertificate.Value;
            
            if (draft.SignatureApplicant.HasValue)
                chk_citizen_signature.IsChecked = draft.SignatureApplicant.Value;
            
            if (draft.SignatureOfficer.HasValue)
                chk_officer_signature.IsChecked = draft.SignatureOfficer.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDraftAsync: {ex.Message}");
        }
    }
}