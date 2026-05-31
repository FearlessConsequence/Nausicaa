using System;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class NewResolution : Window
{
    private readonly int _currentUserId;
    private readonly DatabaseHelper _db;
    private int? _currentDraftId;
    private readonly Window? _previousWindow;

    public NewResolution(int currentUserId, Window? previousWindow = null, int? currentDraftId = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        _currentDraftId = currentDraftId;
        _previousWindow = previousWindow;

        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

        btn_select_deal.Click += Btn_select_deal_Click;
        btn_cancel.Click += Btn_cancel_Click;
        btn_create.Click += Btn_create_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
    }

    private void CmbPunishment_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (panel_fine == null || panel_arrest == null || panel_forced_labor == null) return;
        
        var selected = (cmb_punishment.SelectedItem as ComboBoxItem)?.Content?.ToString();
        
        panel_fine.IsVisible = selected == "Штраф";
        panel_arrest.IsVisible = selected == "Арест";
        panel_forced_labor.IsVisible = selected == "Принудительные работы";
    }

    private async void Btn_select_deal_Click(object? sender, RoutedEventArgs e)
    {
        var dealWindow = new SelectDealWindow(_currentUserId, this);
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

        if (string.IsNullOrWhiteSpace(txt_number.Text) || !int.TryParse(txt_number.Text, out int number))
        {
            NotificationsControl.ShowWarning("Внимание", "Введите корректный номер постановления (число)");
            return;
        }

        if (dp_date.SelectedDate == null || tp_time.SelectedTime == null)
        {
            NotificationsControl.ShowWarning("Внимание", "Выберите дату и время");
            return;
        }

        if (string.IsNullOrWhiteSpace(txt_resolution.Text))
        {
            NotificationsControl.ShowWarning("Внимание", "Введите текст постановления");
            return;
        }

        if (cmb_punishment.SelectedItem is not ComboBoxItem selectedPunishmentItem)
        {
            NotificationsControl.ShowWarning("Внимание", "Выберите вид наказания из списка");
            return;
        }

        int punishmentId = cmb_punishment.SelectedIndex + 1;

        try
        {
            int dealId = (int)txt_deal.Tag;
            DateTime dateTime = dp_date.SelectedDate.Value.Date + tp_time.SelectedTime.Value;
            string content = txt_resolution.Text.Trim();

            int? staffId = await _db.GetCitizensAndPostsIdByUserIdAsync(_currentUserId);
            
            if (!staffId.HasValue)
            {
                NotificationsControl.ShowError("Ошибка", "Сотрудник не найден в системе");
                return;
            }

            int? fineSum = null;
            if (panel_fine.IsVisible && int.TryParse(txt_fine.Text, out int fine))
                fineSum = fine;

            int newId = await _db.CreateResolutionAsync(
                dealId: dealId,
                protocolNumber: number,
                dateTime: dateTime,
                content: content,
                punishmentId: punishmentId,
                courtStaffId: staffId.Value,
                fineSum: fineSum
            );

            if (_currentDraftId.HasValue)
                await _db.DeleteDraftAsync(_currentDraftId.Value);

            NotificationsControl.ShowSuccess("Успех", $"Постановление №{number} создано!");
            
            // ✅ Возврат в предыдущее окно
            if (_previousWindow != null)
            {
                _previousWindow.Show();
            }
            this.Close();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка БД", $"Не удалось создать постановление: {ex.Message}");
        }
    }

    private async void Btn_save_draft_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var selectedPunishment = (cmb_punishment.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            
            var formData = new
            {
                deal_id = txt_deal.Tag as int?,
                deal_name = txt_deal.Text,
                number = txt_number.Text,
                making_date = dp_date.SelectedDate?.ToString("yyyy-MM-dd"),
                making_time = tp_time.SelectedTime?.ToString(),
                resolution = txt_resolution.Text,
                punishment = selectedPunishment,
                fine = txt_fine.Text,
                days = txt_days.Text,
                forced_labor = txt_forced_labor.Text,
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
                int newDraftId = await _db.SaveDraftAsync(_currentUserId, "resolution", formDataJson);
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
            
            if (root.TryGetProperty("resolution", out var res) && res.ValueKind != JsonValueKind.Null)
                txt_resolution.Text = res.GetString();
            
            if (root.TryGetProperty("fine", out var fine) && fine.ValueKind != JsonValueKind.Null)
                txt_fine.Text = fine.GetString();
            
            if (root.TryGetProperty("days", out var days) && days.ValueKind != JsonValueKind.Null)
                txt_days.Text = days.GetString();
            
            if (root.TryGetProperty("forced_labor", out var labor) && labor.ValueKind != JsonValueKind.Null)
                txt_forced_labor.Text = labor.GetString();
            
            if (root.TryGetProperty("signature", out var sig) && sig.ValueKind != JsonValueKind.Null)
                chk_signature.IsChecked = sig.GetBoolean();
            
            if (root.TryGetProperty("punishment", out var punishment) && punishment.ValueKind != JsonValueKind.Null)
            {
                string punishmentName = punishment.GetString();
                for (int i = 0; i < cmb_punishment.Items.Count; i++)
                {
                    if ((cmb_punishment.Items[i] as ComboBoxItem)?.Content?.ToString() == punishmentName)
                    {
                        cmb_punishment.SelectedIndex = i;
                        break;
                    }
                }
            }
            
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