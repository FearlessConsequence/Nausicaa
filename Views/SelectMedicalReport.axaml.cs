using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class SelectMedicalReport : Window
{
    private readonly DatabaseHelper _db;
    private readonly Window? _previousWindow;
    private readonly int _currentUserId;
    private List<MedicalExaminationReport> _allReports = new();
    public MedicalExaminationReport? SelectedReport { get; private set; }

    public SelectMedicalReport(int currentUserId, Window? previousWindow)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
        _db = new DatabaseHelper();

        btn_select_citizen.Click += Btn_select_citizen_Click;
        btn_search.Click += OnSearchClick;
        btn_cancel.Click += btn_cancel_click;

        WindowState = WindowState.Maximized;
    }

    private void btn_cancel_click(object? sender, RoutedEventArgs e)
    {
        this.Close();
        _previousWindow?.Show();
    }

    private async void Btn_select_citizen_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(App.CurrentUserId);
        await citizensWindow.ShowDialog(this);

        if (citizensWindow.SelectedCitizen != null)
        {
            var citizen = citizensWindow.SelectedCitizen;
            txt_citizen.Text = citizen.FullName;
            txt_citizen.Tag = citizen.Id;
        }
    }

    private async void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        await LoadReportsAsync();
    }

    private async Task LoadReportsAsync()
    {
        try
        {
            string citizenName = txt_citizen.Text?.Trim() ?? "";
            DateTime? date = dp_date.SelectedDate?.DateTime;

            _allReports = await _db.GetMedicalExaminationReportsAsync(citizenName, "0", date);
            reportsContainer.ItemsSource = _allReports;
            emptyStateBorder.IsVisible = _allReports.Count == 0;

            await Task.Delay(100);
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadReportsAsync: {ex.Message}");
        }
    }

    private void SubscribeToButtons()
    {
        var buttons = reportsContainer.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Name == "btnSelect")
            .ToList();

        foreach (var button in buttons)
        {
            button.Click -= OnSelectClick;
            button.Click += OnSelectClick;
        }
    }

    private void OnSelectClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MedicalExaminationReport report)
        {
            SelectedReport = report;
            Close();
        }
    }

    private void btnSelect_Click(object? sender, RoutedEventArgs e)
    {
        if (_previousWindow is not null)
        {


            var textBox = _previousWindow.FindControl<TextBox>("txt_examinationReport");


            if (textBox != null && SelectedReport != null)
            {
    
                textBox.Text = $"№{SelectedReport.Number} - {SelectedReport.PatientFullName}";
                textBox.Tag = SelectedReport.Id;
            }


            _previousWindow.Show();


            this.Close();
        }
    }
}