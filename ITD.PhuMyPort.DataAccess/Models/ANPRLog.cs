using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.DataAccess.Models
{
    public class ANPRLog
    {
        [Key]
        public string Id { get; set; }
        public DateTime Time { get; set; }
        public string FunctionName { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Duration { get; set; }
    }
}
