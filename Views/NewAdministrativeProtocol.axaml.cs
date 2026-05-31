#pragma warning disable CS0649
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class NewAdministrativeProtocol : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private readonly Window? _previousWindow;
    private int? _currentDraftId;
    
    public NewAdministrativeProtocol(int currentUserId, Window? previousWindow = null, int? currentDraftId = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
        _db = new DatabaseHelper();
        _currentDraftId = currentDraftId;

        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        SetupFormButtons();
    }

    private void SetupFormButtons()
    {
        btn_select_deal.Click += Btn_select_deal_Click;
        btn_select_witness1.Click += Btn_select_witness1_Click;
        btn_select_witness2.Click += Btn_select_witness2_Click;
        btn_create.Click += Btn_create_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
        btn_cancel.Click += Btn_cancel_Click;
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
                    txt_deal.Text = $"{selectedDeal.Number} - {selectedDeal.CitizenFullName}";
                    txt_deal.Tag = selectedDeal.Id;
                }
                Activate();
            });
        };
        await dealWindow.ShowDialog(this);
    }

    private async void Btn_select_witness1_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(App.CurrentUserId, this);
        citizensWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selectedCitizen = citizensWindow.SelectedCitizen;
                if (selectedCitizen != null)
                {
                    txt_witness1.Text = selectedCitizen.FullName;
                    txt_witness1.Tag = selectedCitizen.Id;
                    txt_witness1_error.IsVisible = false;
                }
                Activate();
            });
        };
        citizensWindow.Show();
    }

    private async void Btn_select_witness2_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(App.CurrentUserId, this);
        citizensWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selectedCitizen = citizensWindow.SelectedCitizen;
                if (selectedCitizen != null)
                {
                    txt_witness2.Text = selectedCitizen.FullName;
                    txt_witness2.Tag = selectedCitizen.Id;
                }
                Activate();
            });
        };
        citizensWindow.Show();
    }

    private async void Btn_create_Click(object? sender, RoutedEventArgs e)
    {
        if (txt_deal.Tag == null)
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите дело");
            return;
        }

        if (string.IsNullOrWhiteSpace(txt_protocol_number.Text))
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, заполните номер протокола");
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

        if (string.IsNullOrWhiteSpace(txt_description.Text))
        {
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, заполните описание правонарушения");
            return;
        }
        
        if (txt_witness1.Tag == null)
        {
            txt_witness1_error.IsVisible = true;
            NotificationsControl.ShowError("Ошибка", "Пожалуйста, выберите первого свидетеля");
            return;
        }
        txt_witness1_error.IsVisible = false;

        try
        {
            int dealId = (int)txt_deal.Tag;
            int protocolNumber = int.Parse(txt_protocol_number.Text);
            string description = txt_description.Text ?? "";
            string otherInfo = txt_other_info.Text ?? "";
            int witness1Id = (int)txt_witness1.Tag;
            int? witness2Id = txt_witness2.Tag as int?;
            
            DateTime selectedDateTime = dp_date.SelectedDate.Value.Date + tp_time.SelectedTime.Value;

            int newId = await _db.CreateAdministrativeProtocolAsync(dealId, protocolNumber, description, otherInfo, witness1Id, witness2Id);

            if (_currentDraftId.HasValue)
            {
                await _db.DeleteDraftAsync(_currentDraftId.Value);
            }
            
            NotificationsControl.ShowSuccess("Успех", "Административный протокол успешно создан!");
            
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
                deal = txt_deal.Tag as int?,
                deal_name = txt_deal.Text,
                protocol_number = txt_protocol_number.Text,
                making_date = dp_date.SelectedDate?.ToString("yyyy-MM-dd"),
                making_time = tp_time.SelectedTime?.ToString(),
                description = txt_description.Text,
                other_information = txt_other_info.Text,
                witness1 = txt_witness1.Tag as int?,
                witness1_name = txt_witness1.Text,
                witness2 = txt_witness2.Tag as int?,
                witness2_name = txt_witness2.Text,
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
                int newDraftId = await _db.SaveDraftAsync(_currentUserId, "administrative_protocol", formDataJson);
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
            
            if (draft.DealId.HasValue)
            {
                txt_deal.Tag = draft.DealId;
                txt_deal.Text = draft.DealId.ToString();
            }
            
            if (!string.IsNullOrWhiteSpace(draft.ProtocolNumber))
            {
                txt_protocol_number.Text = draft.ProtocolNumber;
            }
            
            if (draft.DocumentDate.HasValue)
            {
                dp_date.SelectedDate = draft.DocumentDate.Value.Date;
                tp_time.SelectedTime = new TimeSpan(draft.DocumentDate.Value.Hour, draft.DocumentDate.Value.Minute, 0);
            }
            
            if (!string.IsNullOrWhiteSpace(draft.Description))
            {
                txt_description.Text = draft.Description;
            }
            
            if (!string.IsNullOrWhiteSpace(draft.OtherInfo))
            {
                txt_other_info.Text = draft.OtherInfo;
            }
            
            if (draft.Witness1Id.HasValue)
            {
                var citizen = await _db.GetCitizenByIdAsync(draft.Witness1Id.Value);
                if (citizen != null)
                {
                    txt_witness1.Text = citizen.FullName;
                    txt_witness1.Tag = citizen.Id;
                }
            }
            
            if (draft.Witness2Id.HasValue)
            {
                var citizen = await _db.GetCitizenByIdAsync(draft.Witness2Id.Value);
                if (citizen != null)
                {
                    txt_witness2.Text = citizen.FullName;
                    txt_witness2.Tag = citizen.Id;
                }
            }
            
            if (draft.SignatureOfficer.HasValue)
            {
                chk_signature.IsChecked = draft.SignatureOfficer.Value;
            }
            
            Console.WriteLine($"[DEBUG] Загружен черновик ID: {_currentDraftId} для административного протокола");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDraftAsync: {ex.Message}");
        }
    }
}