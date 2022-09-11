using System.ComponentModel.DataAnnotations;

namespace PromocodesManagementSystem.Model
{
    public class PromoCodeGetModel
    {
        [Required]
        public string Phone { get; set; }
    }
}
