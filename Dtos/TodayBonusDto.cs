using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class TodayBonusDto: TodayBonus
    {
        public IFormFile Photo { get; set; }

    }
}
