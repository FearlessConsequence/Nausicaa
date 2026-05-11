using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class DocumentViewerWindow : Window
{
    private readonly int _currentUserId;
    
    // ✅ Упрощённый конструктор — убираем лишние параметры
    public DocumentViewerWindow(int currentUserId, DocumentFull? document)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        // ✅ Кнопка "Назад" просто закрывает это окно
        btn_back.Click += (s, e) => Close();
        
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