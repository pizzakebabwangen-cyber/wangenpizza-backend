using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class CompanyShortBreakViewModel
    {
        public int CompanyId { get; set; }

        // Pause History
        public string? Pausefrom { get; set; }
        public string? Pausetill { get; set; }
        public int? Pausetyp { get; set; }

        // Opening Hours
    }
}
