using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ITD.PhuMyPort.DataAccess.Models
{
    public class PLC
    {
        [Key]
        [Required(ErrorMessage = "PLC code is required")]
        [MaxLength(50, ErrorMessage = "PLC Code max length is 50 characters")]
        public string Code { get; set; }

        [Required(ErrorMessage = "IP is required")]
        [MaxLength(250, ErrorMessage = "IP max length is 250 characters")]
        public string IP { get; set; }

        [Required(ErrorMessage = "Barrier is required")]
        [MaxLength(10, ErrorMessage = "Barrier max length is 10 characters")]
        public string Barrier { get; set; } 
        //ForeignKey
        public string WorkplaceCode { get; set; }
        [ForeignKey("WorkplaceCode")]
        public Workplace Workplace { get; set; }
    }
}
