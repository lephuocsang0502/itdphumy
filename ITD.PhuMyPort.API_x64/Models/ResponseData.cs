using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API.Models
{
    public class ResponseData
    {
        /// <summary>
        /// biển số xe trả về
        /// </summary>
        public string Plate { get; set; }
        /// <summary>
        /// image string base64
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// image plate base64
        /// </summary>        
        public string PlateImage { get; set; }

    }
}
