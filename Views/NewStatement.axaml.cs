#pragma warning disable CS0649
using System;
using System.Data.Common;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class NewStatement : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private int? _currentDraftId;
    private readonly Window? _previousWindow;
    
    public NewStatement(int currentUserId, Window? previousWindow = null, int? currentDraftId = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        _previousWindow = previousWindow;
        _currentDraftId = currentDraftId;

        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId, App.CurrentUserRole);
        
        btn_create.Click += Btn_create_Click;
        btn_cancel.Click += Btn_cancel_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
        btn_select_citizen.Click += Btn_select_citizen_Click;
    }

    private async void Btn_select_citizen_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var citizensWindow = new SelectCitizenWindow(_currentUserId, this);
            
            citizensWindow.Closed += (s, args) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var selectedCitizen = citizensWindow.SelectedCitizen;
                    if (selectedCitizen != null)
                    {
                        txt_applicant.Text = $"{selectedCitizen.LastName} {selectedCitizen.FirstName} {selectedCitizen.Patronymic}".Trim();
                        txt_applicant.Tag = selectedCitizen.Id;
                        txt_applicant_error.IsVisible = false;
                    }
                    this.Activate();
                });
            };
            
            citizensWindow.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
    }

    private async void Btn_create_Click(object? sender, RoutedEventArgs e)
    {
        if (txt_applicant.Tag == null)
        {
            txt_applicant_error.IsVisible = true;
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите заявителя");
            return;
        }
        txt_applicant_error.IsVisible = false;

        if (string.IsNullOrWhiteSpace(txt_content.Text))
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, заполните содержание заявления");
            return;
        }
        
        if (dp_date.SelectedDate == null)
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите дату");
            return;
        }
        
        if (tp_time.SelectedTime == null)
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите время");
            return;
        }

        try
        {
            int applicantId = (int)txt_applicant.Tag;
            string content = txt_content.Text ?? "";
            
            DateTime selectedDateTime = dp_date.SelectedDate.Value.Date + tp_time.SelectedTime.Value;
            
            int? number = null;
            if (!string.IsNullOrWhiteSpace(txt_number.Text))
            {
                if (int.TryParse(txt_number.Text, out int num))
                {
                    number = num;
                }
            }
            
            bool signatureApplicant = chk_signature_applicant.IsChecked ?? false;
            bool signatureOfficer = chk_signature_officer.IsChecked ?? false;

            int newId = await _db.CreateStatementAsync(applicantId, content, _currentUserId, number, selectedDateTime, signatureApplicant, signatureOfficer);

            if (_currentDraftId.HasValue)
            {
                await _db.DeleteDraftAsync(_currentDraftId.Value);
            }

            NotificationsControl.ShowSuccess("Успех", "Заявление успешно создано!");
            
            try
            {
                if (_previousWindow != null)
                {
                    _previousWindow.Show();
                }
            }
            catch
            {
                new MainWindow(App.CurrentUserId, App.CurrentUserRole).Show();
            }
            this.Close();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Не удалось создать заявление: {ex.Message}");
        }
    }

    private async void Btn_save_draft_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var formData = new
            {
                applicant = txt_applicant.Tag as int?,
                applicant_name = txt_applicant.Text,
                number = string.IsNullOrWhiteSpace(txt_number.Text) ? null : txt_number.Text,
                date_and_time = dp_date.SelectedDate?.ToString("yyyy-MM-dd HH:mm"),
                content = txt_content.Text ?? "",
                signature_applicant = chk_signature_applicant.IsChecked ?? false,
                signature_officer = chk_signature_officer.IsChecked ?? false
            };
            
            string formDataJson = JsonSerializer.Serialize(formData);
            
            if (_currentDraftId.HasValue)
            {
                await _db.UpdateDraftAsync(_currentDraftId.Value, formDataJson);
                NotificationsControl.ShowSuccess("Успех", "Черновик обновлён!");
            }
            else
            {
                int newDraftId = await _db.SaveDraftAsync(_currentUserId, "statement", formDataJson);
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
            
            if (draft.ApplicantId.HasValue)
            {
                var citizen = await _db.GetCitizenByIdAsync(draft.ApplicantId.Value);
                if (citizen != null)
                {
                    txt_applicant.Text = citizen.FullName;
                    txt_applicant.Tag = citizen.Id;
                }
            }
            
            if (!string.IsNullOrWhiteSpace(draft.Number))
            {
                txt_number.Text = draft.Number;
            }
            
            if (draft.DocumentDate.HasValue)
            {
                dp_date.SelectedDate = draft.DocumentDate.Value.Date;
                tp_time.SelectedTime = new TimeSpan(draft.DocumentDate.Value.Hour, 
                                                    draft.DocumentDate.Value.Minute, 0);
            }
            
            if (!string.IsNullOrWhiteSpace(draft.Content))
            {
                txt_content.Text = draft.Content;
            }
            
            if (draft.SignatureApplicant.HasValue)
            {
                chk_signature_applicant.IsChecked = draft.SignatureApplicant.Value;
            }
            
            if (draft.SignatureOfficer.HasValue)
            {
                chk_signature_officer.IsChecked = draft.SignatureOfficer.Value;
            }
            
            Console.WriteLine($"[DEBUG] Загружен черновик ID: {_currentDraftId} для заявления");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDraftAsync: {ex.Message}");
        }
    }
}