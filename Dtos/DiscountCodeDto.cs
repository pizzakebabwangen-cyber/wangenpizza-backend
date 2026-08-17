using System.ComponentModel.DataAnnotations;
using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class DiscountCodeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public string? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }

     
    }
}
