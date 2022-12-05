using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models.Dto
{
    public class VillaNumUpdateDTO
    {
        [Required]
        public int VillaNo { get; set; }
        public string SpecialDatails { get; set; }

    }
}
