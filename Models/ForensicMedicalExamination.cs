using System;

namespace CourseWork.Models;

public class ForensicMedicalExamination
{
    public int Id { get; set; }                // id_forensic_medical_examination
    public string Number { get; set; }         // examination_number
    
    // Даты
    public DateTime ReferralDate { get; set; } // дата направления
    public DateTime? CompletionDate { get; set; } // дата завершения
    
    // Связи
    public int DealId { get; set; }            // deal (ссылка на дело)
    public int CitizenId { get; set; }         // citizen (объект экспертизы)
    public int? ExaminerId { get; set; }       // expert_id (кто проводил)
    
    // Результаты
    public string Conclusion { get; set; }     // заключение эксперта
    public string Diagnosis { get; set; }      // диагноз / выявленные повреждения
    public string Severity { get; set; }       // степень тяжести (легкая, средняя, тяжелая)
    
    // Статус
    public string Status { get; set; }         // "Назначена", "Проводится", "Готово"
}