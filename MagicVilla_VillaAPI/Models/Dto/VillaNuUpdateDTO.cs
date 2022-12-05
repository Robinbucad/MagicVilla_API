using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models.Dto
{
    public class VillaNuUpdateDTO
    {
        [Required]
        public int VillaNo { get; set; }
        public string SpecialDatails { get; set; }
    }
}
