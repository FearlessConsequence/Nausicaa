using System;

namespace CourseWork.Models;

public class Resolution
{
    public int Id { get; set; }                    // id_resolution
    public int Number { get; set; }                // number_protocol
    public DateTime MakingDateAndTime { get; set; } // making_date_and_time
    public int SettlementId { get; set; }          // settlements_resolution
    public int CourtStaffId { get; set; }          // court_staff (id из citizens_and_posts)
    public int? KdmEmployeeId { get; set; }        // kdm_employee (опционально)
    public string ResolutionText { get; set; }      // resolution (текст постановления)
    public int DealId { get; set; }                // deal
    public int PunishmentId { get; set; }          // punishment (id из type_of_punishment)
    public int? FineSum { get; set; }              // fine_sum
    public int? DaysOfArrest { get; set; }         // days_of_arrest
    public int? DaysOfForcedLabor { get; set; }    // days_of_forced_labor
    public int ArticleId { get; set; }             // id_article
    public int ResponsibilityId { get; set; }      // id_responsibility
}