using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API.Models
{
    public class ImageData
    {
        /// <summary>
        /// thứ tự hình ảnh
        /// </summary>
        public int SequenceID { get; set; }    
        /// <summary>
        /// dữ liệu hình ảnh
        /// </summary>
        public byte[] Data { get; set; }
    }

}
