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

public partial class NewAppel : Window
{
    private readonly DatabaseHelper? _db;
    private int? _currentDraftId;
    private readonly Window? _previousWindow;
    private readonly int _currentUserId;

    public NewAppel(int currentUserId, Window? previousWindow = null, int? currentDraftId = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _currentDraftId = currentDraftId;
        _previousWindow = previousWindow;
        _db = new DatabaseHelper();

        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        btn_create.Click += Btn_create_Click;
        btn_cancel.Click += Btn_cancel_Click;
        btn_select_citizen.Click += Btn_select_citizen_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
    }

    private async void Btn_select_citizen_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var citizensWindow = new SelectCitizenWindow(App.CurrentUserId, this);
            citizensWindow.Show();
            
            citizensWindow.Closed += (s, args) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var selectedCitizen = citizensWindow.SelectedCitizen;
                    if (selectedCitizen != null)
                    {
                        txt_citizen.Text = $"{selectedCitizen.LastName} {selectedCitizen.FirstName} {selectedCitizen.Patronymic}".Trim();
                        txt_citizen.Tag = selectedCitizen.Id;
                        txt_citizen_error.IsVisible = false;
                    }
                    Activate();
                });
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] ОШИБКА: {ex.Message}");
        }
    }

    private async void Btn_create_Click(object? sender, RoutedEventArgs e)
    {
        if (txt_citizen.Tag == null)
        {
            txt_citizen_error.IsVisible = true;
            NotificationsControl.ShowWarning("Внимание", "Выберите гражданина");
            return;
        }
        txt_citizen_error.IsVisible = false;

        if (string.IsNullOrWhiteSpace(txt_content.Text))
        {
            NotificationsControl.ShowWarning("Внимание", "Заполните содержание обращения");
            return;
        }
        
        if (dp_date.SelectedDate == null)
        {
            NotificationsControl.ShowWarning("Внимание", "Выберите дату");
            return;
        }
        
        if (tp_time.SelectedTime == null)
        {
            NotificationsControl.ShowWarning("Внимание", "Выберите время");
            return;
        }

        try
        {
            int citizenId = (int)txt_citizen.Tag;
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
            
            int newId = await _db.CreateAppealAsync(citizenId, content, _currentUserId, number, selectedDateTime);

            if (_currentDraftId.HasValue)
            {
                await _db.DeleteDraftAsync(_currentDraftId.Value);
            }

            NotificationsControl.ShowSuccess("Успех", "Обращение успешно создано");
            
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
            Console.WriteLine($"[ERROR] {ex.Message}");
            NotificationsControl.ShowError("Ошибка", $"Не удалось создать обращение: {ex.Message}");
        }
    }

    private async void Btn_save_draft_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var formData = new
            {
                appeal_citizen = txt_citizen.Tag as int?,
                citizen_name = txt_citizen.Text,
                number = string.IsNullOrWhiteSpace(txt_number.Text) ? null : txt_number.Text,
                making_date = dp_date.SelectedDate?.ToString("yyyy-MM-dd"),
                making_time = tp_time.SelectedTime?.ToString(),
                content = txt_content.Text ?? "",
                has_citizen = txt_citizen.Tag != null,
                has_number = !string.IsNullOrWhiteSpace(txt_number.Text),
                has_date = dp_date.SelectedDate != null,
                has_content = !string.IsNullOrWhiteSpace(txt_content.Text)
            };
            
            string formDataJson = JsonSerializer.Serialize(formData);
            
            if (_currentDraftId.HasValue)
            {
                await _db.UpdateDraftAsync(_currentDraftId.Value, formDataJson);
                NotificationsControl.ShowSuccess("Успех", "Черновик обновлён");
            }
            else
            {
                int newDraftId = await _db.SaveDraftAsync(_currentUserId, "appeals", formDataJson);
                _currentDraftId = newDraftId;
                NotificationsControl.ShowSuccess("Успех", "Черновик сохранён");
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
            
            Console.WriteLine($"[DEBUG] Загружен черновик ID: {_currentDraftId}, данные: {draft.Content?.Length ?? 0} символов");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDraftAsync: {ex.Message}");
        }
    }
}