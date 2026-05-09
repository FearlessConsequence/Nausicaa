using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class DocumentViewerWindow : Window
{
    private readonly int? _citizenId;       
    private readonly string? _citizenFullName;
    private readonly Window? _previousWindow;
    private readonly int _currentUserId;
    
    public DocumentViewerWindow(int currentUserId, DocumentFull? document, Window? previousWindow = null, int? citizenId = null, string? citizenFullName = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
        _citizenId = citizenId;
        _citizenFullName = citizenFullName;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        btn_back.Click += (s, e) =>
        {
            try
            {
                if (_previousWindow != null)
                {
                    Console.WriteLine($"[DEBUG] Возврат в окно: {_previousWindow.GetType()}");
                    _previousWindow.Show();
                }
                else
                {
                    Console.WriteLine("[DEBUG] _previousWindow = null, открываем RecentsWindow");
                    new RecentsWindow(_currentUserId).Show();
                }
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Назад: {ex.Message}");
                NotificationsControl.ShowError("Ошибка", $"Не удалось вернуться: {ex.Message}");
            }
        };
        
        if (document != null)
        {
            LoadDocument(document);
        }
    }

    public void LoadDocument(DocumentFull doc)
    {
        txt_docType.Text = doc.DocumentType;
        txt_docNumber.Text = $"№ {doc.Number}";
        txt_docDate.Text = doc.CreatedAt.ToString("dd.MM.yyyy HH:mm");
        txt_citizenName.Text = doc.CitizenFullName;
        txt_dealNumber.Text = doc.DealNumber;
        txt_article.Text = doc.ArticleName;
        txt_officer.Text = doc.OfficerName;
        txt_description.Text = doc.Description;
        txt_otherInfo.Text = doc.OtherInformation;
        txt_firstWitness.Text = doc.FirstWitnessName;
        txt_secondWitness.Text = doc.SecondWitnessName;
        chk_signature.IsChecked = doc.SignatureForKnowing;
    }
}