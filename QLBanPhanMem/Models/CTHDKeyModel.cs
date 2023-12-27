
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBanPhanMem.Models
{
    [Table("CTHD_Key", Schema = "dbo")]
    public class CTHDKeyModel
    {
        [Key]
        [Required]
        public int? Id{get;set;}
        [Required]
        [Column(Order = 1)]
        public string? MAHD { get; set; }
        
        [Required]
        [Column(Order = 2)]
        public int? MAPM { get; set; }
        
        [Required]
        [Column(Order = 3)]
        public int? MAKEY { get; set; }

        [ForeignKey("MAHD")]
        public virtual HoaDonModel? HoaDon { get; set; }
        [ForeignKey("MAPM")]
        public virtual PhanMemModel? PhanMem { get; set; }

        [ForeignKey("MAKEY")]

        public virtual KEYPMModel? KEYPM { get; set; }


    }
}
