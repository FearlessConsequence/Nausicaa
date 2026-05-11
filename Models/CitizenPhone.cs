namespace CourseWork.Models;

public class CitizenPhone
{
    public int Id { get; set; }           // id
    public string PhoneNumber { get; set; } = string.Empty; // phone_number
    public int CitizenId { get; set; }    // citizen (внешний ключ на citizens.id_citizens)
    public bool IsPrimary { get; set; }   // is_primary
}