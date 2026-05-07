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
    private readonly int _currentUserId;
    private readonly string _previousWindow;
    private readonly object? _previousContext; 

    public DocumentViewerWindow() : this(0, null) { }
    
    public DocumentViewerWindow(int currentUserId, DocumentFull? document, 
        string previousWindow = "Recents", object? previousContext = null, string citizenFullName = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
        _previousContext = previousContext;
        _citizenFullName = citizenFullName;
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        SetupButtons();
        
        if (document != null)
        {
            LoadDocument(document);
        }
    }

    private void SetupButtons()
    {
        btn_back.Click += (s, e) =>
        {
            switch (_previousWindow)
            {
                case "Recents":
                    new RecentsWindow(_currentUserId).Show();
                    break;
                case "Favorites":
                    new FavouritesWindow(_currentUserId).Show();
                    break;
                case "YourDocuments":
                    new YourDocumentsWindow(_currentUserId).Show();
                    break;
                case "OtherDocuments":
                    new OtherDocumentsWindow(_currentUserId).Show();
                    break;
                case "Drafts":
                    new DraftsWindow(_currentUserId).Show();
                    break;
                // ✅ Возврат в документы гражданина
                case "CitizenDocuments":
                    if (_citizenId.HasValue && !string.IsNullOrEmpty(_citizenFullName))
                    {
                        var citizenDocsWindow = new CitizenDocumentsWindow(
                            _currentUserId, 
                            _citizenId.Value, 
                            _citizenFullName);
                        citizenDocsWindow.Show();
                    }
                    else
                    {
                        new RecentsWindow(_currentUserId).Show();
                    }
                    break;
                default:
                    new RecentsWindow(_currentUserId).Show();
                    break;
            }
            Close();
        };
    }

    private void GoBack()
    {
        Window? targetWindow = _previousWindow switch
        {
            "Recents" => new RecentsWindow(_currentUserId),
            "Favorites" => new FavouritesWindow(_currentUserId),
            "YourDocuments" => new YourDocumentsWindow(_currentUserId),
            "OtherDocuments" => new OtherDocumentsWindow(_currentUserId),
            "Drafts" => new DraftsWindow(_currentUserId),
            
        
            "CitizenDocuments" => CreateCitizenDocumentWindow(),
            
        
            _ => RestoreFromContext() ?? new RecentsWindow(_currentUserId)
        };

        targetWindow.Show();
        Close();
    }


    private Window CreateCitizenDocumentWindow()
    {
    
        if (_previousContext is int citizenId)
        {
        
            return new CitizenDocumentsWindow(_currentUserId, citizenId, "Гражданин", null);
        }
    
        else if (_previousContext is Citizen citizen)
        {
            string citizenName = $"{citizen.LastName} {citizen.FirstName}".Trim();
            return new CitizenDocumentsWindow(_currentUserId, citizen.Id, citizenName, null);
        }
    
        return new CitizenDocumentsWindow(_currentUserId, 0, "Неизвестно", null);
    }
    
    private Window? RestoreFromContext()
    {
        if (_previousContext is int citizenId)
        {
            return new CitizenDocumentsWindow(_currentUserId, citizenId, "Гражданин", null);
        }
        return null;
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