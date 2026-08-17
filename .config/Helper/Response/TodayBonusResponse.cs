using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class TodayBonusResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public IEnumerable <TodayBonus> Data { get; set; }

    }
}
