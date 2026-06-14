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

public partial class NewExaminationReport : Window
{
    private readonly DatabaseHelper? _db;
    private int? _currentDraftId;
    private readonly int _currentUserId;
    private readonly Window? _previousWindow;   
    
    public NewExaminationReport(int currentUserId, Window? previousWindow = null, int? currentDraftId = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _currentDraftId = currentDraftId;
        _db = new DatabaseHelper();
        _previousWindow = previousWindow;

        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        btn_create.Click += Btn_create_Click;
        btn_cancel.Click += Btn_cancel_Click;
        btn_select_patient.Click += Btn_select_patient_Click;
        btn_select_deal.Click += Btn_select_deal_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
    }
    
    private async void Btn_select_patient_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var citizensWindow = new SelectCitizenWindow(App.CurrentUserId, this);
            citizensWindow.Closed += (s, args) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var selected = citizensWindow.SelectedCitizen;
                    if (selected != null)
                    {
                        txt_patient.Text = $"{selected.LastName} {selected.FirstName} {selected.Patronymic}".Trim();
                        txt_patient.Tag = selected.Id;
                        txt_deal_error.IsVisible = false;
                    }
                    Activate();
                });
            };
            citizensWindow.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Select patient: {ex.Message}");
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
        if (txt_patient.Tag == null)
        {
            txt_deal_error.IsVisible = true;
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите пациента");
            return;
        }
        txt_deal_error.IsVisible = false;

        if (string.IsNullOrWhiteSpace(txt_content.Text))
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, заполните содержание направления");
            return;
        }
        
        if (cmb_report_type.SelectedItem == null)
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите тип освидетельствования");
            return;
        }
        
        if (dp_date.SelectedDate == null || tp_time.SelectedTime == null)
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите дату и время");
            return;
        }

        try
        {
            int patientId = (int)txt_patient.Tag;
            int? dealId = txt_deal.Tag as int?;
            
            string reportType = "";
            if (cmb_report_type.SelectedItem is ComboBoxItem selectedItem)
            {
                reportType = selectedItem.Content?.ToString() ?? "";
            }
            
            Console.WriteLine($"[DEBUG] Выбран тип освидетельствования: '{reportType}'");
            
            if (string.IsNullOrWhiteSpace(reportType))
            {
                NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите тип освидетельствования");
                return;
            }
            
            string content = txt_content.Text ?? "";
            string signs = txt_signs.Text ?? "";
            DateTime selectedDateTime = dp_date.SelectedDate.Value.Date + tp_time.SelectedTime.Value;
            
            int? number = null;
            if (!string.IsNullOrWhiteSpace(txt_number.Text) && int.TryParse(txt_number.Text, out int num))
                number = num;

            bool citizenSig = chk_citizen_signature.IsChecked ?? false;
            bool officerSig = chk_officer_signature.IsChecked ?? false;

            int newId = await _db.CreateMedicalExaminationReportAsync(
                patientId, dealId, reportType, content, signs, 
                number, selectedDateTime, citizenSig, officerSig);

            NotificationsControl.ShowSuccess("Успех", "Направление создано!");
            
            // ✅ Возврат в предыдущее окно
            if (_previousWindow != null)
            {
                new MainWindow(App.CurrentUserId, App.CurrentUserRole).Show();
            }
            this.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            NotificationsControl.ShowError("Ошибка", ex.Message);
        }
    }
    
    private async void Btn_save_draft_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var formData = new
            {
                patient_id = txt_patient.Tag as int?,
                patient_name = txt_patient.Text,
                deal_id = txt_deal.Tag as int?,
                deal_name = txt_deal.Text,
                number = string.IsNullOrWhiteSpace(txt_number.Text) ? null : txt_number.Text,
                report_type = (cmb_report_type.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                making_date = dp_date.SelectedDate?.ToString("yyyy-MM-dd"),
                making_time = tp_time.SelectedTime?.ToString(),
                content = txt_content.Text ?? "",
                signs = txt_signs.Text ?? "",
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
                int newDraftId = await _db.SaveDraftAsync(_currentUserId, "medical_examination_report", formDataJson);
                _currentDraftId = newDraftId;
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
            
            if (draft.PatientId.HasValue)
            {
                var citizen = await _db.GetCitizenByIdAsync(draft.PatientId.Value);
                if (citizen != null)
                {
                    txt_patient.Text = citizen.FullName;
                    txt_patient.Tag = citizen.Id;
                }
            }
            
            if (draft.DealId.HasValue)
            {
                txt_deal.Tag = draft.DealId.Value;
                txt_deal.Text = draft.DealId.Value.ToString();
            }
            
            if (!string.IsNullOrWhiteSpace(draft.Number))
            {
                txt_number.Text = draft.Number;
            }
            
            if (draft.ReportTypeId.HasValue)
            {
                cmb_report_type.SelectedIndex = draft.ReportTypeId.Value - 1;
            }
            
            if (draft.DocumentDate.HasValue)
            {
                dp_date.SelectedDate = draft.DocumentDate.Value.Date;
                tp_time.SelectedTime = new TimeSpan(draft.DocumentDate.Value.Hour, draft.DocumentDate.Value.Minute, 0);
            }
            
            if (!string.IsNullOrWhiteSpace(draft.Content))
            {
                txt_content.Text = draft.Content;
            }
            
            if (!string.IsNullOrWhiteSpace(draft.Signs))
            {
                txt_signs.Text = draft.Signs;
            }
            
            if (draft.SignatureApplicant.HasValue)
            {
                chk_citizen_signature.IsChecked = draft.SignatureApplicant.Value;
            }
            
            if (draft.SignatureOfficer.HasValue)
            {
                chk_officer_signature.IsChecked = draft.SignatureOfficer.Value;
            }
            
            Console.WriteLine($"[DEBUG] Загружен черновик ID: {_currentDraftId} для направления на мед. освид.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDraftAsync: {ex.Message}");
        }
    }
}