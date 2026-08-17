using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class CompanyDataWithDaysDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Street { get; set; }
        public string? Postbox { get; set; }
        public string? City { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }

        public string? ContactPrs { get; set; }
        public string? UID { get; set; }
        public string? Bank { get; set; }
        public string? IBAN { get; set; }
        public string? OpenTime1 { get; set; }
        public string? OpenTime2 { get; set; }
        public string? OpenTime3 { get; set; }
        public string? Delivery1 { get; set; }
        public string? Delivery2 { get; set; }
        public string? Delivery3 { get; set; }
        public string? Pausefrom { get; set; }
        public string? Pausetill { get; set; }
        public int? Pausetyp { get; set; }
        public string? MondayFrom1 { get; set; }
        public string? MondayTill1 { get; set; }
        public string? MondayFrom2 { get; set; }
        public string? MondayTill2 { get; set; }

        public string? TuesdayFrom1 { get; set; }
        public string? TuesdayTill1 { get; set; }
        public string? TuesdayFrom2 { get; set; }
        public string? TuesdayTill2 { get; set; }

        public string? WednesdayFrom1 { get; set; }
        public string? WednesdayTill1 { get; set; }
        public string? WednesdayFrom2 { get; set; }
        public string? WednesdayTill2 { get; set; }

        public string? ThursdayFrom1 { get; set; }
        public string? ThursdayTill1 { get; set; }
        public string? ThursdayFrom2 { get; set; }
        public string? ThursdayTill2 { get; set; }

        public string? FridayFrom1 { get; set; }
        public string? FridayTill1 { get; set; }
        public string? FridayFrom2 { get; set; }
        public string? FridayTill2 { get; set; }

        public string? SaturdayFrom1 { get; set; }
        public string? SaturdayTill1 { get; set; }
        public string? SaturdayFrom2 { get; set; }
        public string? SaturdayTill2 { get; set; }

        public string? SundayFrom1 { get; set; }
        public string? SundayTill1 { get; set; }
        public string? SundayFrom2 { get; set; }
        public string? SundayTill2 { get; set; }
    }
}
