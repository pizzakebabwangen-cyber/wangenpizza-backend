namespace WangenPizza.Dtos
{
    public class ShortBreakDto
    {
        public int Id { get; set; }   // CompanyId

        // بيانات البريك
        public string Pausefrom { get; set; }   // مثلا 20.09.2025
        public string Pausetill { get; set; }   // مثلا 25.09.2025
        public int Pausetyp { get; set; }       // 1, 2, 3

        // Opening Hours (كلهم hh:mm)
        public string MondayFrom1 { get; set; }
        public string MondayTill1 { get; set; }
        public string MondayFrom2 { get; set; }
        public string MondayTill2 { get; set; }

        public string TuesdayFrom1 { get; set; }
        public string TuesdayTill1 { get; set; }
        public string TuesdayFrom2 { get; set; }
        public string TuesdayTill2 { get; set; }

        public string WednesdayFrom1 { get; set; }
        public string WednesdayTill1 { get; set; }
        public string WednesdayFrom2 { get; set; }
        public string WednesdayTill2 { get; set; }

        public string ThursdayFrom1 { get; set; }
        public string ThursdayTill1 { get; set; }
        public string ThursdayFrom2 { get; set; }
        public string ThursdayTill2 { get; set; }

        public string FridayFrom1 { get; set; }
        public string FridayTill1 { get; set; }
        public string FridayFrom2 { get; set; }
        public string FridayTill2 { get; set; }

        public string SaturdayFrom1 { get; set; }
        public string SaturdayTill1 { get; set; }
        public string SaturdayFrom2 { get; set; }
        public string SaturdayTill2 { get; set; }

        public string SundayFrom1 { get; set; }
        public string SundayTill1 { get; set; }
        public string SundayFrom2 { get; set; }
        public string SundayTill2 { get; set; }
    }

}
