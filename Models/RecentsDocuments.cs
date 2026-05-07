using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CourseWork.Models;

public class RecentDocument : INotifyPropertyChanged
{
    public int Id { get; set; }
    public int DocumentTypeId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public int? Number { get; set; }
    
    public DateTime MakingDateAndTime { get; set; } 
    
    public string? CitizenName { get; set; }

    private string _favoriteIcon = "☆";
    public string FavoriteIcon 
    { 
        get => _favoriteIcon; 
        set { _favoriteIcon = value; OnPropertyChanged(); } 
    }

    private string _favoriteColor = "#CCCCCC";
    public string FavoriteColor 
    { 
        get => _favoriteColor; 
        set { _favoriteColor = value; OnPropertyChanged(); } 
    }

    public string DocumentName => $"{DocumentType} №{Number ?? Id}";
    
    public string LastOpenedDateFormatted => MakingDateAndTime.ToString("dd.MM.yyyy HH:mm");
    
    public string Preview => CitizenName ?? "Без данных";
    
    public string DocumentTypeIcon => DocumentType switch
    {
        "Заявление" => "📝",
        "Обращение" => "✉️",
        "Протокол объяснения" => "🗣️",
        "Направление на мед. освид." or "Направление на медицинское освидетельствование" => "🏥",
        "Административный протокол" => "📋",
        _ => "📄"
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}