using System;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class NewForensicExpertise : Window
{
    private readonly int _currentUserId;
    private int? _currentDraftId;
    private readonly Window? _previousWindow;
    private readonly DatabaseHelper _db;

    public NewForensicExpertise(int currentUserId, Window? previousWindow = null, int? currentDraftId = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _currentDraftId = currentDraftId;
        _db = new DatabaseHelper();
        _previousWindow = previousWindow;

        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

        btn_select_deal.Click += Btn_select_deal_Click;
        btn_cancel.Click += Btn_cancel_Click;
        btn_create.Click += Btn_create_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
    }

    private async void Btn_select_deal_Click(object? sender, RoutedEventArgs e)
    {
        var dealWindow = new SelectDealWindow(App.CurrentUserId, this);
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
        await dealWindow.ShowDialog(this);
    }
    
    private async void Btn_create_Click(object? sender, RoutedEventArgs e)
    {
        if (txt_deal.Tag == null)
        {
            NotificationsControl.ShowWarning("Внимание", "Выберите дело");
            return;
        }

        if (string.IsNullOrWhiteSpace(txt_number.Text))
        {
            NotificationsControl.ShowWarning("Внимание", "Введите номер экспертизы");
            return;
        }

        if (dp_date.SelectedDate == null)
        {
            NotificationsControl.ShowWarning("Внимание", "Выберите дату");
            return;
        }

        if (string.IsNullOrWhiteSpace(txt_content.Text))
        {
            NotificationsControl.ShowWarning("Внимание", "Заполните содержание экспертизы");
            return;
        }

        try
        {
            int dealId = (int)txt_deal.Tag;
            int number = int.Parse(txt_number.Text);
            DateTime dateTime = dp_date.SelectedDate.Value.Date + tp_time.SelectedTime.Value;
            string content = txt_content.Text.Trim();
            
            int? expertId = await _db.GetCitizensAndPostsIdByUserIdAsync(_currentUserId);
            if (!expertId.HasValue)
            {
                NotificationsControl.ShowError("Ошибка", "Эксперт не найден в системе");
                return;
            }

            bool physicalInjuries = chk_physical_injuries.IsChecked ?? false;
            bool severityHarm = chk_severity.IsChecked ?? false;
            bool couldOccur = chk_could_occur.IsChecked ?? false;
            bool signatureExpert = chk_signature.IsChecked ?? false;

            int structureId = 1;
            int newId = await _db.CreateForensicExpertiseAsync(
                dealId: dealId,
                number: number,
                dateTime: dateTime,
                structureId: structureId,
                expertId: expertId.Value,
                content: content,
                physicalInjuries: physicalInjuries,
                severityHarm: severityHarm,
                couldOccur: couldOccur,
                signatureExpert: signatureExpert
            );

            if (_currentDraftId.HasValue)
                await _db.DeleteDraftAsync(_currentDraftId.Value);

            NotificationsControl.ShowSuccess("Успех", $"Судебно-медицинская экспертиза №{number} создана!");
            
            // ✅ Возврат в предыдущее окно
            if (_previousWindow != null)
            {
                _previousWindow.Show();
            }
            this.Close();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Не удалось создать экспертизу: {ex.Message}");
        }
    }

    private async void Btn_save_draft_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var formData = new
            {
                deal_id = txt_deal.Tag as int?,
                deal_name = txt_deal.Text,
                number = txt_number.Text,
                making_date = dp_date.SelectedDate?.ToString("yyyy-MM-dd"),
                making_time = tp_time.SelectedTime?.ToString(),
                content = txt_content.Text,
                conclusion = txt_conclusion.Text,
                physical_injuries = chk_physical_injuries.IsChecked ?? false,
                severity = chk_severity.IsChecked ?? false,
                could_occur = chk_could_occur.IsChecked ?? false,
                signature = chk_signature.IsChecked ?? false
            };
            
            string formDataJson = JsonSerializer.Serialize(formData);
            
            if (_currentDraftId.HasValue)
            {
                await _db.UpdateDraftAsync(_currentDraftId.Value, formDataJson);
                NotificationsControl.ShowSuccess("Успех", "Черновик обновлён!");
            }
            else
            {
                int newDraftId = await _db.SaveDraftAsync(_currentUserId, "forensic_expertise", formDataJson);
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
            
            if (string.IsNullOrEmpty(draft.FormDataJson)) return;
            
            using var doc = JsonDocument.Parse(draft.FormDataJson);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("number", out var num) && num.ValueKind != JsonValueKind.Null)
                txt_number.Text = num.GetString();
            
            if (root.TryGetProperty("making_date", out var date) && date.ValueKind != JsonValueKind.Null && DateTime.TryParse(date.GetString(), out DateTime d))
                dp_date.SelectedDate = d;
            
            if (root.TryGetProperty("making_time", out var time) && time.ValueKind != JsonValueKind.Null && TimeSpan.TryParse(time.GetString(), out TimeSpan t))
                tp_time.SelectedTime = t;
            
            if (root.TryGetProperty("content", out var content) && content.ValueKind != JsonValueKind.Null)
                txt_content.Text = content.GetString();
            
            if (root.TryGetProperty("conclusion", out var conclusion) && conclusion.ValueKind != JsonValueKind.Null)
                txt_conclusion.Text = conclusion.GetString();
            
            if (root.TryGetProperty("physical_injuries", out var injuries) && injuries.ValueKind != JsonValueKind.Null)
                chk_physical_injuries.IsChecked = injuries.GetBoolean();
            
            if (root.TryGetProperty("severity", out var severity) && severity.ValueKind != JsonValueKind.Null)
                chk_severity.IsChecked = severity.GetBoolean();
            
            if (root.TryGetProperty("could_occur", out var occur) && occur.ValueKind != JsonValueKind.Null)
                chk_could_occur.IsChecked = occur.GetBoolean();
            
            if (root.TryGetProperty("signature", out var sig) && sig.ValueKind != JsonValueKind.Null)
                chk_signature.IsChecked = sig.GetBoolean();
            
            if (root.TryGetProperty("deal_id", out var dealId) && dealId.TryGetInt32(out int id))
            {
                txt_deal.Tag = id;
                txt_deal.Text = id.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDraftAsync: {ex.Message}");
        }
    }
}