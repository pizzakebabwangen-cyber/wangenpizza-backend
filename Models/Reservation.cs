namespace WangenPizza.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string? Salute { get; set; }
        public string? Name { get; set; }
        public string? NumberOfPeople { get; set; }   
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? RDate { get; set; }
        public string? RTime { get; set; }
        public string? Notes { get; set; }
        public bool Verified { get; set; }  // New property

    }
}
