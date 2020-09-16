using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITD.PhuMyPort.DataAccess.Models
{

    public class Transection
    {
        [Key]
        /*public string TransectionId { get; set; }*/
        public string Date { get; set; }
        public string Plate { get; set; }
        public string Images { get; set; }

        public int? Result { get; set; }
        public DateTime Time { get; set; }

    }
}
