using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API.Models
{
    public class Response
    {
        /// <summary>
        /// mẫ lỗi
        /// </summary>
        public int ErrorCode { get; set; }
        
        /// <summary>
        /// dữ liệu biển số trả về
        /// </summary>
        public ResponseData ResponseData { get; set; }
    }
    public class PLCResponse
    {
        /// <summary>
        /// mẫ lỗi
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// dữ liệu biển số trả về
        /// </summary>
        public string Message { get; set; }
    }
}
