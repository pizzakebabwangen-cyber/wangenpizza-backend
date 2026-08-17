namespace WangenPizza.Dtos
{
    public class UpsertOpeningHourDto
    {
        public DayOfWeek Day { get; set; }          // مثال: DayOfWeek.Monday
        public string? From1 { get; set; }          // "HH:mm" أو null
        public string? To1 { get; set; }
        public string? From2 { get; set; }
        public string? To2 { get; set; }
    }
}
