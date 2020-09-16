using ITD.PhuMyPort.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API.Services
{
    public class PLCServices
    {
        /// <summary>
        /// hàng đợi lệnh request mở barrier
        /// </summary>
        public Queue<PLCData> requuestQueue { get; set; }// = new Queue<PLCData>();
        /// <summary>
        /// danh sách kết quả xử lý
        /// </summary>
        public Dictionary<int, PLCData> responseQueue { get; set; }// = new Dictionary<int, PLCData>();
        public PLCServices()
        {
            requuestQueue = new Queue<PLCData>();
            responseQueue = new Dictionary<int, PLCData>();
        }
    }
}
