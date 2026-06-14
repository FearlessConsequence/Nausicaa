using System;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class NewMedicalCertificate : Window
{
    private readonly int _currentUserId;
    private readonly Window? _previousWindow;
    private readonly DatabaseHelper _db;
    private int? _currentDraftId;

    public NewMedicalCertificate(int currentUserId, Window? previousWindow = null, int? currentDraftId = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
        _currentDraftId = currentDraftId;
        _db = new DatabaseHelper();

        dp_date.SelectedDate = DateTime.Now;
        tp_time.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId, App.CurrentUserRole);

        btn_selectReport.Click += Btn_selectReport_Click;
        btn_create.Click += Btn_create_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
        btn_cancel.Click += Btn_cancel_Click;
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
    
    private async void Btn_create_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (txt_examinationReport.Tag == null)
            {
                NotificationsControl.ShowError("Ошибка", "Необходимо выбрать направление.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txt_number.Text))
            {
                NotificationsControl.ShowError("Ошибка", "Необходимо указать номер акта.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txt_result.Text))
            {
                NotificationsControl.ShowError("Ошибка", "Необходимо указать результат.");
                return;
            }

            int medicalReportId = (int)txt_examinationReport.Tag;
            string number = txt_number.Text.Trim();
            DateTime dateTime = dp_date.SelectedDate!.Value.Date + tp_time.SelectedTime!.Value;
            string signs = txt_signs.Text ?? "";
            string intoxicationType = ((ComboBoxItem)cmb_intoxication_type.SelectedItem!).Content?.ToString() ?? "Не выявлено";
            string result = txt_result.Text.Trim();
            bool isSigned = chk_signature.IsChecked ?? false;
            
            int medicalInstitutionId = 1;
            int doctorId = await _db.GetCitizensAndPostsIdByUserIdAsync(_currentUserId) ?? 1;

            int newId = await _db.CreateMedicalCertificateAsync(
                medicalReportId,
                number,
                dateTime,
                signs,
                intoxicationType,
                result,
                isSigned,
                medicalInstitutionId,
                doctorId
            );

            if (_currentDraftId.HasValue)
                await _db.DeleteDraftAsync(_currentDraftId.Value);

            NotificationsControl.ShowSuccess("Успех", "Акт медицинского освидетельствования успешно создан!");
            
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
            NotificationsControl.ShowError("Ошибка", $"Не удалось создать акт: {ex.Message}");
        }
    }

    private async void Btn_save_draft_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var formData = new
            {
                examination_report_id = txt_examinationReport.Tag as int?,
                examination_report_name = txt_examinationReport.Text,
                number = txt_number.Text,
                making_date = dp_date.SelectedDate?.ToString("yyyy-MM-dd"),
                making_time = tp_time.SelectedTime?.ToString(),
                signs = txt_signs.Text,
                intoxication_type = ((ComboBoxItem)cmb_intoxication_type.SelectedItem)?.Content?.ToString(),
                result = txt_result.Text,
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
                int newDraftId = await _db.SaveDraftAsync(_currentUserId, "medical_certificate", formDataJson);
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
            
            if (root.TryGetProperty("signs", out var signs) && signs.ValueKind != JsonValueKind.Null)
                txt_signs.Text = signs.GetString();
            
            if (root.TryGetProperty("result", out var res) && res.ValueKind != JsonValueKind.Null)
                txt_result.Text = res.GetString();
            
            if (root.TryGetProperty("signature", out var sig) && sig.ValueKind != JsonValueKind.Null)
                chk_signature.IsChecked = sig.GetBoolean();
            
            if (root.TryGetProperty("intoxication_type", out var type) && type.ValueKind != JsonValueKind.Null)
            {
                string typeName = type.GetString();
                for (int i = 0; i < cmb_intoxication_type.Items.Count; i++)
                {
                    if ((cmb_intoxication_type.Items[i] as ComboBoxItem)?.Content?.ToString() == typeName)
                    {
                        cmb_intoxication_type.SelectedIndex = i;
                        break;
                    }
                }
            }
            
            if (root.TryGetProperty("examination_report_id", out var reportId) && reportId.TryGetInt32(out int id))
            {
                txt_examinationReport.Tag = id;
                txt_examinationReport.Text = id.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDraftAsync: {ex.Message}");
        }
    }

    private void Btn_selectReport_Click(object? sender, RoutedEventArgs e)
    {
        var selectWindow = new SelectMedicalReport(_currentUserId, this);
        selectWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selected = selectWindow.SelectedReport;
                if (selected != null)
                {
                    txt_examinationReport.Text = $"№{selected.Number} - {selected.PatientFullName}";
                    txt_examinationReport.Tag = selected.Id;
                }
                Activate();
            });
        };
        selectWindow.Show();
        this.Hide();
    }
}