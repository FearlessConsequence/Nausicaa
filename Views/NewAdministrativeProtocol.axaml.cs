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

public partial class NewAdministrativeProtocol : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private readonly Window? _previousWindow;
    private int? _currentDraftId;

    public NewAdministrativeProtocol() : this(0) { }
    
    public NewAdministrativeProtocol(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        _currentDraftId = null;

        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
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
        var dealWindow = new SelectDealWindow(_currentUserId, _previousWindow);
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

    private async void Btn_select_witness1_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(_currentUserId, _previousWindow);
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
        var citizensWindow = new SelectCitizenWindow(_currentUserId, _previousWindow);
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
            txt_deal_error.IsVisible = true;
            await ShowMessage("Ошибка", "Пожалуйста, выберите дело");
            return;
        }
        txt_deal_error.IsVisible = false;

        if (string.IsNullOrWhiteSpace(txt_protocol_number.Text))
        {
            await ShowMessage("Ошибка", "Пожалуйста, заполните номер протокола");
            return;
        }
        
        if (dp_date.SelectedDate == null)
        {
            await ShowMessage("Ошибка", "Пожалуйста, выберите дату");
            return;
        }
        
        if (tp_time.SelectedTime == null)
        {
            await ShowMessage("Ошибка", "Пожалуйста, выберите время");
            return;
        }

        if (string.IsNullOrWhiteSpace(txt_description.Text))
        {
            await ShowMessage("Ошибка", "Пожалуйста, заполните описание правонарушения");
            return;
        }
        
        if (txt_witness1.Tag == null)
        {
            txt_witness1_error.IsVisible = true;
            await ShowMessage("Ошибка", "Пожалуйста, выберите первого свидетеля");
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

            await ShowMessage("Успех", "Административный протокол успешно создан!");
            new RecentsWindow(_currentUserId).Show();
            Close();
        }
        catch (Exception ex)
        {
            await ShowMessage("Ошибка", $"Не удалось создать протокол: {ex.Message}");
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
                await ShowMessage("Успех", "Черновик обновлён!");
                Console.WriteLine($"[DEBUG] Обновлён черновик ID: {_currentDraftId}");
            }
            else
            {
    
                int newDraftId = await _db.SaveDraftAsync(_currentUserId, "administrative_protocol", formDataJson);
                _currentDraftId = newDraftId;
                await ShowMessage("Успех", "Черновик сохранён!");
                Console.WriteLine($"[DEBUG] Создан новый черновик ID: {_currentDraftId}");
            }
        }
        catch (Exception ex)
        {
            await ShowMessage("Ошибка", $"Не удалось сохранить черновик: {ex.Message}");
        }
    }

    private void Btn_cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
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

    private async Task ShowMessage(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new Button { Content = "OK", Width = 80, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center }
                }
            }
        };
        
        var okButton = (dialog.Content as StackPanel)?.Children[1] as Button;
        if (okButton != null) okButton.Click += (s, args) => dialog.Close();
        
        await dialog.ShowDialog(this);
    }
}