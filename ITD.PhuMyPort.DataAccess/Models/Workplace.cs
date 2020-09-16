using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.DataAccess.Models
{
    public class Workplace
    {
        [Key]
        [Required(ErrorMessage = "Workplace code is required")]
        [MaxLength(50, ErrorMessage = "Workplace Code max length is 50 characters")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Workplace name is required")]
        [MaxLength(250, ErrorMessage = "Workplace Name max length is 250 characters")]
        public string Name { get; set; }


        public ICollection<Camera> Cameras { get; set; }
    }
}
